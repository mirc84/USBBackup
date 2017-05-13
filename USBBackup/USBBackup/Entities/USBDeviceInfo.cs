using System.Collections.Generic;

namespace USBBackup.Entities
{
    public class USBDeviceInfo : DatabaseModel
    {
        protected USBDeviceInfo()
        {
            Backups = new List<BackupInfo>();
        }

        public USBDeviceInfo(string deviceID, string pnpDeviceID, string description)
        {
            DeviceID = deviceID;
            PnpDeviceID = pnpDeviceID;
            Description = description;
            Backups = new List<BackupInfo>();
        }

        public virtual string DeviceID { get; private set; }
        public virtual string PnpDeviceID { get; private set; }
        public virtual string Description { get; private set; }
        public virtual bool IsAttached { get; set; }
        public virtual bool IsEnabled { get; set; }
        public virtual IList<BackupInfo> Backups { get; set; }
    }
    public class USBDeviceInfoMap : DatabaseModelMap<USBDeviceInfo>
    {
        public USBDeviceInfoMap()
        {
            Map(x => x.DeviceID);
            Map(x => x.Description);
            Map(x => x.IsEnabled);
            HasMany(x => x.Backups);
        }
    }
}
