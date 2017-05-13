using System.Windows;
using System.Windows.Controls;
using Ookii.Dialogs.Wpf;

namespace USBBackup.Controls
{
    public partial class FolderBrowseControl : UserControl
    {
        public FolderBrowseControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty SelectedPathProperty = DependencyProperty.Register(
            "SelectedPath", typeof(string), typeof(FolderBrowseControl), new PropertyMetadata(default(string)));

        public string SelectedPath
        {
            get { return (string) GetValue(SelectedPathProperty); }
            set { SetValue(SelectedPathProperty, value); }
        }

        private void OpenButton_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new VistaFolderBrowserDialog {SelectedPath = SelectedPath};
            if (dialog.ShowDialog().GetValueOrDefault())
            {
                SelectedPath = dialog.SelectedPath;
            }
        }
    }
}
