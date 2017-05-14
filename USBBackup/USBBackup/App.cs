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

        [STAThread]
        public static int Main(params string[] args)
        {
            var app = new App();
            app.Start();
            var viewModel = new MainWindowViewModel(app._deviceRepository, app._backupHandler);
            var window = new MainWindow()
            {
                DataContext = viewModel
            };
            window.Closed += Shutdown;

            var trayIcon = new TrayIcon(window);
            window.ShowDialog();

            return 0;
        }

        private void Start()
        {
            _backupHandler = new BackupHandler();
            _watcher = new USBWatcher();
            _watcher.Init();
            _databaseContext = new DatabaseConnection("db.db");
            _deviceRepository = new UsbDeviceRepository(_watcher, _databaseContext, _backupHandler, Dispatcher.CurrentDispatcher);
            _deviceRepository.Load();
        }

        private static void Shutdown(object sender, EventArgs eventArgs)
        {

        }
    }
}
