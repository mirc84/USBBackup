using System.Collections.Generic;
using System.Linq;

namespace USBBackup.Entities
{
    public class Drive : DatabaseModel
    {
        private string _driveLetter;
        private string _model;

        public Drive()
        {
            Backups = new List<Backup>();
        }

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
        public virtual string PNPDeviceID  { get; set; }

        public virtual ulong FreeSpace { get; set; }

        public virtual string Description { get; set; }
        public virtual bool IsAttached { get; set; }

        public virtual IList<Backup> Backups { get; set; }

        public virtual void UpdateBackupPaths()
        {
            foreach (var backup in Backups.Where(x => !string.IsNullOrEmpty(x.SourcePath) && !string.IsNullOrEmpty(x.TargetPath)))
            {
                if (backup.IsInverse)
                    backup.SourcePath = DriveLetter + backup.SourcePath.Substring(DriveLetter.Length);
                else
                    backup.TargetPath = DriveLetter + backup.TargetPath.Substring(DriveLetter.Length);
            }
        }
    }

    class DriveInfoMap : DatabaseModelMap<Drive>
    {
        public DriveInfoMap()
        {
            Map(x => x.DriveLetter);
            Map(x => x.Name);
            Map(x => x.DeviceID);
            Map(x => x.Model);
            Map(x => x.PNPDeviceID);
            Map(x => x.Description);
            HasMany(x => x.Backups).Inverse().KeyColumn("Drive_id").Not.LazyLoad();
        }
    }
}
