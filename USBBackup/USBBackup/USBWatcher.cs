using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management;
using USBBackup.Core;
using USBBackup.Entities;

namespace USBBackup
{
    public delegate void USBDeviceAttachedHandler(USBDeviceInfo deviceInfo);

    internal class USBWatcher : NotificationObject
    {
        private ManagementEventWatcher _insertWatcher;
        private ManagementEventWatcher _removeWatcher;

        public USBWatcher()
        {
        }

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

        protected virtual void OnDeviceAttached(USBDeviceInfo deviceInfo)
        {
            DeviceAttached?.Invoke(deviceInfo);
        }

        protected virtual void OnDeviceDetached(USBDeviceInfo deviceInfo)
        {
            DeviceDetached?.Invoke(deviceInfo);
        }

        public event USBDeviceAttachedHandler DeviceAttached;
        public event USBDeviceAttachedHandler DeviceDetached;

        public IEnumerable<USBDeviceInfo> LoadUSBDevices()
        {
            ManagementObjectCollection collection;
            using (var searcher = new ManagementObjectSearcher(@"Select * From Win32_USBHub"))
            {
                collection = searcher.Get();
            }

            foreach (var item in collection)
            {
                yield return CreateUSBDeviceInfo(item);
            }
        }

        private void DeviceRemovedEvent(object sender, EventArrivedEventArgs e)
        {
            var instance = (ManagementBaseObject)e.NewEvent["TargetInstance"];
            var deviceId = (string) instance["DeviceID"];

            var deviceInfo = new USBDeviceInfo(deviceId, (string) instance["PNPDeviceID"], (string) instance["Description"]);
            OnDeviceDetached(deviceInfo);
        }

        private void DeviceInsertedEvent(object sender, EventArrivedEventArgs e)
        {
            var instance = (ManagementBaseObject)e.NewEvent["TargetInstance"];
            var device = CreateUSBDeviceInfo(instance);

            var query = $"SELECT * FROM Win32_LogicalDisk WHERE DeviceID='{device.DeviceID.Replace(@"\", @"\\")}'";
            var diskCollection = new ManagementObjectSearcher(query).Get().Cast<ManagementBaseObject>();
            foreach (var disk in diskCollection)
            {
                var name = (string)disk.GetPropertyValue("Name");
            }

            OnDeviceAttached(device);
        }

        private static USBDeviceInfo CreateUSBDeviceInfo(ManagementBaseObject device)
        {
            return new USBDeviceInfo(
                    (string)device.GetPropertyValue("DeviceID"),
                    (string)device.GetPropertyValue("PNPDeviceID"),
                    (string)device.GetPropertyValue("Description"));
        }
    }
}
