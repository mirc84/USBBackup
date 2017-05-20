using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Ookii.Dialogs.Wpf;
using USBBackup.Entities;
using USBBackupGUI.Controls;
using System.ComponentModel;
using System;

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
            Closing += OnClosing;
        }

        private void OnClosing(object sender, CancelEventArgs args)
        {
            var choice = MessageBox.Show("Do you want to save your changes?", "Save Changes?", MessageBoxButton.YesNoCancel);
            if (choice == MessageBoxResult.Yes)
                _viewModel.SaveCommand.Execute(null);

            args.Cancel = choice == MessageBoxResult.Cancel;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _viewModel = e.NewValue as MainWindowViewModel; 
            _viewModel.UserChoiceRequested += OnUserChoiceRequested;
            _viewModel.UserNotification += OnUserNotification;
        }

        private void OnUserNotification(string message, string caption)
        {
            MessageBox.Show(message, caption, MessageBoxButton.OK);
        }

        private MessageBoxResult OnUserChoiceRequested(string message, string caption)
        {
            var result = MessageBox.Show(message, caption, MessageBoxButton.YesNo);
            return result;
        }
        
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            USBBackup.Properties.Settings.Default.Reload();
            var win = new SettingsWindow()
            {
                Owner = this,
                BackupInterval = USBBackup.Properties.Settings.Default.BackupInterval,
                BackupOnIntervals = USBBackup.Properties.Settings.Default.HandleBackupOnInterval,
                WatchBackupFolders = USBBackup.Properties.Settings.Default.WatchBackupSources,
                CleanupRemovedFile = USBBackup.Properties.Settings.Default.CleanupRemovedFile,
            };
            if (win.ShowDialog().GetValueOrDefault())
            {
                USBBackup.Properties.Settings.Default.BackupInterval = win.BackupInterval;
                USBBackup.Properties.Settings.Default.HandleBackupOnInterval = win.BackupOnIntervals;
                USBBackup.Properties.Settings.Default.WatchBackupSources = win.WatchBackupFolders;
                USBBackup.Properties.Settings.Default.CleanupRemovedFile = win.CleanupRemovedFile;
                USBBackup.Properties.Settings.Default.Save();
            }
        }
    }
}
