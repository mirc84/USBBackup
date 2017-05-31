using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using USBBackup.Entities;

namespace USBBackupGUI.Controls
{
    /// <summary>
    /// Interaction logic for BackupControl.xaml
    /// </summary>
    public partial class BackupControl : UserControl
    {
        #region Constructor

        public BackupControl()
        {
            InitializeComponent();
        }

        #endregion

        #region Properties

        public Backup Backup
        {
            get { return (Backup)GetValue(BackupProperty); }
            set { SetValue(BackupProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Backup.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BackupProperty =
            DependencyProperty.Register("Backup", typeof(Backup), typeof(BackupControl), new PropertyMetadata(null));

        public ICommand RunPauseResumeBackupCommand
        {
            get { return (ICommand)GetValue(RunPauseResumeBackupCommandProperty); }
            set { SetValue(RunPauseResumeBackupCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RunPauseResumeBackupCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RunPauseResumeBackupCommandProperty =
            DependencyProperty.Register("RunPauseResumeBackupCommand", typeof(ICommand), typeof(BackupControl), new PropertyMetadata(default(ICommand)));

        public ICommand CancelBackupCommand
        {
            get { return (ICommand)GetValue(CancelBackupCommandProperty); }
            set { SetValue(CancelBackupCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CancelBackupCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CancelBackupCommandProperty =
            DependencyProperty.Register("CancelBackupCommand", typeof(ICommand), typeof(BackupControl), new PropertyMetadata(default(ICommand)));

        public ICommand RemoveBackupCommand
        {
            get { return (ICommand)GetValue(RemoveBackupCommandProperty); }
            set { SetValue(RemoveBackupCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RemoveBackupCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RemoveBackupCommandProperty =
            DependencyProperty.Register("RemoveBackupCommand", typeof(ICommand), typeof(BackupControl), new PropertyMetadata(default(ICommand)));

        #endregion

        #region Non Public Methods

        private void FolderBrowseControl_ValueChanged(object source, RoutedEventArgs e)
        {
            var ctrl = source as FolderBrowseControl;
            if (ctrl == null)
                return;

            var container = ctrl.Parent as Panel;
            if (container == null)
                return;

            foreach (var element in container.Children.OfType<FolderBrowseControl>())
            {
                element.GetBindingExpression(FolderBrowseControl.SelectedPathProperty)?.UpdateSource();
            }
        }

        #endregion
    }

    internal class AreNotEqualToVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2 || Equals(values[0], values[1]))
                return Visibility.Collapsed;
            return Visibility.Visible;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
