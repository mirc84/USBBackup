using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using USBBackup.Core;
using USBBackup.Entities;

namespace USBBackup
{
    class MainWindowViewModel : NotificationObject
    {
        private readonly UsbDeviceRepository _usbDeviceRepository;
        private IList<DriveNotificationWrapper> _usbDevices;
        private BackupHandler _backupHandler;

        public MainWindowViewModel(UsbDeviceRepository usbDeviceRepository, BackupHandler backupHandler)
        {
            _backupHandler = backupHandler;
            _usbDeviceRepository = usbDeviceRepository;
            _usbDeviceRepository.USBDevicesChanged += OnUSBDevicesChanged;
            AddBackupCommand = new RelayCommand(AddBackup);
            RunBackupCommand = new RelayCommand(RunBackup);
            RunAllBackupsCommand = new RelayCommand(RunAllBackups);
            SaveCommand = new RelayCommand(Save);
            UsbDevices = _usbDeviceRepository.USBDevices.Select(x => new DriveNotificationWrapper(x)).ToList();
        }

        private void Save(object obj)
        {
            _usbDeviceRepository.Save();
        }

        private void RunAllBackups(object obj)
        {
            foreach (var device in UsbDevices)
            {
                _backupHandler.HandleBackup(device);
            }
        }

        public IList<DriveNotificationWrapper> UsbDevices
        {
            get { return _usbDevices; }
            set
            {
                _usbDevices = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddBackupCommand { get; }
        public ICommand RunBackupCommand { get; }
        public ICommand RunAllBackupsCommand { get; }
        public ICommand SaveCommand { get; }

        private void OnUSBDevicesChanged(object sender, EventArgs eventArgs)
        {
            UsbDevices = _usbDeviceRepository.USBDevices.Select(x => new DriveNotificationWrapper(x)).ToList();
            
        }

        private void AddBackup(object obj)
        {
            var drive = obj as DriveNotificationWrapper;
            if (drive == null)
                return;

            var backup = new Backup();
            drive.Backups.Add(new BackupNotificationWrapper(backup));
        }

        private void RunBackup(object obj)
        {
            var drive = obj as DriveNotificationWrapper;
            if (drive != null)
            {
                _backupHandler.HandleBackup(drive);
                return;
            }
            var backup = obj as BackupNotificationWrapper;
            if (backup != null)
            {
                _backupHandler.HandleBackup(backup);
                return;
            }
        }
    }
}
