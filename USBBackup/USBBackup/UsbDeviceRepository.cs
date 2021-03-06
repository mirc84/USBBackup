﻿using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Timers;
using USBBackup.DatabaseAccess;
using USBBackup.Entities;

namespace USBBackup
{
    public delegate void DeviceChangedEventHandler(Drive drive);

    public class UsbDeviceRepository
    {
        #region Fields

        private readonly USBWatcher _watcher;
        private readonly DatabaseConnection _databaseConncetion;
        private readonly BackupHandler _backupHandler;
        private Timer _backupTimer;
        private IDictionary<Backup, FileSystemWatcher> _backupFileWatchers;

        #endregion

        #region Constructor

        public UsbDeviceRepository(USBWatcher watcher, DatabaseConnection databaseConncetion, BackupHandler backupHandler)
        {
            _watcher = watcher;
            _databaseConncetion = databaseConncetion;
            _backupHandler = backupHandler;
            _backupFileWatchers = new Dictionary<Backup, FileSystemWatcher>();
            USBDevices = new List<Drive>();
            _backupTimer = new Timer()
            {
                AutoReset = true,
                Interval = Properties.Settings.Default.BackupInterval.TotalMilliseconds
            };
            _backupTimer.Elapsed += (_, __) => RunAllBackups();
            Properties.Settings.Default.PropertyChanged += OnSettingsChanged;
        }

        #endregion
        
        #region Properties

        public IList<Drive> USBDevices { get; }

        #endregion

        #region Events

        public event DeviceChangedEventHandler USBDevicesChanged;

        #endregion

        #region Public Methods

        public void Load()
        {
            var usbDevices = _databaseConncetion.GetAll<Drive>();
            foreach (var usbDeviceInfo in usbDevices)
            {
                USBDevices.Add(usbDeviceInfo);
                foreach (var backup in usbDeviceInfo.Backups)
                {
                    backup.SetDataSaved();

                    if (backup.SourcePath == null || !Directory.Exists(backup.SourcePath))
                        continue;

                    var watcher = new FileSystemWatcher(backup.SourcePath)
                    {
                        EnableRaisingEvents = true,
                        IncludeSubdirectories = true
                    };
                    watcher.Changed += (s, f) => OnDirChanged(backup, f);
                    watcher.Created += (s, f) => OnDirChanged(backup, f);
                    watcher.Deleted += (s, f) => OnDirDeleted(backup, f);
                    _backupFileWatchers[backup] = watcher;
                }
                OnUSBDevicesChanged(usbDeviceInfo);
            }
            var attachedUSBDevices = _watcher.LoadDrives().ToList();
            foreach (var attachedUSBDevice in attachedUSBDevices)
                OnUSBDriveAttached(attachedUSBDevice);

            _watcher.DriveAttached += OnUSBDriveAttached;
            _watcher.DriveDetached += OnUSBDriveDetached;
        }

        private void OnDirDeleted(Backup backup, FileSystemEventArgs f)
        {
            if (USBBackup.Properties.Settings.Default.CleanupRemovedFile)
                _backupHandler.RecycleBackupFiles(backup);
        }

        private void OnDirChanged(Backup backup, FileSystemEventArgs f)
        {
            _backupHandler.HandleBackup(backup, f.FullPath);
        }

        public void Save()
        {
            _databaseConncetion.SaveDevices(USBDevices);
        }

        #endregion

        #region Non Public Methods

        private void OnSettingsChanged(object sender, PropertyChangedEventArgs e)
        {
            _backupTimer.Stop();
            _backupTimer.Interval = Properties.Settings.Default.BackupInterval.TotalMilliseconds;

            foreach (var fileWatcher in _backupFileWatchers.Values)
            {
                fileWatcher.EnableRaisingEvents = Properties.Settings.Default.WatchBackupSources;
            }

            if (Properties.Settings.Default.HandleBackupOnInterval)
            {
                _backupTimer.Start();
            }
        }

        private void RunAllBackups()
        {
            foreach (var drive in USBDevices)
            {
                _backupHandler.HandleBackup(drive);
            }
        }

        private void OnUSBDriveAttached(Drive drive)
        {
            var existingDevice = USBDevices.FirstOrDefault(x => x.DeviceID == drive.DeviceID && x.PNPDeviceID == drive.PNPDeviceID);
            if (existingDevice == null)
            {
                USBDevices.Add(drive);
                OnUSBDevicesChanged(drive);

                return;
            }

            existingDevice.IsAttached = true;
            existingDevice.DriveLetter = drive.DriveLetter;
            existingDevice.Name = drive.Name;
            existingDevice.UpdateBackupPaths();
            OnExistingDriveAttached(existingDevice);
            OnUSBDevicesChanged(existingDevice);
        }

        private void OnExistingDriveAttached(Drive existingDrive)
        {
            _backupHandler.HandleBackup(existingDrive);
            if (USBBackup.Properties.Settings.Default.CleanupRemovedFile)
                foreach (var backup in existingDrive.Backups)
                    _backupHandler.RecycleBackupFiles(backup);
        }

        private void OnUSBDriveDetached(Drive attachedUSBDevice)
        {
            var existingDevice = USBDevices.FirstOrDefault(x => x.DeviceID == attachedUSBDevice.DeviceID && x.PNPDeviceID == attachedUSBDevice.PNPDeviceID);
            if (existingDevice == null)
                return;

            existingDevice.IsAttached = false;
            if (existingDevice.Backups == null || !existingDevice.Backups.Any())
                USBDevices.Remove(existingDevice);

            OnUSBDevicesChanged(existingDevice);
        }

        protected virtual void OnUSBDevicesChanged(Drive drive)
        {
            USBDevicesChanged?.Invoke(drive);
        }

        #endregion
    }
}