using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using USBBackup.Properties;

namespace USBBackup
{
    class TrayIcon
    {
        private NotifyIcon _icon;

        public TrayIcon()
        {
            _icon = new NotifyIcon()
            {
                Icon = (Icon)Resources.Hopstarter_Soft_Scraps_USB,
                ContextMenu = new ContextMenu()
            };
        }
    }
}
