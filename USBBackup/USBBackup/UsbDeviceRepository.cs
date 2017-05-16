using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Threading;
using USBBackup.DatabaseAccess;
using USBBackup.Entities;

namespace USBBackup
{
    internal class UsbDeviceRepository
    {
        private readonly USBWatcher _watcher;
        private readonly DatabaseConnection _databaseConncetion;
        private readonly BackupHandler _backupHandler;
        private readonly Dispatcher _dispatcher;

        public UsbDeviceRepository(USBWatcher watcher, DatabaseConnection databaseConncetion, BackupHandler backupHandler, Dispatcher dispatcher)
        {
            _watcher = watcher;
            _databaseConncetion = databaseConncetion;
            _backupHandler = backupHandler;
            _dispatcher = dispatcher;

            USBDevices = new ObservableCollection<Drive>();
        }

        public IList<Drive> USBDevices { get; }

        public event EventHandler USBDevicesChanged;

        public void Load()
        {
            var usbDevices = _databaseConncetion.GetAll<Drive>();
            foreach (var usbDeviceInfo in usbDevices)
                USBDevices.Add(usbDeviceInfo);

            var attachedUSBDevices = _watcher.LoadDrives().ToList();
            foreach (var attachedUSBDevice in attachedUSBDevices)
                OnUSBDriveAttached(attachedUSBDevice);

            _watcher.DriveAttached += OnUSBDriveAttached;
            _watcher.DriveDetached += OnUSBDriveDetached;
        }

        public void Save()
        {
            _databaseConncetion.SaveDevices(USBDevices);
        }

        private void OnUSBDriveAttached(Drive drive)
        {
            var existingDevice = USBDevices.FirstOrDefault(x => x.DeviceID == drive.DeviceID && x.PNPDeviceID == drive.PNPDeviceID);
            if (existingDevice == null)
            {
                _dispatcher.Invoke(() => USBDevices.Add(drive));
                OnUSBDevicesChanged();

                return;
            }

            existingDevice.IsAttached = true;
            existingDevice.DriveLetter = drive.DriveLetter;
            existingDevice.Name = drive.Name;
            existingDevice.UpdateBackupPaths();
            OnExistingDriveAttached(drive);
        }

        private void OnExistingDriveAttached(Drive existingDrive)
        {
            _backupHandler.HandleBackup(existingDrive);
        }

        private void OnUSBDriveDetached(Drive attachedUSBDevice)
        {
            var existingDevice = USBDevices.FirstOrDefault(x => x.DeviceID == attachedUSBDevice.DeviceID && x.PNPDeviceID == attachedUSBDevice.PNPDeviceID);
            if (existingDevice == null)
                return;

            existingDevice.IsAttached = false;
            if (existingDevice.Backups == null || !existingDevice.Backups.Any())
                _dispatcher.Invoke(() => USBDevices.Remove(existingDevice));

            OnUSBDevicesChanged();
        }

        protected virtual void OnUSBDevicesChanged()
        {
            USBDevicesChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}