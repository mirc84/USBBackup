using System;
using System.Collections.Generic;
using System.Linq;

namespace USBBackup.Entities
{
    public class Drive : DatabaseModel
    {
        public Drive()
        {
            Backups = new List<Backup>();
        }

        public Drive(string driveLetter, string name, string volumeSerialNumber) : this()
        {
            DriveLetter = driveLetter;
            VolumeName = name;
            VolumeSerialNumber = volumeSerialNumber;
        }

        public virtual string DriveLetter { get; set; }
        public virtual string VolumeName { get; set; }
        public virtual string VolumeSerialNumber { get; set; }
        
        public virtual string Description { get; set; }
        public virtual bool IsAttached { get; set; }

        public virtual IList<Backup> Backups { get; set; }

        public virtual void UpdateBackupPaths()
        {
            foreach (var backup in Backups.Where(x => !string.IsNullOrEmpty(x.SourcePath) && !string.IsNullOrEmpty(x.TargetPath)))
            {
                backup.TargetPath = DriveLetter + backup.TargetPath.Substring(2);
            }
        }
    }

    class DriveInfoMap : DatabaseModelMap<Drive>
    {
        public DriveInfoMap()
        {
            Map(x => x.DriveLetter);
            Map(x => x.VolumeName);
            Map(x => x.VolumeSerialNumber);
            Map(x => x.Description);
            HasMany(x => x.Backups).Not.LazyLoad();
        }
    }
}
