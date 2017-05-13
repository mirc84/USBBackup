using System.Collections.Generic;
using USBBackup.Core;
using USBBackup.Entities;

namespace USBBackup
{
    class MainWindowViewModel : NotificationObject
    {
        private readonly UsbDeviceRepository _usbDeviceRepository;

        public MainWindowViewModel(UsbDeviceRepository usbDeviceRepository)
        {
            _usbDeviceRepository = usbDeviceRepository;
        }

        public IList<USBDeviceInfo> UsbDevices => _usbDeviceRepository.USBDevices;
    }
}
