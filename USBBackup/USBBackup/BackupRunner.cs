using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using USBBackup.Strings;

namespace USBBackup
{
    public class BackupRunner
    {
        #region Constructor

        public BackupRunner(IBackup backup)
        {
            Backup = backup;
            CancellationTokenSource = new CancellationTokenSource();
            PauseWaitHandle = new ManualResetEvent(false);
            Task = Task.Factory.StartNew(() => { });
        }

        #endregion

        #region Properties

        public IBackup Backup { get; }
        public EventWaitHandle PauseWaitHandle { get; }
        public CancellationTokenSource CancellationTokenSource { get; private set; }
        public CancellationTokenSource PauseCancellationTokenSource { get; private set; }
        public Task Task { get; private set; }

        #endregion

        #region Events

        public event BackupHandledEventHandler BackupStarted;
        public event BackupHandledEventHandler BackupFinished;
        public event BackupHandledEventHandler CleanupStarted;
        public event BackupHandledEventHandler CleanupFinished;

        #endregion

        #region Public Methods

        public void AppendExecuteBackup()
        {
            if (CancellationTokenSource.Token.IsCancellationRequested)
                CancellationTokenSource = new CancellationTokenSource();

            Task = Task.ContinueWith(_ => ExecuteBackup(CancellationTokenSource.Token), CancellationTokenSource.Token);
        }

        public void AppendRecycleExecute()
        {
            if (CancellationTokenSource.Token.IsCancellationRequested)
                CancellationTokenSource = new CancellationTokenSource();

            Task = Task.ContinueWith(_ => RecycleExecute(CancellationTokenSource.Token), CancellationTokenSource.Token);
        }

        public void AppendHandleChangedPath(string changedPath)
        {
            if (CancellationTokenSource.Token.IsCancellationRequested)
                CancellationTokenSource = new CancellationTokenSource();

            Task = Task.ContinueWith(t => { HandleChangedPath(changedPath); }, CancellationTokenSource.Token);
        }

        #endregion

        #region Non Public Methods

        private void ExecuteBackup(CancellationToken cancellationToken)
        {
            try
            {
                Log.Backup.Info($"Starting backup from '{Backup.SourcePath}' to '{Backup.TargetPath}'");
                Backup.IsRunning = true;
                var dir = new DirectoryInfo(Backup.SourcePath);
                if (!dir.Exists)
                    return;

                Backup.CurrentFile = new Loc(nameof(StringResource.Backup_Analyzing));

                var files = GetUpdatedBackupFiles(Backup);
                if (!files.Any())
                    return;

                OnBackupStarted(Backup);

                Backup.BytesToWrite = files.Keys.Sum(x => x.Length);
                Backup.WrittenBytes = 0L;
                Backup.FinishedBytes = 0L;
                Backup.CurrentFileBytes = 0L;
                Backup.CurrentFileWrittenBytes = 0L;

                CopyBackupFiles(files, cancellationToken);

                Log.Backup.Info($"Finished backup from '{Backup.SourcePath}' to '{Backup.TargetPath}'");
            }
            catch (Exception e)
            {
                Log.Backup.Error(e,
                    $"An error occurred during backup from '{Backup.SourcePath}' to '{Backup.TargetPath}'.");
            }
            finally
            {
                Backup.CurrentFile = null;
                Backup.IsRunning = false;
                Backup.IsPaused = false;
                Backup.BytesToWrite = 0L;
                Backup.WrittenBytes = 0L;
                Backup.FinishedBytes = 0L;
                Backup.CurrentFileBytes = 0L;
                Backup.CurrentFileWrittenBytes = 0L;
                OnBackupFinished(Backup);
            }
        }

        private void CopyBackupFiles(IDictionary<FileInfo, FileInfo> files, CancellationToken cancellationToken)
        {
            PauseCancellationTokenSource = new CancellationTokenSource();
            foreach (var backupFilePair in files)
            {
                try
                {
                    if (cancellationToken.IsCancellationRequested)
                        return;

                    PauseWaitHandle.WaitOne();
                    Backup.IsPaused = false;
                    if (cancellationToken.IsCancellationRequested)
                        return;

                    var fileInfo = backupFilePair.Key;
                    var targetFileInfo = backupFilePair.Value;

                    Backup.CurrentFile = ".." + fileInfo.FullName.Substring(Backup.SourcePath.Length);

                    var bakPath = targetFileInfo.FullName + ".bak";
                    if (File.Exists(bakPath))
                        File.Delete(bakPath);
                    if (!targetFileInfo.Directory.Exists)
                        targetFileInfo.Directory.Create();

                    var isCopyFileCompleted = CopyFile(fileInfo, bakPath, cancellationToken);
                    if (isCopyFileCompleted)
                    {
                        if (File.Exists(targetFileInfo.FullName))
                            File.Delete(targetFileInfo.FullName);

                        File.Move(bakPath, targetFileInfo.FullName);
                        targetFileInfo.CreationTime = fileInfo.CreationTime;
                        targetFileInfo.LastWriteTime = fileInfo.LastWriteTime;
                    }
                    else
                    {
                        File.Delete(bakPath);
                    }
                    Backup.CurrentFileBytes = 0L;
                    Backup.CurrentFileWrittenBytes = 0L;
                }
                catch (Exception e)
                {
                    Log.Backup.Error(e,
                        $"An error occurred during backing up '{backupFilePair.Key.FullName}'.");
                }
            }
            PauseCancellationTokenSource = null;
        }

