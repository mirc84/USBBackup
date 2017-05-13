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
        private readonly DatabaseContext _databaseContext;
        private readonly BackupHandler _backupHandler;
        private readonly Dispatcher _dispatcher;

        public UsbDeviceRepository(USBWatcher watcher, DatabaseContext databaseContext, BackupHandler backupHandler, Dispatcher dispatcher)
        {
            _watcher = watcher;
            _databaseContext = databaseContext;
            _backupHandler = backupHandler;
            _dispatcher = dispatcher;

            USBDevices = new ObservableCollection<USBDeviceInfo>();

        }

        public IList<USBDeviceInfo> USBDevices { get; }

        public void Init()
        {
            _databaseContext.BackupInfos.ToList();
            var usbDevices = _databaseContext.USBDeviceInfos.ToList();
            foreach (var usbDeviceInfo in usbDevices)
                USBDevices.Add(usbDeviceInfo);

            var attachedUSBDevices = _watcher.LoadUSBDevices().ToList();
            foreach (var attachedUSBDevice in attachedUSBDevices)
                OnUSBDeviceAttached(attachedUSBDevice);

            _watcher.DeviceAttached += OnUSBDeviceAttached;
            _watcher.DeviceDetached += OnUSBDeviceDetached;
        }

        private void OnUSBDeviceAttached(USBDeviceInfo attachedUSBDevice)
        {
            var existingDevice = USBDevices.FirstOrDefault(x => x.DeviceID == attachedUSBDevice.DeviceID);
            if (existingDevice == null)
            {
                _dispatcher.Invoke(() => USBDevices.Add(attachedUSBDevice));
                return;
            }

            existingDevice.IsAttached = true;
            _backupHandler.HandleBackup(existingDevice);
        }

        private void OnUSBDeviceDetached(USBDeviceInfo attachedUSBDevice)
        {
            var existingDevice = USBDevices.FirstOrDefault(x => x.DeviceID == attachedUSBDevice.DeviceID);
            if (existingDevice == null)
                return;

            existingDevice.IsEnabled = false;
            if (existingDevice.Backups == null || !existingDevice.Backups.Any())
                _dispatcher.Invoke(() => USBDevices.Remove(existingDevice));
        }
    }
}