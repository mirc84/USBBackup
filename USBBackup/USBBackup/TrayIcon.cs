using System;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using USBBackup.Properties;

namespace USBBackup
{
    class TrayIcon
    {
        private MainWindow _window;
        private NotifyIcon _icon;
        private WindowState _lastState;

        public TrayIcon(MainWindow window)
        {
            _window = window;
            _icon = new NotifyIcon()
            {
                Icon = (Icon)Resources.Hopstarter_Soft_Scraps_USB,
                Visible = true
            };

            _icon.DoubleClick += OnIconDoubleClick;
            _window.StateChanged += OnWindowStateChanged;
            _lastState = _window.WindowState;
        }

        private void OnWindowStateChanged(object sender, EventArgs e)
        {
            if (_window.WindowState == WindowState.Minimized)
            {
                _window.ShowInTaskbar = false;
                return;
            }

            _lastState = _window.WindowState;
        }

        private void OnIconDoubleClick(object sender, EventArgs e)
        {
            if (_window.ShowInTaskbar)
                return;

            _window.ShowInTaskbar = true;
            _window.WindowState = _lastState;
        }
    }
}
