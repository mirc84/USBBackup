using System.Windows;
using System.Windows.Controls;
using Ookii.Dialogs.Wpf;

namespace USBBackupGUI.Controls
{
    public delegate void ValueChangedEventHandler(object source, RoutedEventArgs e);

    public partial class FolderBrowseControl : UserControl
    {
        #region Constructor

        public FolderBrowseControl()
        {
            InitializeComponent();
        }

        #endregion

        #region Properties

        public static readonly DependencyProperty PathRestrictionProperty = DependencyProperty.Register(
            "PathRestriction", typeof(string), typeof(FolderBrowseControl), new PropertyMetadata(default(string)));

        public string PathRestriction
        {
            get { return (string)GetValue(PathRestrictionProperty); }
            set { SetValue(PathRestrictionProperty, value); }
        }

        public static readonly DependencyProperty SelectedPathProperty = DependencyProperty.Register(
            "SelectedPath", typeof(string), typeof(FolderBrowseControl), new PropertyMetadata(default(string), SelectedPathChanged));

        public string SelectedPath
        {
            get { return (string)GetValue(SelectedPathProperty); }
            set { SetValue(SelectedPathProperty, value); }
        }

        #endregion

        #region Events

        public event ValueChangedEventHandler ValueChanged;

        #endregion

        #region Non Public Methods

        private static void SelectedPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (FolderBrowseControl)d;
            ctrl.OnValueChanged();
        }

        private void OpenButton_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new VistaFolderBrowserDialog { SelectedPath = SelectedPath };
            if (dialog.ShowDialog().GetValueOrDefault())
            {
                while (PathRestriction != null && !dialog.SelectedPath.StartsWith(PathRestriction, System.StringComparison.InvariantCultureIgnoreCase))
                {
                    if (!dialog.ShowDialog().GetValueOrDefault())
                        return;
                }
                SelectedPath = dialog.SelectedPath;
            }
        }

        private void OnValueChanged()
        {
            ValueChanged?.Invoke(this, new RoutedEventArgs());
        }

        #endregion
    }
}
