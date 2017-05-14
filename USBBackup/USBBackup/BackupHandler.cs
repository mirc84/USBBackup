using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using USBBackup.Entities;

namespace USBBackup
{
    internal class BackupHandler
    {
        public void HandleBackup(USBDeviceNotificationWrapper usbDeviceInfo)
        {
            foreach (var drive in usbDeviceInfo.Drives)
            {
                HandleBackup(drive);
            }
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
        }

        public void HandleBackup(IBackup backup)
        {
            if (!backup.IsEnabled || backup.IsRunning)
                return;

            Task.Factory.StartNew(() =>
            {
                try
                {
                    backup.IsRunning = true;
                    var dir = new DirectoryInfo(backup.SourcePath);
                    if (!dir.Exists)
                        return;

                    foreach (var fileInfo in dir.EnumerateFiles())
                    {
                        try
                        {
                            var relativePath = fileInfo.FullName.Substring(backup.SourcePath.Length);
                            var targetFileInfo = new FileInfo(string.Concat(backup.TargetPath, relativePath));

                            if (fileInfo.LastWriteTime == targetFileInfo.LastWriteTime)
                                continue;

                            var targetPath = targetFileInfo.FullName;
                            File.Copy(fileInfo.FullName, targetPath + ".bak");
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
            });
        }
    }
}