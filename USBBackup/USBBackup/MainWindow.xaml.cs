﻿using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using USBBackup.Entities;

namespace USBBackup
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
        }

        private void Control_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void SourceButton_OnClick(object sender, RoutedEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (element == null)
                return;

            var backupInfo = element.DataContext as BackupInfo;
            if (backupInfo == null)
            {
                element = element.FindAncestor<DataGridRow>();
                var device = element?.DataContext as USBDeviceInfo;
                if (device == null)
                    return;

                backupInfo = new BackupInfo();
                device.Backups.Add(backupInfo);
            }

            var dialog = new VistaFolderBrowserDialog {SelectedPath = backupInfo.SourcePath};
            if (dialog.ShowDialog().GetValueOrDefault())
            {
                backupInfo.SourcePath = dialog.SelectedPath;
            }
        }

        private void TargetButton_OnClick(object sender, RoutedEventArgs e)
        {
            var backupInfo = (sender as FrameworkElement)?.DataContext as BackupInfo;
            if (backupInfo == null)
                return;

            var dialog = new VistaFolderBrowserDialog {SelectedPath = backupInfo.SourcePath};
            if (dialog.ShowDialog().GetValueOrDefault())
            {
                backupInfo.SourcePath = dialog.SelectedPath;
            }
        }
    }
}