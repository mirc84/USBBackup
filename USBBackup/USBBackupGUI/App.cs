using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using USBBackup;
using USBBackup.DatabaseAccess;

namespace USBBackupGUI
{
    public class App : Application
    {
        private USBWatcher _watcher;
        private UsbDeviceRepository _deviceRepository;
        private DatabaseConnection _databaseContext;
        private BackupHandler _backupHandler;
        private static MainWindow _window;
        private MainWindowViewModel _viewModel;
        private TrayIcon _trayIcon;

        public App()
        {
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
        }

        [STAThread]
        public static int Main(params string[] args)
        {
            var app = new App();
            
            app.Run();

            return 0;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            var @event = new EventWaitHandle(false, EventResetMode.ManualReset, "Global_MKayBackupStartup", out bool created);
            if (!created)
            {
                @event.Set();
                Shutdown();
                return;
            }

            base.OnStartup(e);
            try
            {
                if (string.IsNullOrEmpty(USBBackup.Properties.Settings.Default.Language))
                {
                    Loc.CurrentCulture = CultureInfo.CurrentUICulture;
                    USBBackup.Properties.Settings.Default.Language = Loc.CurrentCulture.Name;
                    USBBackup.Properties.Settings.Default.Save();
                }
                else
                {
                    Loc.CurrentCulture = CultureInfo.GetCultureInfo(USBBackup.Properties.Settings.Default.Language);
                }
            }
            catch (Exception)
            {
                Loc.CurrentCulture = CultureInfo.CurrentUICulture;
            }


            _watcher = new USBWatcher();
            _watcher.Init();
            _backupHandler = new BackupHandler();
            var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "USBBackup/backup.db");
            var directory = Path.GetDirectoryName(dbPath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            _databaseContext = new DatabaseConnection(dbPath);
            _deviceRepository = new UsbDeviceRepository(_watcher, _databaseContext, _backupHandler, Dispatcher.CurrentDispatcher);

            _viewModel = new MainWindowViewModel(_deviceRepository, _backupHandler);
            _window = new MainWindow()
            {
                DataContext = _viewModel,
            };
            _window.Visibility = Visibility.Hidden;
            _window.ShowInTaskbar = false;
            _window.Closed += Shutdown;

            _trayIcon = new TrayIcon(_window);
            _trayIcon.RunBackupRequested += OnRunBackupRequested;
            _trayIcon.PauseResumeBackupsRequested += OnPauseResumeBackupsRequested;
            _trayIcon.CancelBackupsRequested += OnCancelBackupsRequested;

            _backupHandler.BackupStarted += _trayIcon.OnNotifyBackupStarted;
            _backupHandler.BackupFinished += _trayIcon.OnNotifyBackupFinished;
            _backupHandler.CleanupStarted += _trayIcon.OnNotifyCleanupStarted;
            _backupHandler.CleanupFinished += _trayIcon.OnNotifyCleanupFinished;
            _backupHandler.StateChanged += _trayIcon.OnStateChanged;

            _deviceRepository.Load();
            WaitStartHandle(@event);
        }

        private void WaitStartHandle(EventWaitHandle @event)
        {
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    @event.WaitOne();
                    Dispatcher.BeginInvoke(new Action(_trayIcon.ShowWindow));
                    @event.Reset();
                }
            });
        }

        private void OnPauseResumeBackupsRequested(object sender, EventArgs e)
        {
            _backupHandler.PauseResumeBackups();
        }

        private void OnCancelBackupsRequested(object sender, EventArgs e)
        {
            var choice = MessageBox.Show("Cancel ", "Cancel", MessageBoxButton.YesNoCancel);
            if (choice == MessageBoxResult.Cancel)
                return;

            _backupHandler.CancelBackups(choice == MessageBoxResult.Yes);
        }

        private void OnRunBackupRequested(object sender, EventArgs e)
        {
            _viewModel.RunAllBackupsCommand.Execute(null);
        }

        private void Shutdown(object sender, EventArgs eventArgs)
        {
            _backupHandler.CancelBackups(true);
            Shutdown();
        }
    }
}
