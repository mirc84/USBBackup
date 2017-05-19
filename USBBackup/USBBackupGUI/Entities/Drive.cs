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

        public virtual string DriveLetter { get; set; }
        public virtual string Name { get; set; }
        public virtual string DeviceID { get; set; }
        public virtual string Model { get; set; }
        public virtual string PNPDeviceID  { get; set; }

        public virtual ulong FreeSpace { get; set; }

        public virtual string Description { get; set; }
        public virtual bool IsAttached { get; set; }

        public virtual IList<Backup> Backups { get; set; }

        public virtual void UpdateBackupPaths()
        {
            foreach (var backup in Backups.Where(x => !string.IsNullOrEmpty(x.SourcePath) && !string.IsNullOrEmpty(x.TargetPath)))
            {
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
            HasMany(x => x.Backups).Not.LazyLoad();
        }
    }
}
