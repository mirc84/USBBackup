using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using USBBackup.Core;
using USBBackup.Entities;

namespace USBBackup
{
    public delegate MessageBoxResult AskUserEventHandler(string message, string caption);

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
            RemoveBackupCommand = new RelayCommand(RemoveBackup);
            RunBackupCommand = new RelayCommand(RunBackup);
            CancelBackupCommand = new RelayCommand(CancelBackup);
            RunAllBackupsCommand = new RelayCommand(RunAllBackups);
            SaveCommand = new RelayCommand(Save);
            UsbDevices = new ObservableCollection<DriveNotificationWrapper>(_usbDeviceRepository.USBDevices.Select(x => new DriveNotificationWrapper(x)).ToList());
        }

        protected virtual MessageBoxResult OnUserChoiceRequested(string message, string caption)
        {
            return (UserChoiceRequested?.Invoke(message, caption)).GetValueOrDefault(MessageBoxResult.No);
        }

        public event AskUserEventHandler UserChoiceRequested;

        private void CancelBackup(object obj)
        {
            var backup = obj as BackupNotificationWrapper;
            if (backup == null)
                return;

            _backupHandler.CancelBackup(backup);
        }

        private void RemoveBackup(object obj)
        {
            var backup = obj as BackupNotificationWrapper;
            if (backup == null)
                return;

            var drive = UsbDevices.FirstOrDefault(x => x.Backups.Contains(backup));
            if (drive == null)
                return;

            var choice = UserChoiceRequested("Do you want to remove this backup?", "Remove Backup?");
            if (choice != MessageBoxResult.Yes)
                return;

            drive.Backups.Remove(backup);
            _backupHandler.CancelBackup(backup);
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
        public ICommand RemoveBackupCommand { get; }
        public ICommand RunBackupCommand { get; }
        public ICommand CancelBackupCommand { get; }
        public ICommand RunAllBackupsCommand { get; }
        public ICommand SaveCommand { get; }

        private void OnUSBDevicesChanged(Drive drive)
        {
            var existing = UsbDevices.FirstOrDefault(x => x.Drive.DeviceID == drive.DeviceID && x.Drive.PNPDeviceID == drive.PNPDeviceID);
            if (existing == null)
                UsbDevices.Add(new DriveNotificationWrapper(drive));

            existing.IsAttached = drive.IsAttached;
        }

        private void AddBackup(object obj)
        {
            var drive = obj as DriveNotificationWrapper;
            if (drive == null)
                return;

            var backup = new Backup();
            backup.TargetPath = drive.DriveLetter;
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
