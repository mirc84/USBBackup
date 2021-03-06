﻿using System.Collections.Generic;
using System.Linq;

namespace USBBackup.Entities
{
    public class Drive : DatabaseModel
    {
        #region Fields

        private string _driveLetter;
        private string _model;
        private bool _isAttached;

        #endregion
        
        #region Constructor

        public Drive()
        {
            Backups = new List<Backup>();
        }

        #endregion

        #region Properties

        public virtual string DriveLetter
        {
            get { return _driveLetter; }
            set
            {
                _driveLetter = value;
                OnPropertyChanged();
            }
        }

        public virtual string Model
        {
            get { return _model; }
            set
            {
                _model = value;
                OnPropertyChanged();
            }
        }

        public virtual string Name { get; set; }
        public virtual string DeviceID { get; set; }
        public virtual string PNPDeviceID { get; set; }

        public virtual ulong FreeSpace { get; set; }

        public virtual string Description { get; set; }

        public virtual bool IsAttached
        {
            get { return _isAttached; }
            set
            {
                _isAttached = value;
                OnPropertyChanged();
            }
        }

        public virtual IList<Backup> Backups { get; set; }

        #endregion

        #region Public Methods

        public virtual void UpdateBackupPaths()
        {
            foreach (var backup in Backups.Where(x => !string.IsNullOrEmpty(x.SourcePath) && !string.IsNullOrEmpty(x.TargetPath)))
            {
                backup.SetDriveLetter(DriveLetter);
            }
        }

        #endregion
    }
}
