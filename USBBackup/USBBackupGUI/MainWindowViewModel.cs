using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using USBBackup;
using USBBackup.Core;
using USBBackup.Entities;
using USBBackup.Strings;

namespace USBBackupGUI
{
    public delegate MessageBoxResult AskUserEventHandler(string message, string caption, MessageBoxButton results);
    public delegate void NotifyUserEventHandler(string message, string caption);

    class MainWindowViewModel : NotificationObject
    {
        #region Fields

        private readonly UsbDeviceRepository _usbDeviceRepository;
        private IList<DriveNotificationWrapper> _usbDevices;
        private BackupHandler _backupHandler;
        private Dispatcher _dispatcher;

        #endregion

        #region Constructor

        public MainWindowViewModel(UsbDeviceRepository usbDeviceRepository, BackupHandler backupHandler, Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
            _backupHandler = backupHandler;
            _usbDeviceRepository = usbDeviceRepository;
            _usbDeviceRepository.USBDevicesChanged += OnUSBDevicesChanged;
            AddBackupCommand = new RelayCommand(AddBackup);
            RemoveBackupCommand = new RelayCommand(RemoveBackup);
            RunBackupCommand = new RelayCommand(RunBackup);
            CancelBackupCommand = new RelayCommand(CancelBackup);
            RunPauseResumeBackupCommand = new RelayCommand(RunPauseResumeBackup);
            RunAllBackupsCommand = new RelayCommand(RunAllBackups);
            SaveCommand = new RelayCommand(Save);
            UsbDevices = new ObservableCollection<DriveNotificationWrapper>(_usbDeviceRepository.USBDevices.Select(x => new DriveNotificationWrapper(x)).ToList());
        }

        #endregion

        #region Properties

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
        public ICommand RunPauseResumeBackupCommand { get; }
        public ICommand RunAllBackupsCommand { get; }
        public ICommand SaveCommand { get; }

        #endregion

        #region Events

        public event AskUserEventHandler UserChoiceRequested;
        public event NotifyUserEventHandler UserNotification;

        #endregion

        #region Non Public Methods

        protected virtual MessageBoxResult OnUserChoiceRequested(string message, string caption, MessageBoxButton? result = null)
        {
            return (UserChoiceRequested?.Invoke(message, caption, result ?? MessageBoxButton.YesNo)).GetValueOrDefault(MessageBoxResult.No);
        }

        protected virtual void OnUserNotification(string message, string caption)
        {
            UserNotification?.Invoke(message, caption);
        }

        private void RunPauseResumeBackup(object obj)
        {
            var backup = obj as Backup;
            if (backup == null)
                return;
            else if (!backup.IsRunning)
                _backupHandler.HandleBackup(backup, true);
            else if (backup.IsPaused)
                _backupHandler.ResumeBackup(backup);
            else
                _backupHandler.PauseBackup(backup);
        }

        private void CancelBackup(object obj)
        {
            var backup = obj as Backup;
            if (backup == null)
                return;

            var choice = backup.IsPaused 
                ? MessageBoxResult.Yes
                : OnUserChoiceRequested(new Loc(nameof(StringResource.UserChoice_HardCancel)),
                new Loc(nameof(StringResource.UserChoice_HardCancel_Caption)), MessageBoxButton.YesNoCancel);
            if (choice == MessageBoxResult.Cancel)
                return;

            _backupHandler.CancelBackup(backup, choice == MessageBoxResult.Yes);
        }

        private void RemoveBackup(object obj)
        {
            var backup = obj as Backup;
            if (backup == null)
                return;

            var drive = UsbDevices.FirstOrDefault(x => x.Backups.Contains(backup));
            if (drive == null)
                return;

            var choice = OnUserChoiceRequested(new Loc(nameof(StringResource.UserChoice_RemoveBackup)),
                new Loc(nameof(StringResource.UserChoiceCaption_RemoveBackup)));
            if (choice != MessageBoxResult.Yes)
                return;

            drive.Backups.Remove(backup);
            _backupHandler.CancelBackup(backup, true);
        }

        private void Save(object obj)
        {
            try
            {
                _usbDeviceRepository.Save();
                OnUserNotification(new Loc(nameof(StringResource.UserNotification_BackupsSaved)),
                    new Loc(nameof(StringResource.UserNotificationCaption_BackupsSaved)));
            }
            catch (Exception e)
            {
                Log.Application.Error(e, "An error occurred while saving backups");
            }
        }

        private void RunAllBackups(object obj)
        {
            foreach (var device in UsbDevices)
            {
                _backupHandler.HandleBackup(device.Drive);
            }
        }

        private void OnUSBDevicesChanged(Drive drive)
        {
            var existing = UsbDevices.FirstOrDefault(x => x.Drive.DeviceID == drive.DeviceID && x.Drive.PNPDeviceID == drive.PNPDeviceID);
            if (existing == null)
                _dispatcher.Invoke(new Action(() => UsbDevices.Add(new DriveNotificationWrapper(drive))));
            else
                existing.IsAttached = drive.IsAttached;
        }

        private void AddBackup(object obj)
        {
            var drive = obj as DriveNotificationWrapper;
            if (drive == null)
                return;

            var backup = new Backup()
            {
                Drive = drive.Drive,
                TargetPath = drive.DriveLetter
            };
            drive.Backups.Add(backup);
        }

        private void RunBackup(object obj)
        {
            var drive = obj as DriveNotificationWrapper;
            if (drive != null)
            {
                _backupHandler.HandleBackup(drive.Drive);
                return;
            }
            var backup = obj as Backup;
            if (backup != null)
            {
                _backupHandler.HandleBackup(backup);
                return;
            }
        }

        #endregion
    }
}
