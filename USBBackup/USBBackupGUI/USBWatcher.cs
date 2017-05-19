using System.Collections.Generic;
using System.Linq;
using System.Management;
using USBBackup.Core;
using USBBackup.Entities;

namespace USBBackup
{
    internal delegate void DriveAttachedHandler(Drive deviceInfo);

    internal class USBWatcher : NotificationObject
    {
        private ManagementEventWatcher _changedWatcher;
        private readonly IList<Drive> _drives;

        public USBWatcher()
        {
            _drives = new List<Drive>();
        }

        public event DriveAttachedHandler DriveAttached;
        public event DriveAttachedHandler DriveDetached;

        public void Init()
        {
            var changedQuery = new WqlEventQuery("SELECT * FROM Win32_VolumeChangeEvent");
            _changedWatcher = new ManagementEventWatcher(changedQuery);
            _changedWatcher.EventArrived += OnVolumeChangeEvent;
            _changedWatcher.Start();
        }

        private void OnVolumeChangeEvent(object sender, EventArrivedEventArgs e)
        {
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
            var drive = LoadDrives(driveLetter).FirstOrDefault();
            if (drive == null)
                return;
            
            OnDeviceAttached(drive);
        }

        private void OnVolumeDetached(string driveLetter)
        {
            var drive = _drives.FirstOrDefault(x => x.DriveLetter == driveLetter);
            if (drive == null)
                return;

            _drives.Remove(drive);
            OnDeviceDetached(drive);
        }

        public IEnumerable<Drive> LoadDrives(string driveLetter = null)
        {
            var query = "SELECT * FROM Win32_Volume";
            if (driveLetter != null)
                query += $" WHERE DriveLetter='{driveLetter}'";

            var searcher = new ManagementObjectSearcher(query).Get();
            foreach (ManagementObject item in searcher)
            {
                Drive drive = null;
                try
                {
                    driveLetter = (string)item["DriveLetter"];
                    if (string.IsNullOrEmpty(driveLetter))
                        continue;

                    var driveObject = GetDiskVolume(driveLetter);
                    if (driveObject == null)
                        continue;

                    drive = new Drive
                    {
                        DriveLetter = driveLetter,
                        IsAttached = true,
                        DeviceID = (string)item["DeviceID"],
                        FreeSpace = (ulong)item["FreeSpace"],
                        Name = (string)driveObject["Name"],
                        Model = (string)driveObject["Model"],
                        PNPDeviceID = (string)driveObject["PNPDeviceID"]
                    };

                    _drives.Add(drive);
                }
                catch (System.Exception e)
                {
                    continue;                    
                }
                if (drive != null)
                    yield return drive;
            }
        }

        private ManagementObject GetDiskVolume(string volumeLetter)
        {
            var collection = new ManagementObjectSearcher($"ASSOCIATORS OF {{Win32_LogicalDisk.DeviceID='{volumeLetter}'}} WHERE AssocClass=Win32_LogicalDiskToPartition").Get();
            foreach (ManagementObject item in collection)
            {
                var associators = new ManagementObjectSearcher($"ASSOCIATORS OF {{Win32_DiskPartition.DeviceID='{item["DeviceID"]}'}}").Get();
                foreach (ManagementObject associator in associators)
                {
                    if (associator["CreationClassName"].ToString() != "Win32_DiskDrive")
                        continue;

                    var drives = new ManagementObjectSearcher($"SELECT * FROM Win32_DiskDrive WHERE DeviceID='{associator["DeviceID"].ToString().Replace(@"\", @"\\")}'").Get();
                    foreach (ManagementObject diskVolume in drives)
                    {
                        return diskVolume;
                    }
                }
            }
            return null;
        }

        protected virtual void OnDeviceAttached(Drive deviceInfo)
        {
            DriveAttached?.Invoke(deviceInfo);
        }

        protected virtual void OnDeviceDetached(Drive deviceInfo)
        {
            DriveDetached?.Invoke(deviceInfo);
        }
    }
}
