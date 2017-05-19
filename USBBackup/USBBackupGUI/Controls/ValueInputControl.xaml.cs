using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace USBBackupGUI.Controls
{
    /// <summary>
    /// Interaction logic for ValueInputControl.xaml
    /// </summary>
    public partial class ValueInputControl : UserControl
    {
        public ValueInputControl()
        {
            InitializeComponent();
        }

        public int Value
        {
            get { return (int)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(int), typeof(ValueInputControl), new PropertyMetadata(0, OnValueChanged));



        public int MaxValue
        {
            get { return (int)GetValue(MaxValueProperty); }
            set { SetValue(MaxValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MaxValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxValueProperty =
            DependencyProperty.Register("MaxValue", typeof(int), typeof(ValueInputControl), new PropertyMetadata(0));

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (ValueInputControl)d;
            ctrl.CheckValueLimits();
        }

        private void CheckValueLimits()
        {
            if (Value > MaxValue)
                Value = MaxValue;
        }

        private void TextBox_TextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, "^\\d*$");
        }
    }

    class IntToStringConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var number = value as int?;
            return number?.ToString() ?? "0";
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var text = value as string;
            int number;
            if (int.TryParse(text, out number))
                return number;

            return 0;
        }
    }
}
