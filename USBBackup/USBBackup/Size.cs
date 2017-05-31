using System;

namespace USBBackup
{
    public struct Size
    {
        #region Fields

        private double _stringValue;
        private string _unit;

        #endregion

        #region Constructor

        public Size(long value)
        {
            Value = value;
            _stringValue = value;
            _unit = "UNKNOWN";

            UpdateStringValue();
        }

        #endregion
        
        #region Properties

        public long Value { get; set; }

        #endregion

        #region Operators

        public static implicit operator Size(long value)
        {
            return new Size(value);
        }

        public static implicit operator long(Size value)
        {
            return value.Value;
        }

        #endregion

        #region Public Methods

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

        #endregion

        #region Non Public Methods

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

        #endregion
    }
}
