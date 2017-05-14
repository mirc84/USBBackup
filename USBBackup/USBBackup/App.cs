using System;
using System.Windows.Threading;
using USBBackup.DatabaseAccess;

namespace USBBackup
{
    class App
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
            app.Start();

            _window.ShowDialog();

            return 0;
        }

        private void Start()
        {
            _watcher = new USBWatcher();
            _watcher.Init();
            _backupHandler = new BackupHandler();
            _databaseContext = new DatabaseConnection("backup.db");
            _deviceRepository = new UsbDeviceRepository(_watcher, _databaseContext, _backupHandler, Dispatcher.CurrentDispatcher);
            _deviceRepository.Load();

            _viewModel = new MainWindowViewModel(_deviceRepository, _backupHandler);
            _window = new MainWindow()
            {
                DataContext = _viewModel,                
            };
            _window.WindowState = System.Windows.WindowState.Minimized;
            _window.ShowInTaskbar = false;
            _window.Closed += Shutdown;

            var trayIcon = new TrayIcon(_window);
            trayIcon.RunBackupRequested += OnRunBackupRequested;
        }

        private void OnRunBackupRequested(object sender, EventArgs e)
        {
            _viewModel.RunAllBackupsCommand.Execute(null);
        }

        private static void Shutdown(object sender, EventArgs eventArgs)
        {

        }
    }
}