        private bool CopyFile(FileSystemInfo fileInfo, string bakPath, CancellationToken cancellationToken)
        {
            var isCopyFileCompleted = false;
            using (var fileStream = new FileStream(fileInfo.FullName, FileMode.Open))
            {
                Backup.CurrentFileBytes = fileStream.Length;
                using (var targetFileStream = new FileStream(bakPath, FileMode.Create))
                {
                    while (!cancellationToken.IsCancellationRequested &&
                           fileStream.Position < fileStream.Length)
                    {
                        try
                        {
                            if (PauseCancellationTokenSource.Token.IsCancellationRequested)
                            {
                                PauseCancellationTokenSource = new CancellationTokenSource();
                            }
                            var copyTask = fileStream.CopyToAsync(targetFileStream, 81920,
                                PauseCancellationTokenSource.Token);

                            while (!copyTask.IsCompleted)
                            {
                                Backup.WrittenBytes = Backup.FinishedBytes + fileStream.Position;
                                Backup.CurrentFileWrittenBytes = fileStream.Position;
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
                        if (!cancellationToken.IsCancellationRequested)
                            PauseWaitHandle.WaitOne();
                    }
                }
                isCopyFileCompleted = fileStream.Position == fileStream.Length;
                Backup.FinishedBytes += fileStream.Length;
            }
            return isCopyFileCompleted;
        }

        private void RecycleExecute(CancellationToken cancellationToken)
        {
            var directory = new DirectoryInfo(Backup.TargetPath);
            if (!directory.Exists)
                return;
            try
            {
                Backup.IsRunning = true;
                OnCleanupStarted(Backup);
                var recycleBin = Path.Combine(Backup.TargetPath, ".recyclebin");
                foreach (var fileInfo in directory.EnumerateFiles("*.*", SearchOption.AllDirectories))
                {
                    if (fileInfo.FullName.StartsWith(recycleBin))
                        continue;

                    PauseWaitHandle.WaitOne();
                    Backup.IsPaused = false;
                    if (cancellationToken.IsCancellationRequested)
                        return;

                    try
                    {
                        var relativePath = fileInfo.FullName.Substring(Backup.TargetPath.Length);
                        if (!Directory.Exists(Backup.SourcePath))
                            return;
                        var sourcePath = string.Concat(Backup.SourcePath, relativePath);
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
                    catch (Exception e)
                    {
                        Log.Backup.Error(e, $"An error occurred while recylcing '{fileInfo.FullName}'.");
                    }
                }
            }
            catch (Exception e)
            {
                Log.Backup.Error(e, $"An error occurred during cleanup of '{Backup.TargetPath}'.");
            }
            finally
            {
                Backup.CurrentFile = null;
                Backup.IsRunning = false;
                Backup.IsPaused = false;
                OnCleanupFinished(Backup);
            }
        }

        private void HandleChangedPath(string changedPath)
        {
            try
            {
                OnBackupStarted(Backup);
                Backup.IsRunning = true;
                var fileInfo = new FileInfo(changedPath);
                if (!fileInfo.Exists)
                    return;

                var targetDir = new DirectoryInfo(Backup.SourcePath);
                if (!targetDir.Exists)
                    targetDir.Create();

                var relativePath = changedPath.Substring(Backup.SourcePath.Length);
                Backup.CurrentFile = ".." + relativePath;
                var targetFileInfo = new FileInfo(string.Concat(Backup.TargetPath, relativePath));

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
                Log.Backup.Error(e,
                    $"An error occurred while applying change of '{changedPath}' to '{Backup.TargetPath}'.");
            }
            finally
            {
                Backup.CurrentFile = null;
                Backup.IsRunning = false;
                OnBackupFinished(Backup);
            }
        }

        private static IDictionary<FileInfo, FileInfo> GetUpdatedBackupFiles(IBackup backup)
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

        protected virtual void OnBackupStarted(IBackup backup)
        {
            BackupStarted?.Invoke(backup);
        }

        protected virtual void OnBackupFinished(IBackup backup)
        {
            BackupFinished?.Invoke(backup);
        }

        protected virtual void OnCleanupStarted(IBackup backup)
        {
            CleanupStarted?.Invoke(backup);
        }

        protected virtual void OnCleanupFinished(IBackup backup)
        {
            CleanupFinished?.Invoke(backup);
        }

        #endregion
    }
}