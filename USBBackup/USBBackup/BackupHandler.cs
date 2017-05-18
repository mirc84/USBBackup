using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Threading;
using System.Threading.Tasks;
using USBBackup.Entities;

namespace USBBackup
{
    internal class BackupHandler
    {
        private CancellationTokenSource _cancellationToken;
        private Dictionary<IBackup, Task> _tasks;
        private Dictionary<IBackup, CancellationTokenSource> _backupCancellationTokens;

        public BackupHandler()
        {
            _cancellationToken = new CancellationTokenSource();
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

            var token = new CancellationTokenSource();
            _backupCancellationTokens[backup] = token;
            var task = Task.Factory.StartNew(() =>
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

                    foreach (var fileInfo in dir.EnumerateFiles())
                    {
                        if (_cancellationToken.IsCancellationRequested || token.Token.IsCancellationRequested)
                            return;

                        try
                        {
                            var relativePath = fileInfo.FullName.Substring(backup.SourcePath.Length);
                            var targetFileInfo = new FileInfo(string.Concat(backup.TargetPath, relativePath));

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
            }, _cancellationToken.Token);

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
            }, _cancellationToken.Token);

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
            _cancellationToken.Cancel();
            foreach (var task in _tasks.Values.ToList())
                task.Wait();
        }
    }
}