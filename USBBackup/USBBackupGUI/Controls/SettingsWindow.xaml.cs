using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using USBBackup;
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

            var cultures = new HashSet<CultureInfo>
            {
                CultureInfo.GetCultureInfo("en-US")
            };
            foreach (var culture in LocalizeDictionary.Instance.DefaultProvider.AvailableCultures.Where(x => x != CultureInfo.InvariantCulture))
            {
                cultures.Add(culture);
            }
            _languageComboBox.ItemsSource = cultures;
            SelectedLanguage = Loc.CurrentCulture;
        }


        public CultureInfo SelectedLanguage
        {
            get { return (CultureInfo)GetValue(LanguageProperty); }
            set { SetValue(LanguageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Language.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LanguageProperty =
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

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            var choice = MessageBox.Show(new Loc("SettingsWindow_CancelQuestion"), new Loc("SettingsWindow_CancelQuestion_Caption"), MessageBoxButton.OKCancel);
            if (choice != MessageBoxResult.OK)
                return;

            DialogResult = false;
        }
    }    
}
