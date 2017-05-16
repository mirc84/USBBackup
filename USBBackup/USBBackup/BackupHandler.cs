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
        private CancellationTokenSource _cancellationToken;
        private Dictionary<IBackup, Task> _tasks;

        public BackupHandler()
        {
            _cancellationToken = new CancellationTokenSource();
            _tasks = new Dictionary<IBackup, Task>();
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
            if (!backup.IsEnabled || backup.IsRunning)
                return;

            var task = Task.Factory.StartNew(() =>
            {
                try
                {
                    backup.IsRunning = true;
                    var dir = new DirectoryInfo(backup.SourcePath);
                    if (!dir.Exists)
                        return;

                    foreach (var fileInfo in dir.EnumerateFiles())
                    {
                        if (_cancellationToken.IsCancellationRequested)
                            return;

                        try
                        {
                            var relativePath = fileInfo.FullName.Substring(backup.SourcePath.Length);
                            var targetFileInfo = new FileInfo(string.Concat(backup.TargetPath, relativePath));

                            if (fileInfo.LastWriteTime == targetFileInfo.LastWriteTime)
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

                    Thread.Sleep(2000);

                }
                catch (System.Exception e)
                {

                }
                finally
                {
                    backup.IsRunning = false;
                }
            }, _cancellationToken.Token);

            _tasks.Add(backup, task);
            task.ContinueWith(_ =>
            {
                _tasks.Remove(backup);
            });
        }

        internal void CancelBackups()
        {
            _cancellationToken.Cancel();
            foreach (var task in _tasks.Values.ToList())
                task.Wait();
        }
    }
}