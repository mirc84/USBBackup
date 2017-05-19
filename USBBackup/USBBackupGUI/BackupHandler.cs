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
    internal class BackupHandler
    {
        private Dictionary<IBackup, Task> _tasks;
        private Dictionary<IBackup, CancellationTokenSource> _backupCancellationTokens;

        public BackupHandler()
        {
            _tasks = new Dictionary<IBackup, Task>();
            _backupCancellationTokens = new Dictionary<IBackup, CancellationTokenSource>();
        }
        
        public void HandleBackup(DriveNotificationWrapper existingDevice)
        {
            foreach (var backup in existingDevice.Backups)
            {
                HandleBackup(backup);
            }
        }    

        internal void HandleBackup(Drive drive)
        {
            foreach (var backup in drive.Backups)
            {
                HandleBackup(backup);
            }
        }
        
        public void HandleBackup(IBackup backup)
        {
            if (!backup.IsEnabled || backup.IsRunning || _tasks.ContainsKey(backup))
                return;

            CancellationTokenSource token;
            if (!_backupCancellationTokens.TryGetValue(backup, out token) || token.Token.IsCancellationRequested)
            {
                token = new CancellationTokenSource();
                _backupCancellationTokens[backup] = token;
            }

            Task task;
            if (!_tasks.TryGetValue(backup, out task))
            {
                task = Task.Factory.StartNew(() => { });
            }

            task = task.ContinueWith(t =>
            {
                try
                {
                    backup.IsRunning = true;
                    var dir = new DirectoryInfo(backup.SourcePath);
                    if (!dir.Exists)
                        return;

                    var targetDir = new DirectoryInfo(backup.SourcePath);
                    if (!targetDir.Exists)
                        targetDir.Create();

                    foreach (var fileInfo in dir.EnumerateFiles("*.*", SearchOption.AllDirectories))
                    {
                        if (token.Token.IsCancellationRequested)
                            return;

                        try
                        {
                            var relativePath = fileInfo.FullName.Substring(backup.SourcePath.Length);
                            var targetFileInfo = new FileInfo(string.Concat(backup.TargetPath, relativePath));

                            if (!targetFileInfo.Directory.Exists)
                                targetFileInfo.Directory.Create();

                            if (targetFileInfo.Exists && fileInfo.LastWriteTime == targetFileInfo.LastWriteTime)
                                continue;

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
                }
                catch (Exception e)
                {

                }
                finally
                {
                    backup.IsRunning = false;
                }
            }, token.Token);
            _tasks[backup] = task;
        }

        public void RecycleBackupFiles(IBackup backup)
        {
            var token = new CancellationTokenSource();
            if (!_backupCancellationTokens.TryGetValue(backup, out token) || token.Token.IsCancellationRequested)
            {
                token = new CancellationTokenSource();
                _backupCancellationTokens[backup] = token;
            }

            Task task;
            if (!_tasks.TryGetValue(backup, out task))
            {
                task = Task.Factory.StartNew(() => { });
            }
            task = task.ContinueWith(t =>
            {
                var directory = new DirectoryInfo(backup.TargetPath);
                if (!directory.Exists)
                    return;

                foreach (var fileInfo in directory.EnumerateFiles("*.*", SearchOption.AllDirectories))
                {
                    var relativePath = fileInfo.FullName.Substring(backup.TargetPath.Length);
                    if (!Directory.Exists(backup.SourcePath))
                        return;
                    var sourcePath = string.Concat(backup.SourcePath, relativePath);
                    if (File.Exists(sourcePath))
                        continue;

                    var recycleBin = Path.Combine(backup.TargetPath, ".recyclebin");
                    if (Directory.Exists(recycleBin))
                        Directory.CreateDirectory(recycleBin);

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
                    File.Move(fileInfo.FullName, recycleFileInfo.FullName);
                }
            }
            , token.Token);

            _tasks[backup] = task;
        }

        internal void HandleBackup(Backup backup, string changedPath)
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
                    backup.IsRunning = false;
                }
            }, token.Token);

            _tasks[backup] = task;
        }

        internal void CancelBackup(BackupNotificationWrapper backup)
        {
            CancellationTokenSource cancellationToken;
            if (!_backupCancellationTokens.TryGetValue(backup, out cancellationToken))
                return;

            cancellationToken.Cancel();
        }

        internal void CancelBackups()
        {
            foreach (var token in _backupCancellationTokens.Values)
                token.Cancel();

            foreach (var task in _tasks.Values.ToList())
                task.Wait();
        }
    }
}