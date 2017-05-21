using System;
using System.Globalization;
using System.IO;
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

        [STAThread]
        public static int Main(params string[] args)
        {
            var app = new App();
            
            app.Run();

            return 0;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
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

            ShutdownMode = ShutdownMode.OnExplicitShutdown;

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

            var trayIcon = new TrayIcon(_window);
            trayIcon.RunBackupRequested += OnRunBackupRequested;
            trayIcon.PauseResumeBackupsRequested += OnPauseResumeBackupsRequested;
            trayIcon.CancelBackupsRequested += OnCancelBackupsRequested;

            _backupHandler.BackupStarted += trayIcon.OnNotifyBackupStarted;
            _backupHandler.BackupFinished += trayIcon.OnNotifyBackupFinished;
            _backupHandler.CleanupStarted += trayIcon.OnNotifyCleanupStarted;
            _backupHandler.CleanupFinished += trayIcon.OnNotifyCleanupFinished;
            _backupHandler.StateChanged += trayIcon.OnStateChanged;

            _deviceRepository.Load();
        }

        private void OnPauseResumeBackupsRequested(object sender, EventArgs e)
        {
            _backupHandler.PauseResumeBackups();
        }

        private void OnCancelBackupsRequested(object sender, EventArgs e)
        {
            _backupHandler.CancelBackups();
        }

        private void OnRunBackupRequested(object sender, EventArgs e)
        {
            _viewModel.RunAllBackupsCommand.Execute(null);
        }

        private void Shutdown(object sender, EventArgs eventArgs)
        {
            _backupHandler.CancelBackups();
            Shutdown();
        }
    }
}
