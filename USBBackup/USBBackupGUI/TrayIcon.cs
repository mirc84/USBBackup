using System;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using USBBackup;
using USBBackupGUI.Properties;

namespace USBBackupGUI
{
    class TrayIcon
    {
        private MainWindow _window;
        private NotifyIcon _icon;
        private WindowState _lastState;
        private MenuItem _pauseItem;
        private MenuItem _cancelItem;
        private bool _areBackupsRunning;

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
            _lastState = WindowState.Normal;

            _window.Closed += OnClosing;
        }

        private void OnClosing(object sender, EventArgs e)
        {
            _icon.Visible = false;
            _icon.Dispose();
        }
        
        public bool AreBackupsRunning
        {
            get { return _areBackupsRunning; }
            set
            {
                _areBackupsRunning = value;
                _pauseItem.Enabled = value;
                _cancelItem.Enabled = value;
            }
        }

        public event EventHandler RunBackupRequested;
        public event EventHandler PauseResumeBackupsRequested;
        public event EventHandler CancelBackupsRequested;

        private void SetMenuItems()
        {
            var runItem = new MenuItem()
            {
                Name = "Run Backup",
                Text = "Run",
            };
            runItem.Click += OnRunBackupRequested;

            _pauseItem = new MenuItem()
            {
                Name = "Pause Backup",
                Text = "Pause",
            };
            _pauseItem.Click += OnPauseBackupsRequested;

            _cancelItem = new MenuItem()
            {
                Name = "Cancel Backup",
                Text = "Cancel",
            };
            _cancelItem.Click += OnCancelBackupsRequested;

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
            _icon.ContextMenu.MenuItems.Add("-");
            _icon.ContextMenu.MenuItems.Add(runItem);
            _icon.ContextMenu.MenuItems.Add(_pauseItem);
            _icon.ContextMenu.MenuItems.Add(_cancelItem);
            _icon.ContextMenu.MenuItems.Add("-");
            _icon.ContextMenu.MenuItems.Add(closeItem);
        }

        internal void OnStateChanged(BackupState state)
        {
            switch (state)
            {
                case BackupState.Idle:
                    _cancelItem.Enabled = false;
                    _pauseItem.Enabled = false;
                    break;
                case BackupState.Running:
                    _pauseItem.Enabled = true;
                    _pauseItem.Text = "Pause";
                    _cancelItem.Enabled = true;
                    break;
                case BackupState.Paused:
                    _pauseItem.Enabled = true;
                    _pauseItem.Text = "Resume";
                    _cancelItem.Enabled = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Backup state {state} unknown.");
            }
        }

        internal void OnNotifyCleanupStarted(IBackup backup)
        {
            _icon.ShowBalloonTip(500, "Clean Up Started", $"Clean up of {backup.TargetPath} started.", ToolTipIcon.Info);
        }

        internal void OnNotifyCleanupFinished(IBackup backup)
        {
            _icon.ShowBalloonTip(500, "Clean Up Finished", $"Clean up of {backup.TargetPath} finished.", ToolTipIcon.Info);
        }

        internal void OnNotifyBackupFinished(IBackup backup)
        {
            _icon.ShowBalloonTip(500, "Backup Finished", $"Backup to {backup.TargetPath} finished.", ToolTipIcon.Info);

        }

        internal void OnNotifyBackupStarted(IBackup backup)
        {
            _icon.ShowBalloonTip(500, "Starting Backup", $"Starting backup to {backup.TargetPath}.", ToolTipIcon.Info);
        }

        private void OnCancelBackupsRequested(object sender, EventArgs e)
        {
            CancelBackupsRequested?.Invoke(sender, e);
        }

        private void OnPauseBackupsRequested(object sender, EventArgs e)
        {
            PauseResumeBackupsRequested?.Invoke(sender, e);
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
                _window.Visibility = Visibility.Hidden;
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
            _window.Visibility = Visibility.Visible;
        }
    }
}
