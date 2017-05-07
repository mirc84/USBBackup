using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace USBBackup
{
    public delegate void USBDeviceAttachedHandler(USBDeviceInfo deviceInfo);

    class USBWatcher
    {
        private ManagementEventWatcher _insertWatcher;
        private ManagementEventWatcher _removeWatcher;

        public USBWatcher()
        {
            var insertQuery = new WqlEventQuery("SELECT * FROM __InstanceCreationEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_USBHub'");

            _insertWatcher = new ManagementEventWatcher(insertQuery);
            _insertWatcher.EventArrived += new EventArrivedEventHandler(DeviceInsertedEvent);
            _insertWatcher.Start();

            var removeQuery = new WqlEventQuery("SELECT * FROM __InstanceDeletionEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_USBHub'");
            _removeWatcher = new ManagementEventWatcher(removeQuery);
            _removeWatcher.EventArrived += new EventArrivedEventHandler(DeviceRemovedEvent);
            _removeWatcher.Start();
        }

        protected virtual void OnDeviceAttached(USBDeviceInfo deviceInfo)
        {
            DeviceAttached?.Invoke(deviceInfo);
        }

        public event USBDeviceAttachedHandler DeviceAttached;

        private void DeviceInsertedEvent(object sender, EventArrivedEventArgs e)
        {
            ManagementBaseObject instance = (ManagementBaseObject)e.NewEvent["TargetInstance"];
            foreach (var property in instance.Properties)
            {
                Console.WriteLine(property.Name + " = " + property.Value);
            }
        }

        private void DeviceRemovedEvent(object sender, EventArrivedEventArgs e)
        {
            ManagementBaseObject instance = (ManagementBaseObject)e.NewEvent["TargetInstance"];
            foreach (var property in instance.Properties)
            {
                Console.WriteLine(property.Name + " = " + property.Value);
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            
        }
    }
}
