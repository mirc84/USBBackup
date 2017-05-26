using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using USBBackup.Entities;

namespace USBBackup
{
    public delegate void BackupHandledEventHandler(IBackup backup);
    public delegate void BackupStateChangedEventHandler(BackupState state);

    public class BackupHandler
    {
        private Dictionary<IBackup, Task> _tasks;
        private Dictionary<IBackup, CancellationTokenSource> _backupCancellationTokens;
        private Dictionary<IBackup, CancellationTokenSource> _backupPauseCancellationTokens;
        private volatile Dictionary<IBackup, ManualResetEvent> _backupResetEvents;
        private BackupState _state;

        public BackupHandler()
        {
            _tasks = new Dictionary<IBackup, Task>();
            _backupCancellationTokens = new Dictionary<IBackup, CancellationTokenSource>();
            _backupResetEvents = new Dictionary<IBackup, ManualResetEvent>();
            _backupPauseCancellationTokens = new Dictionary<IBackup, CancellationTokenSource>();
        }

        public event BackupHandledEventHandler BackupStarted;
        public event BackupHandledEventHandler BackupFinished;
        public event BackupHandledEventHandler CleanupStarted;
        public event BackupHandledEventHandler CleanupFinished;
        public event BackupStateChangedEventHandler StateChanged;

        public void HandleBackup(DriveNotificationWrapper existingDevice)
        {
            foreach (var backup in existingDevice.Backups)
            {
                HandleBackup(backup);
            }
        }

        public void HandleBackup(Drive drive)
        {
            foreach (var backup in drive.Backups)
            {
                HandleBackup(backup);
            }
        }
        
        public void HandleBackup(IBackup backup, bool force = false)
        {
            if ((!backup.IsEnabled && !force) || backup.IsRunning)
                return;

            PrepareBackup(backup, out CancellationTokenSource token, out ManualResetEvent pauseEvent, out Task task);

            task = task.ContinueWith(t =>
            {
                try
                {
                    backup.IsRunning = true;
                    OnBackupStarted(backup);
                    var dir = new DirectoryInfo(backup.SourcePath);
                    if (!dir.Exists)
                        return;

                    backup.CurrentFile = "Analysing";

                    var files = GetUpdatedBackupFiles(backup);
                    backup.BytesToWrite = files.Keys.Sum(x => x.Length);
                    backup.WrittenBytes = 0L;
                    backup.FinishedBytes = 0L;
                    backup.CurrentFileBytes = 0L;
                    backup.CurrentFileWrittenBytes = 0L;
                    var pauseCancellationToken = new CancellationTokenSource();
                    _backupPauseCancellationTokens[backup] = pauseCancellationToken;
                    foreach (var backupFilePair in files)
                    {
                        try
                        {
                            pauseEvent.WaitOne();
                            backup.IsPaused = false;
                            if (token.Token.IsCancellationRequested)
                                return;

                            var fileInfo = backupFilePair.Key;
                            var targetFileInfo = backupFilePair.Value;

                            backup.CurrentFile = ".." + fileInfo.FullName.Substring(backup.SourcePath.Length);

                            var bakPath = targetFileInfo.FullName + ".bak";
                            if (File.Exists(bakPath))
                                File.Delete(bakPath);
                            if (!targetFileInfo.Directory.Exists)
                                targetFileInfo.Directory.Create();

                            using (var fileStream = new FileStream(fileInfo.FullName, FileMode.Open))
                            {
                                backup.CurrentFileBytes = fileStream.Length;
                                using (var targetFileStream = new FileStream(bakPath, FileMode.Create))
                                {
                                    while (!token.Token.IsCancellationRequested && fileStream.Position < fileStream.Length)
                                    {
                                        try
                                        {
                                            if (pauseCancellationToken.IsCancellationRequested)
                                            {
                                                pauseCancellationToken = new CancellationTokenSource();
                                                _backupPauseCancellationTokens[backup] = pauseCancellationToken;
                                            }
                                            var copyTask = fileStream.CopyToAsync(targetFileStream, 81920,
                                                pauseCancellationToken.Token);

                                            while (!copyTask.IsCompleted)
                                            {
                                                backup.WrittenBytes = backup.FinishedBytes + fileStream.Position;
                                                backup.CurrentFileWrittenBytes = fileStream.Position;
                                                copyTask.Wait(10);
                                            }
                                            targetFileStream.Flush();
                                            if (fileStream.Position >= fileStream.Length)
                                                break;
                                        }
                                        catch (AggregateException ex)
                                        {
                                            if (!(ex.InnerException is TaskCanceledException))
                                                throw;
                                        }

                                        pauseEvent.WaitOne();
                                    }
                                }
                                backup.FinishedBytes += fileStream.Length;
                            }

                            if (File.Exists(targetFileInfo.FullName))
                                File.Delete(targetFileInfo.FullName);

                            File.Move(bakPath, targetFileInfo.FullName);
                            targetFileInfo.CreationTime = fileInfo.CreationTime;
                            targetFileInfo.LastWriteTime = fileInfo.LastWriteTime;

                            backup.CurrentFileBytes = 0L;
                            backup.CurrentFileWrittenBytes = 0L;
                        }
                        catch (Exception e)
                        {
                            Debugger.Break();
                        }
                    }
                    _backupPauseCancellationTokens.Remove(backup);
                }
                catch (Exception e)
                {

                }
                finally
                {
                    backup.CurrentFile = null;
                    backup.IsRunning = false;
                    backup.IsPaused = false;
                    backup.BytesToWrite = 0L;
                    backup.WrittenBytes = 0L;
                    backup.FinishedBytes = 0L;
                    backup.CurrentFileBytes = 0L;
                    backup.CurrentFileWrittenBytes = 0L;
                    OnBackupFinished(backup);
                }
            }, token.Token);
            _tasks[backup] = task;

            if (USBBackup.Properties.Settings.Default.CleanupRemovedFile)
                RecycleBackupFiles(backup);
        }

