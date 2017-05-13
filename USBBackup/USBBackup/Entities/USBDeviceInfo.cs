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

        public string DeviceID { get; private set; }
        public string PnpDeviceID { get; private set; }
        public string Description { get; private set; }
        public bool IsAttached { get; set; }
        public bool IsEnabled { get; set; }
        public IList<BackupInfo> Backups { get; set; }
    }
}
