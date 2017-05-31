using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using USBBackup.Core;
using USBBackup.Entities;

namespace USBBackup
{
    public class DriveNotificationWrapper : NotificationObject
    {
        #region Fields

        private readonly Drive _drive;

        #endregion

        #region Constructor

        public DriveNotificationWrapper(Drive drive)
        {
            _drive = drive;
            _drive.PropertyChanged += (s, name) => OnPropertyChanged(name.PropertyName);
            Backups = new ObservableCollection<Backup>(drive.Backups);
            Backups.CollectionChanged += BackupsOnCollectionChanged;
        }

        #endregion
        
        #region Properties

        public Drive Drive => _drive;

        public ObservableCollection<Backup> Backups { get; set; }

        public string DriveLetter => _drive.DriveLetter;

        public string Model => _drive.Model;

        public string Name => _drive.Name;

        public bool IsAttached
        {
            get { return _drive.IsAttached; }
            set
            {
                _drive.IsAttached = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Non Public Methods

        private void BackupsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (args.OldItems != null)
                foreach (var oldItem in args.OldItems.OfType<Backup>())
                    _drive.Backups.Remove(oldItem);
            if (args.NewItems != null)
                foreach (var newItem in args.NewItems.OfType<Backup>())
                    _drive.Backups.Add(newItem);
        }

        #endregion
    }
}