        private IDictionary<FileInfo, FileInfo> GetUpdatedBackupFiles(IBackup backup)
        {
            var updatedFiles = new Dictionary<FileInfo, FileInfo>();

            var dir = new DirectoryInfo(backup.SourcePath);
            if (!dir.Exists)
                return updatedFiles;

            var targetDir = new DirectoryInfo(backup.SourcePath);
            if (!targetDir.Exists)
                targetDir.Create();

            foreach (var fileInfo in dir.EnumerateFiles("*.*", SearchOption.AllDirectories))
            {
                var relativePath = fileInfo.FullName.Substring(backup.SourcePath.Length);
                var targetFileInfo = new FileInfo(string.Concat(backup.TargetPath, relativePath));

                if (targetFileInfo.Exists && fileInfo.LastWriteTime == targetFileInfo.LastWriteTime)
                    continue;

                updatedFiles.Add(fileInfo, targetFileInfo);
            }
            return updatedFiles;
        }

        private void PrepareBackup(IBackup backup, out CancellationTokenSource token, out ManualResetEvent pauseEvent, out Task task)
        {
            if (!_backupCancellationTokens.TryGetValue(backup, out token) || token.Token.IsCancellationRequested)
            {
                token = new CancellationTokenSource();
                _backupCancellationTokens[backup] = token;
            }

            if (!_backupResetEvents.TryGetValue(backup, out pauseEvent))
            {
                pauseEvent = new ManualResetEvent(false);
                _backupResetEvents[backup] = pauseEvent;
            }
            pauseEvent.Set();

            if (!_tasks.TryGetValue(backup, out task))
            {
                task = Task.Factory.StartNew(() => { backup.IsRunning = false; });
            }
        }

        public void RecycleBackupFiles(IBackup backup)
        {
            if (!backup.IsEnabled)
                return;

            PrepareBackup(backup, out CancellationTokenSource token, out ManualResetEvent pauseEvent, out Task task);
            
            task = task.ContinueWith(t =>
            {
                var directory = new DirectoryInfo(backup.TargetPath);
                if (!directory.Exists)
                    return;
                try
                {
                    backup.IsRunning = true;
                    OnCleanupStarted(backup);
                    var recycleBin = Path.Combine(backup.TargetPath, ".recyclebin");
                    foreach (var fileInfo in directory.EnumerateFiles("*.*", SearchOption.AllDirectories))
                    {
                        if (fileInfo.FullName.StartsWith(recycleBin))
                            continue;
                        //if (backup.IsPaused)
                        {
                            pauseEvent.WaitOne();
                        }
                        backup.IsPaused = false;
                        if (token.Token.IsCancellationRequested)
                            return;

                        try
                        {
                            var relativePath = fileInfo.FullName.Substring(backup.TargetPath.Length);
                            if (!Directory.Exists(backup.SourcePath))
                                return;
                            var sourcePath = string.Concat(backup.SourcePath, relativePath);
                            if (File.Exists(sourcePath))
                                continue;


                            var recycledFilePath = string.Concat(recycleBin, relativePath);
                            var recycleFileInfo = new FileInfo(recycledFilePath);
                            if (recycleFileInfo.Exists)
                            {
                                if (recycleFileInfo.LastWriteTime == fileInfo.LastWriteTime)
                                {
                                    fileInfo.Delete();
                                    continue;
                                }
                                recycleFileInfo.Delete();
                            }

                            if (!recycleFileInfo.Directory.Exists)
                                recycleFileInfo.Directory.Create();

                            File.Move(fileInfo.FullName, recycleFileInfo.FullName);
                        }
                        catch (Exception)
                        {

                        }
                    }
                }
                catch (Exception)
                {

                }
                finally
                {
                    backup.CurrentFile = null;
                    backup.IsRunning = false;
                    backup.IsPaused = false;
                    OnCleanupFinished(backup);
                }
            }
            , token.Token);

            _tasks[backup] = task;
        }

