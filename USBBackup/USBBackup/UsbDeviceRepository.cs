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

            USBDevices = new ObservableCollection<USBDevice>();
        }

        public IList<USBDevice> USBDevices { get; }

        public event EventHandler USBDevicesChanged;

        public void Load()
        {
            var usbDevices = _databaseConncetion.GetAll<USBDevice>();
            foreach (var usbDeviceInfo in usbDevices)
                USBDevices.Add(usbDeviceInfo);

            var attachedUSBDevices = _watcher.LoadUSBDevices().ToList();
            foreach (var attachedUSBDevice in attachedUSBDevices)
                OnUSBDeviceAttached(attachedUSBDevice);

            _watcher.DeviceAttached += OnUSBDeviceAttached;
            _watcher.DeviceDetached += OnUSBDeviceDetached;
        }

        public void Save()
        {
            _databaseConncetion.SaveDevices(USBDevices);
        }

        private void OnUSBDeviceAttached(USBDevice attachedUSBDevice)
        {
            var existingDevice = USBDevices.FirstOrDefault(x => x.DeviceID == attachedUSBDevice.DeviceID);
            if (existingDevice == null)
            {
                if (attachedUSBDevice.Drives.Any())
                {
                    _dispatcher.Invoke(() => USBDevices.Add(attachedUSBDevice));
                    OnUSBDevicesChanged();
                }
                return;
            }

            existingDevice.IsAttached = true;
            existingDevice.ManagementObject = attachedUSBDevice.ManagementObject;
            foreach (var drive in existingDevice.Drives)
            {
                var attachedDrive =
                    attachedUSBDevice.Drives.FirstOrDefault(x => x.VolumeSerialNumber == drive.VolumeSerialNumber);
                if (attachedDrive == null)
                {
                    continue;
                }

                drive.DriveLetter = attachedDrive.DriveLetter;
                drive.VolumeName = attachedDrive.VolumeName;
                OnExistingDriveAttached(drive);
            }
            foreach (var drive in attachedUSBDevice.Drives.Where(x => existingDevice.Drives.All(y => y.VolumeSerialNumber != x.VolumeSerialNumber)))
            {
                existingDevice.Drives.Add(drive);
            }

            OnUSBDevicesChanged();
        }

        private void OnExistingDriveAttached(Drive existingDrive)
        {
            _backupHandler.HandleBackup(existingDrive);
        }

        private void OnUSBDeviceDetached(USBDevice attachedUSBDevice)
        {
            var existingDevice = USBDevices.FirstOrDefault(x => x.DeviceID == attachedUSBDevice.DeviceID);
            if (existingDevice == null)
                return;

            existingDevice.IsAttached = false;
            if (existingDevice.Drives == null || !existingDevice.Drives.Any())
                _dispatcher.Invoke(() => USBDevices.Remove(existingDevice));

            OnUSBDevicesChanged();
        }

        protected virtual void OnUSBDevicesChanged()
        {
            USBDevicesChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}