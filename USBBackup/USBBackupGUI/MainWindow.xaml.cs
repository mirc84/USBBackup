using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Ookii.Dialogs.Wpf;
using USBBackup.Entities;
using USBBackupGUI.Controls;

namespace USBBackupGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _viewModel = e.NewValue as MainWindowViewModel;
            _viewModel.UserChoiceRequested += OnUserChoiceRequested;
        }

        private MessageBoxResult OnUserChoiceRequested(string message, string caption)
        {
            var result = MessageBox.Show(message, caption, MessageBoxButton.YesNo);
            return result;
        }

        private void Control_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void SourceButton_OnClick(object sender, RoutedEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (element == null)
                return;

            var backupInfo = element.DataContext as Backup;
            if (backupInfo == null)
            {
                element = element.FindAncestor<DataGridRow>();
                var device = element?.DataContext as Drive;
                if (device == null)
                    return;

                //backupInfo = new Backup();
                //device.Drives.Add(backupInfo);
            }

            var dialog = new VistaFolderBrowserDialog {SelectedPath = backupInfo.SourcePath};
            if (dialog.ShowDialog().GetValueOrDefault())
            {
                backupInfo.SourcePath = dialog.SelectedPath;
            }
        }

        private void TargetButton_OnClick(object sender, RoutedEventArgs e)
        {
            var backupInfo = (sender as FrameworkElement)?.DataContext as Backup;
            if (backupInfo == null)
                return;

            var dialog = new VistaFolderBrowserDialog {SelectedPath = backupInfo.SourcePath};
            if (dialog.ShowDialog().GetValueOrDefault())
            {
                backupInfo.SourcePath = dialog.SelectedPath;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            USBBackup.Properties.Settings.Default.Reload();
            var win = new SettingsWindow()
            {
                Owner = this,
                BackupInterval = USBBackup.Properties.Settings.Default.BackupInterval,
                BackupOnIntervals = USBBackup.Properties.Settings.Default.HandleBackupOnInterval,
                WatchBackupFolders = USBBackup.Properties.Settings.Default.WatchBackupSources
        };
            if (win.ShowDialog().GetValueOrDefault())
            {
                USBBackup.Properties.Settings.Default.BackupInterval = win.BackupInterval;
                USBBackup.Properties.Settings.Default.HandleBackupOnInterval = win.BackupOnIntervals;
                USBBackup.Properties.Settings.Default.WatchBackupSources = win.WatchBackupFolders;
                USBBackup.Properties.Settings.Default.Save();
            }
        }
    }
}
