using System.Collections.Generic;

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
        public virtual IList<Backup> Backups { get; set; }
    }

    class DriveInfoMap : DatabaseModelMap<Drive>
    {
        public DriveInfoMap()
        {
            Map(x => x.DriveLetter);
            Map(x => x.VolumeName);
            Map(x => x.VolumeSerialNumber);
            HasMany(x => x.Backups).Not.LazyLoad();
        }
    }
}
