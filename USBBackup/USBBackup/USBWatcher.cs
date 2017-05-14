using System.Collections.Generic;
using System.Linq;
using System.Management;
using USBBackup.Core;
using USBBackup.Entities;

namespace USBBackup
{
    internal delegate void USBDeviceAttachedHandler(Drive deviceInfo);

    internal class USBWatcher : NotificationObject
    {
        private ManagementEventWatcher _changedWatcher;
        private IList<Drive> _drives;

        public USBWatcher()
        {
            _drives = new List<Drive>();
        }

        public event USBDeviceAttachedHandler DeviceAttached;
        public event USBDeviceAttachedHandler DeviceDetached;

        public void Init()
        {
            var changedQuery = new WqlEventQuery("SELECT * FROM Win32_VolumeChangeEvent");
            _changedWatcher = new ManagementEventWatcher(changedQuery);
            _changedWatcher.EventArrived += OnVolumeChangeEvent;
            _changedWatcher.Start();
        }

        private void OnVolumeChangeEvent(object sender, EventArrivedEventArgs e)
        {
            var porps = e.NewEvent.Properties.Cast<PropertyData>().ToDictionary(x => x.Name, x => x.Value);
            
            switch ((ushort)e.NewEvent["EventType"])
            {
                case 2: // added
                    OnVolumeAttached(e.NewEvent["DriveName"].ToString());
                    break;
                case 3: // added
                    OnVolumeDetached(e.NewEvent["DriveName"].ToString());
                    break;
                default:
                    break;
            }
        }

        private void OnVolumeAttached(string driveLetter)
        {
            var drive = LoadUSBDevices(driveLetter);
            if (drive == null)
                return;
            
            OnDeviceAttached(drive);
        }

        private void OnVolumeDetached(string driveLetter)
        {
            var drive = _drives.FirstOrDefault(x => x.DriveLetter == driveLetter);
            if (drive == null)
                return;
            
            OnDeviceDetached(drive);
        }

        public IEnumerable<Drive> LoadUSBDevices()
        {
            ManagementObjectCollection collection;
            using (var searcher = new ManagementObjectSearcher(@"Select * From Win32_LogicalDisk"))
            {
                collection = searcher.Get();
            }

            foreach (var item in collection.OfType<ManagementObject>().Where(x => (uint)x["DriveType"] < 5))
            {
                var drive = CreateUSBDeviceInfo(item);
                if (string.IsNullOrEmpty(drive.DriveLetter))
                    continue;
                _drives.Add(drive);
                yield return drive;
            }
        }

        public Drive LoadUSBDevices(string driveLetter)
        {
            if (string.IsNullOrEmpty(driveLetter))
                return null;

            ManagementObjectCollection collection;
            using (var searcher = new ManagementObjectSearcher($"Select * From Win32_LogicalDisk Where Name = '{driveLetter}'"))
            {
                collection = searcher.Get();
            }

            var drive = CreateUSBDeviceInfo(collection.OfType<ManagementBaseObject>().First());
            _drives.Add(drive);
            return drive;
        }

        //public IEnumerable<Drive> LoadUSBDevices(string deviceID)
        //{
        //    ManagementObjectCollection collection;
        //    using (var searcher = new ManagementObjectSearcher($"Select * From Win32_USBHub WHERE DeviceID='{deviceID.Replace("\\","\\\\")}'"))
        //    {
        //        collection = searcher.Get();
        //    }

        //    foreach (var item in collection.OfType<ManagementObject>())
        //    {
        //        yield return CreateUSBDeviceInfo(item);
        //    }
        //}

        protected virtual void OnDeviceAttached(Drive deviceInfo)
        {
            DeviceAttached?.Invoke(deviceInfo);
        }

        protected virtual void OnDeviceDetached(Drive deviceInfo)
        {
            DeviceDetached?.Invoke(deviceInfo);
        }
        
        private static Drive CreateUSBDeviceInfo(ManagementBaseObject volume)
        {
            var porps = volume.Properties.Cast<PropertyData>().ToDictionary(x => x.Name, x => x.Value);

            var volumeSerialNumber = volume["VolumeSerialNumber"].ToString();
            var driveLetter = volume["Name"].ToString();
            var name = volume["VolumeName"].ToString();
            return new Drive(driveLetter, name, volumeSerialNumber);
        }
    }
}
