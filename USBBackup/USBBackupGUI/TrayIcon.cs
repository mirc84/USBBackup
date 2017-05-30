using System;
using System.Windows;
using System.Windows.Forms;
using USBBackup;
using USBBackup.Strings;
using USBBackupGUI.Resources;

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
                Icon = ImageResource.Hopstarter_Soft_Scraps_USB1,
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
                Text = new Loc(nameof(StringResource.TrayIcon_RunBackups)),
            };
            runItem.Click += OnRunBackupRequested;

            _pauseItem = new MenuItem()
            {
                Text = new Loc(nameof(StringResource.TrayIcon_PauseBackups)),
            };
            _pauseItem.Click += OnPauseBackupsRequested;

            _cancelItem = new MenuItem()
            {
                Text = new Loc(nameof(StringResource.TrayIcon_CancelBackups))
            };
            _cancelItem.Click += OnCancelBackupsRequested;

            var showWindowItem = new MenuItem()
            {
                Text = new Loc(nameof(StringResource.TrayIcon_Open)),
                DefaultItem = true
            };
            showWindowItem.Click += (_, __) => ShowWindow();

            var closeItem = new MenuItem()
            {
                Text = new Loc(nameof(StringResource.TrayIcon_Close))
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
                    _pauseItem.Text = new Loc(nameof(StringResource.TrayIcon_PauseBackups));
                    _cancelItem.Enabled = true;
                    break;
                case BackupState.Paused:
                    _pauseItem.Enabled = true;
                    _pauseItem.Text = new Loc(nameof(StringResource.TrayIcon_ResumeBackups));
                    _cancelItem.Enabled = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Backup state {state} unknown.");
            }
        }

        internal void OnNotifyCleanupStarted(IBackup backup)
        {
            if (!USBBackup.Properties.Settings.Default.NotifyCleanupStarted)
                return;

            _icon.ShowBalloonTip(500, new Loc(nameof(StringResource.TrayIcon_CleanUpStarted_Caption)), 
                new Loc(nameof(StringResource.TrayIcon_CleanUpStarted), backup.TargetPath), ToolTipIcon.Info);
        }

        internal void OnNotifyCleanupFinished(IBackup backup)
        {
            if (!USBBackup.Properties.Settings.Default.NotifyCleanupFinished)
                return;

            _icon.ShowBalloonTip(500, new Loc(nameof(StringResource.TrayIcon_CleanUpFinished_Caption)), 
                new Loc(nameof(StringResource.TrayIcon_CleanUpFinished), backup.TargetPath), ToolTipIcon.Info);
        }

        internal void OnNotifyBackupFinished(IBackup backup)
        {
            if (!USBBackup.Properties.Settings.Default.NotifyBackupFinished)
                return;

            _icon.ShowBalloonTip(500, new Loc(nameof(StringResource.TrayIcon_BackupFinished_Caption)), 
                new Loc(nameof(StringResource.TrayIcon_BackupFinished), backup.TargetPath), ToolTipIcon.Info);

        }

        internal void OnNotifyBackupStarted(IBackup backup)
        {
            if (!USBBackup.Properties.Settings.Default.NotifyBackupStarted)
                return;

            _icon.ShowBalloonTip(500, new Loc(nameof(StringResource.TrayIcon_BackupStarted_Caption)), 
                new Loc(nameof(StringResource.TrayIcon_BackupStarted), backup.TargetPath), ToolTipIcon.Info);
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

        public void ShowWindow()
        {
            if (_window.ShowInTaskbar)
                return;

            _window.ShowInTaskbar = true;
            _window.WindowState = _lastState;
            _window.Visibility = Visibility.Visible;
        }
    }
}