        public void HandleBackup(Backup backup, string changedPath)
        {
            Task runningTask;
            _tasks.TryGetValue(backup, out runningTask);
            if (runningTask == null)
                runningTask = Task.Factory.StartNew(() => { });

            CancellationTokenSource token;
            if (!_backupCancellationTokens.TryGetValue(backup, out token))
            {
                token = new CancellationTokenSource();
                _backupCancellationTokens.Add(backup, token);
            }

            var task = runningTask.ContinueWith(t =>
            {
                try
                {
                    OnBackupStarted(backup);
                    backup.IsRunning = true;
                    var fileInfo = new FileInfo(changedPath);
                    if (!fileInfo.Exists)
                        return;

                    var targetDir = new DirectoryInfo(backup.SourcePath);
                    if (!targetDir.Exists)
                        targetDir.Create();

                    try
                    {
                        var relativePath = changedPath.Substring(backup.SourcePath.Length);
                        backup.CurrentFile = ".." + relativePath;
                        var targetFileInfo = new FileInfo(string.Concat(backup.TargetPath, relativePath));

                        if (targetFileInfo.Exists && fileInfo.LastWriteTime == targetFileInfo.LastWriteTime)
                            return;

                        if (!targetFileInfo.Directory.Exists)
                            targetFileInfo.Directory.Create();

                        var targetPath = targetFileInfo.FullName;
                        var bakPath = targetPath + ".bak";
                        if (File.Exists(bakPath))
                            File.Delete(bakPath);

                        File.Copy(fileInfo.FullName, bakPath);

                        if (File.Exists(targetPath))
                            File.Delete(targetPath);

                        File.Move(targetPath + ".bak", targetPath);
                    }
                    catch (Exception e)
                    {
                        Debugger.Break();
                    }
                }
                catch (Exception e)
                {

                }
                finally
                {
                    backup.CurrentFile = null;
                    backup.IsRunning = false;
                    OnBackupFinished(backup);
                }
            }, token.Token);

            _tasks[backup] = task;
        }

        public void PauseResumeBackups()
        {
            switch (_state)
            {
                case BackupState.Idle:
                    return;
                case BackupState.Running:
                    PauseBackups();
                    break;
                case BackupState.Paused:
                    ResumeBackups();
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Backup state {_state} unknown.");
            }
        }

        public void PauseBackups()
        {
            foreach (var pauseEvent in _backupResetEvents)
            {
                pauseEvent.Key.IsPaused = true;
                pauseEvent.Value.Reset();
            }
            OnStateChanged();
        }

        public void PauseBackup(IBackup backup)
        {
            _backupResetEvents.TryGetValue(backup, out ManualResetEvent pauseEvent);
            pauseEvent?.Reset();
            _backupPauseCancellationTokens.TryGetValue(backup, out CancellationTokenSource pauseCancellationToken);
            pauseCancellationToken?.Cancel();
            backup.IsPaused = true;
            OnStateChanged();
        }

        public void ResumeBackups()
        {
            foreach (var pauseEvent in _backupResetEvents)
            {
                pauseEvent.Key.IsPaused = false;
                pauseEvent.Value.Set();
            }
            OnStateChanged();
        }

        public void ResumeBackup(IBackup backup)
        {
            backup.IsPaused = false;
            _backupResetEvents.TryGetValue(backup, out ManualResetEvent pauseEvent);
            pauseEvent?.Set();
            OnStateChanged();
        }

        public void CancelBackup(IBackup backup, bool hardCancel)
        {
            _backupCancellationTokens.TryGetValue(backup, out CancellationTokenSource cancellationToken);
            cancellationToken?.Cancel();
            _backupResetEvents.TryGetValue(backup, out ManualResetEvent pauseEvent);
            pauseEvent?.Set();
            if (!hardCancel)
                return;

            _backupPauseCancellationTokens.TryGetValue(backup, out CancellationTokenSource pauseCancellationToken);
            pauseCancellationToken.Cancel();
        }

        public void CancelBackups(bool hardCancel)
        {
            foreach (var token in _backupCancellationTokens.Values)
                token.Cancel();

            if (hardCancel)
            {
                foreach (var token in _backupPauseCancellationTokens.Values)
                {
                    token.Cancel();
                }
            }

            foreach (var pauseEvent in _backupResetEvents.Values)
                pauseEvent.Set();
            foreach (var task in _tasks.Values.ToList())
                task.Wait();

            OnStateChanged();
        }

        private void OnBackupStarted(IBackup backup)
        {
            BackupStarted?.Invoke(backup);
            OnStateChanged();
        }

        private void OnBackupFinished(IBackup backup)
        {
            BackupFinished?.Invoke(backup);
            OnStateChanged();
        }

        private void OnCleanupStarted(IBackup backup)
        {
            CleanupStarted?.Invoke(backup);
            OnStateChanged();
        }

        private void OnCleanupFinished(IBackup backup)
        {
            CleanupFinished?.Invoke(backup);
            OnStateChanged();
        }

        private void OnStateChanged()
        {
            _state = default(BackupState);
            if (_tasks.All(x => x.Key.IsPaused))
                _state = BackupState.Paused;
            else if (_tasks.Any(x => x.Key.IsRunning))
                _state = BackupState.Running;
            StateChanged?.Invoke(_state);
        }
    }
}