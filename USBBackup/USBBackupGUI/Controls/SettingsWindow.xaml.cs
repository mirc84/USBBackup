using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using USBBackup;
using USBBackup.Strings;
using WPFLocalizeExtension.Engine;

namespace USBBackupGUI.Controls
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();

            _languageComboBox.ItemsSource = LocalizeDictionary.Instance.DefaultProvider.AvailableCultures.Where(x => x != CultureInfo.InvariantCulture).ToList();
            SelectedLanguage = Loc.CurrentCulture;
        }
        
        public CultureInfo SelectedLanguage
        {
            get { return (CultureInfo)GetValue(SelectedLanguageProperty); }
            set { SetValue(SelectedLanguageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Language.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedLanguageProperty =
            DependencyProperty.Register("Language", typeof(CultureInfo), typeof(SettingsWindow), new PropertyMetadata(default(CultureInfo)));
        
        public TimeSpan BackupInterval
        {
            get { return (TimeSpan)GetValue(BackupIntervalProperty); }
            set { SetValue(BackupIntervalProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BackupInterval.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BackupIntervalProperty =
            DependencyProperty.Register("BackupInterval", typeof(TimeSpan), typeof(SettingsWindow), new PropertyMetadata(default(TimeSpan)));
        
        public bool WatchBackupFolders
        {
            get { return (bool)GetValue(WatchBackupFoldersProperty); }
            set { SetValue(WatchBackupFoldersProperty, value); }
        }

        // Using a DependencyProperty as the backing store for WatchBackupFolders.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WatchBackupFoldersProperty =
            DependencyProperty.Register("WatchBackupFolders", typeof(bool), typeof(SettingsWindow), new PropertyMetadata(false));
        
        public bool CleanupRemovedFile
        {
            get { return (bool)GetValue(CleanupRemovedFileProperty); }
            set { SetValue(CleanupRemovedFileProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CleanupRemovedFile.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CleanupRemovedFileProperty =
            DependencyProperty.Register("CleanupRemovedFile", typeof(bool), typeof(SettingsWindow), new PropertyMetadata(false));
        
        public bool BackupOnIntervals
        {
            get { return (bool)GetValue(BackupOnIntervalsProperty); }
            set { SetValue(BackupOnIntervalsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BackupOnIntervals.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BackupOnIntervalsProperty =
            DependencyProperty.Register("BackupOnIntervals", typeof(bool), typeof(SettingsWindow), new PropertyMetadata(false));
        
        public bool NotifyBackupStarted
        {
            get { return (bool)GetValue(NotifyBackupStartedProperty); }
            set { SetValue(NotifyBackupStartedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NotifyBackupStartedProperty =
            DependencyProperty.Register("NotifyBackupStarted", typeof(bool), typeof(SettingsWindow), new PropertyMetadata(default(bool)));
        
        public bool NotifyBackupFinished
        {
            get { return (bool)GetValue(NotifyBackupFinishedProperty); }
            set { SetValue(NotifyBackupFinishedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NotifyBackupFinished.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NotifyBackupFinishedProperty =
            DependencyProperty.Register("NotifyBackupFinished", typeof(bool), typeof(SettingsWindow), new PropertyMetadata(default(bool)));
        
        public bool NotifyCleanupStarted
        {
            get { return (bool)GetValue(NotifyCleanupStartedProperty); }
            set { SetValue(NotifyCleanupStartedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NotifyCleanupStarted.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NotifyCleanupStartedProperty =
            DependencyProperty.Register("NotifyCleanupStarted", typeof(bool), typeof(SettingsWindow), new PropertyMetadata(default(bool)));
        
        public bool NotifyCleanupFinished
        {
            get { return (bool)GetValue(NotifyCleanupFinishedProperty); }
            set { SetValue(NotifyCleanupFinishedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NotifyCleanupFinished.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NotifyCleanupFinishedProperty =
            DependencyProperty.Register("NotifyCleanupFinished", typeof(bool), typeof(SettingsWindow), new PropertyMetadata(default(bool)));
        
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            var choice = MessageBox.Show(new Loc(nameof(StringResource.SettingsWindow_CancelQuestion)), 
                new Loc(nameof(StringResource.SettingsWindow_CancelQuestion_Caption)), MessageBoxButton.OKCancel);
            if (choice != MessageBoxResult.OK)
                return;

            DialogResult = false;
        }
    }    
}
