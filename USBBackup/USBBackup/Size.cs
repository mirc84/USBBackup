using System;

namespace USBBackup
{
    public struct Size
    {
        private double _stringValue;
        private string _unit;

        public Size(long value)
        {
            Value = value;
            _stringValue = value;
            _unit = "UNKNOWN";

            UpdateStringValue();
        }

        public long Value { get; set; }

        public static implicit operator Size(long value)
        {
            return new Size(value);
        }

        public static implicit operator long(Size value)
        {
            return value.Value;
        }

        private void UpdateStringValue()
        {
            var iterations = 0;
            while (_stringValue > 1024)
            {
                _stringValue /= 1024;
                iterations++;
            }


            var unit = "UNKNOWN";
            var decimals = 0;
            switch (iterations)
            {
                case 0:
                    unit = "B";
                    break;
                case 1:
                    unit = "kB";
                    decimals = 1;
                    break;
                case 2:
                    unit = "MB";
                    decimals = 1;
                    break;
                case 3:
                    unit = "GB";
                    decimals = 2;
                    break;
                case 4:
                    unit = "TB";
                    decimals = 2;
                    break;
            }
            _unit = unit;
            _stringValue = Math.Round(_stringValue, decimals);
        }

        public override bool Equals(object obj)
        {
            var size = obj as Size?;
            return size != null && size.Value == Value;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return $"{_stringValue} {_unit}";
        }
    }
}
