using System;
using System.Windows;

namespace USBBackupGUI.Controls
{
    /// <summary>
    /// Interaction logic for TimeSpanInputControl.xaml
    /// </summary>
    public partial class TimeSpanInputControl
    {
        public TimeSpanInputControl()
        {
            InitializeComponent();
        }
        
        public TimeSpan TimeSpan
        {
            get { return (TimeSpan)GetValue(TimeSpanProperty); }
            set { SetValue(TimeSpanProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TimeSpan.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TimeSpanProperty =
            DependencyProperty.Register("TimeSpan", typeof(TimeSpan), typeof(TimeSpanInputControl), new PropertyMetadata(default(TimeSpan), OnTimeSpanChanged));

        public int Hours
        {
            get { return (int)GetValue(HoursProperty); }
            set { SetValue(HoursProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Hours.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HoursProperty =
            DependencyProperty.Register("Hours", typeof(int), typeof(TimeSpanInputControl), new PropertyMetadata(0, OnValueChanged));

        public int Minutes
        {
            get { return (int)GetValue(MinutesProperty); }
            set { SetValue(MinutesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Minutes.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinutesProperty =
            DependencyProperty.Register("Minutes", typeof(int), typeof(TimeSpanInputControl), new PropertyMetadata(0, OnValueChanged));



        public int Seconds
        {
            get { return (int)GetValue(SecondsProperty); }
            set { SetValue(SecondsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Seconds.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SecondsProperty =
            DependencyProperty.Register("Seconds", typeof(int), typeof(TimeSpanInputControl), new PropertyMetadata(0, OnValueChanged));
        private bool _updatingValues;

        private static void OnTimeSpanChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (TimeSpanInputControl)d;
            ctrl.SetValues();
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (TimeSpanInputControl)d;
            ctrl.SetTimeSpan();
        }

        private void SetValues()
        {
            _updatingValues = true;
            Hours = TimeSpan.Hours;
            Minutes = TimeSpan.Minutes;
            Seconds = TimeSpan.Seconds;
            _updatingValues = false;
        }

        private void SetTimeSpan()
        {
            if (!_updatingValues)
                TimeSpan = new TimeSpan(Hours, Minutes, Seconds);
        }

    }
}
