using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using USBBackup.Core;
using USBBackup.Entities;

namespace USBBackup
{
    public class DriveNotificationWrapper : NotificationObject
    {
        private readonly Drive _drive;

        public DriveNotificationWrapper(Drive drive)
        {
            _drive = drive;
            Backups  = new ObservableCollection<BackupNotificationWrapper>(drive.Backups.Select(x => new BackupNotificationWrapper(x)).ToList());
            Backups.CollectionChanged += BackupsOnCollectionChanged;
        }

        public Drive Drive => _drive;

        private void BackupsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (args.OldItems != null)
                foreach (var oldItem in args.OldItems.OfType<BackupNotificationWrapper>())
                    _drive.Backups.Remove(oldItem.Backup);
            if (args.NewItems != null)
                foreach (var newItem in args.NewItems.OfType<BackupNotificationWrapper>())
                    _drive.Backups.Add(newItem.Backup);
        }

        public ObservableCollection<BackupNotificationWrapper> Backups { get; set; }

        public string DriveLetter => _drive.DriveLetter;

        public string Model => _drive.Model;

        public string VolumeName => _drive.Name;

        public bool IsAttached
        {
            get { return _drive.IsAttached; }
            set
            {
                _drive.IsAttached = value;
                OnPropertyChanged();
            }
        }
    }

    public class BackupNotificationWrapper : NotificationObject, IBackup
    {
        bool _isRunning;

        public BackupNotificationWrapper(Backup backup)
        {
            Backup = backup;
        }

        public Backup Backup { get; }

        public string SourcePath
        {
            get { return Backup.SourcePath; }
            set
            {
                Backup.SourcePath = value;
                OnPropertyChanged();
            }
        }

        public string TargetPath
        {
            get { return Backup.TargetPath; }
            set
            {
                Backup.TargetPath = value;
                OnPropertyChanged();
            }
        }

        public bool IsEnabled
        {
            get { return Backup.IsEnabled; }
            set
            {
                Backup.IsEnabled = value;
                OnPropertyChanged();
            }
        }

        public bool IsRunning
        {
            get { return _isRunning; }
            set
            {
                _isRunning = value;
                OnPropertyChanged();
            }
        }
    }
}