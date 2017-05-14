using System.Collections.Generic;
using System.Management;

namespace USBBackup.Entities
{
    public class USBDevice : DatabaseModel
    {
        protected USBDevice()
        {
            Drives = new List<Drive>();
        }

        public USBDevice(string deviceID, string pnpDeviceID, string description, ManagementObject managementObject)
        {
            DeviceID = deviceID;
            PnpDeviceID = pnpDeviceID;
            Description = description;
            ManagementObject = managementObject;
            Drives = new List<Drive>();
        }

        public virtual string DeviceID { get; set; }
        public virtual string PnpDeviceID { get; set; }
        public virtual string Description { get; set; }
        public virtual bool IsAttached { get; set; }
        public virtual IList<Drive> Drives { get; set; }

        public virtual ManagementObject ManagementObject { get; set; }
    }

    public class USBDeviceInfoMap : DatabaseModelMap<USBDevice>
    {
        public USBDeviceInfoMap()
        {
            Map(x => x.DeviceID);
            Map(x => x.Description);
            HasMany(x => x.Drives).Not.LazyLoad();
        }
    }
}
