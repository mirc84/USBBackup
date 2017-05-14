using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management;
using USBBackup.Core;
using USBBackup.Entities;

namespace USBBackup
{
    internal delegate void USBDeviceAttachedHandler(USBDevice deviceInfo);

    internal class USBWatcher : NotificationObject
    {
        private ManagementEventWatcher _insertWatcher;
        private ManagementEventWatcher _removeWatcher;

        public USBWatcher()
        {
        }

        public event USBDeviceAttachedHandler DeviceAttached;
        public event USBDeviceAttachedHandler DeviceDetached;

        public void Init()
        {
            var insertQuery = new WqlEventQuery("SELECT * FROM __InstanceCreationEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_USBHub'");

            _insertWatcher = new ManagementEventWatcher(insertQuery);
            _insertWatcher.EventArrived += DeviceInsertedEvent;
            _insertWatcher.Start();

            var removeQuery = new WqlEventQuery("SELECT * FROM __InstanceDeletionEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_USBHub'");
            _removeWatcher = new ManagementEventWatcher(removeQuery);
            _removeWatcher.EventArrived += DeviceRemovedEvent;
            _removeWatcher.Start();
        }

        public IEnumerable<USBDevice> LoadUSBDevices()
        {
            ManagementObjectCollection collection;
            using (var searcher = new ManagementObjectSearcher(@"Select * From Win32_USBHub"))
            {
                collection = searcher.Get();
            }

            foreach (var item in collection.OfType<ManagementObject>())
            {
                yield return CreateUSBDeviceInfo(item);
            }
        }

        public IEnumerable<USBDevice> LoadUSBDevices(string deviceID)
        {
            ManagementObjectCollection collection;
            using (var searcher = new ManagementObjectSearcher($"Select * From Win32_USBHub WHERE DeviceID='{deviceID.Replace("\\","\\\\")}'"))
            {
                collection = searcher.Get();
            }

            foreach (var item in collection.OfType<ManagementObject>())
            {
                yield return CreateUSBDeviceInfo(item);
            }
        }

        public static IEnumerable<Drive> GetDrives(USBDevice device)
        {
            foreach (var controller in device.ManagementObject?.GetRelated("Win32_USBController").Cast<ManagementObject>() ?? new ManagementObject[0])
            {
                foreach (var obj in new ManagementObjectSearcher("ASSOCIATORS OF {Win32_USBController.DeviceID='" + controller["PNPDeviceID"].ToString() + "'}").Get().OfType<ManagementObject>())
                {
                    if (!obj.ToString().Contains("DeviceID"))
                        continue;
                    var devId = obj["DeviceID"].ToString();
                    if (!devId.Contains("USBSTOR"))
                        continue;

                    var drives = new ManagementObjectSearcher("select * from Win32_DiskDrive").Get().Cast<ManagementBaseObject>().Where(x => x["PNPDeviceID"].ToString() == devId);

                    foreach (var drive in drives.OfType<ManagementObject>())
                    {
                        foreach (var partition in drive.GetRelated("Win32_DiskPartition").OfType<ManagementObject>())
                        {
                            foreach (var logicalDrive in partition.GetRelated("Win32_LogicalDisk").OfType<ManagementObject>())
                            {
                                var volumeSerialNumber = logicalDrive["VolumeSerialNumber"].ToString();
                                var driveLetter = logicalDrive["Name"].ToString();
                                var name = logicalDrive["VolumeName"].ToString();
                                yield return new Drive(driveLetter, name, volumeSerialNumber);
                            }
                        }
                    }
                }
            }
        }

        protected virtual void OnDeviceAttached(USBDevice deviceInfo)
        {
            DeviceAttached?.Invoke(deviceInfo);
        }

        protected virtual void OnDeviceDetached(USBDevice deviceInfo)
        {
            DeviceDetached?.Invoke(deviceInfo);
        }

        private void DeviceRemovedEvent(object sender, EventArrivedEventArgs e)
        {
            var instance = e.NewEvent["TargetInstance"] as ManagementBaseObject;
            if (instance == null)
                return;

            var deviceId = instance["DeviceID"].ToString();
            var devices = LoadUSBDevices(deviceId);
            foreach (var device in devices)
                OnDeviceAttached(device);

            //var deviceInfo = CreateUSBDeviceInfo(instance);
            //OnDeviceDetached(deviceInfo);
        }

        private void DeviceInsertedEvent(object sender, EventArrivedEventArgs e)
        {
            var instance = e.NewEvent["TargetInstance"] as ManagementBaseObject;
            if (instance == null)
                return;

            var deviceId = instance["DeviceID"].ToString();
            var devices = LoadUSBDevices(deviceId);
            foreach(var device in devices)
                OnDeviceAttached(device);
        }

        private static USBDevice CreateUSBDeviceInfo(ManagementObject obj)
        {
            var device = new USBDevice(
                    (string)obj.GetPropertyValue("DeviceID"),
                    (string)obj.GetPropertyValue("PNPDeviceID"),
                    (string)obj.GetPropertyValue("Description"), obj);

            device.Drives = GetDrives(device).ToList();
            return device;
        }
    }
}
