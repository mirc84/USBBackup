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
                ContextMenu = new ContextMenu(),
                Visible = true
            };
            SetMenuItems();

            _icon.DoubleClick += OnIconDoubleClick;
            _window.StateChanged += OnWindowStateChanged;
            _lastState = _window.WindowState;
        }

        public event EventHandler RunBackupRequested;

        private void SetMenuItems()
        {
            var runItem = new MenuItem()
            {
                Name = "Run Backup",
                Text = "Run",
            };
            runItem.Click += OnRunBackupRequested;

            var showWindowItem = new MenuItem()
            {
                Name = "Open",
                Text = "Open",
                DefaultItem = true
            };
            showWindowItem.Click += (_, __) => ShowWindow();

            var closeItem = new MenuItem()
            {
                Name = "Close",
                Text = "Close",
            };
            closeItem.Click += (_, __) => _window.Close();

            _icon.ContextMenu.MenuItems.Add(showWindowItem);
            _icon.ContextMenu.MenuItems.Add(runItem);
            _icon.ContextMenu.MenuItems.Add(closeItem);
        }

        private void OnRunBackupRequested(object sender, EventArgs e)
        {
            RunBackupRequested?.Invoke(this, EventArgs.Empty);
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
            ShowWindow();
        }

        private void ShowWindow()
        {
            if (_window.ShowInTaskbar)
                return;

            _window.ShowInTaskbar = true;
            _window.WindowState = _lastState;
        }
    }
}
