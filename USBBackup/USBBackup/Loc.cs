using System.Globalization;
using WPFLocalizeExtension.Engine;
using WPFLocalizeExtension.Extensions;

namespace USBBackup
{
    public class Loc
    {
        #region Fields

        private string _key;
        private object[] _formatArguments;

        #endregion

        #region Constructor

        public Loc(string key, params object[] formatArguments)
        {
            _key = "USBBackup:StringResource:" + key;
            _formatArguments = formatArguments;
        }

        #endregion

        #region Properties

        public static CultureInfo CurrentCulture
        {
            get { return CultureInfo.CurrentCulture; }
            set
            {
                LocalizeDictionary.Instance.Culture = value;
                CultureInfo.DefaultThreadCurrentCulture = value;
            }
        }

        #endregion

        #region Public Methods

        public static implicit operator string(Loc loc)
        {
            return loc.ToString();
        }

        public override string ToString()
        {
            var locString = LocExtension.GetLocalizedValue<string>(_key);
            return string.Format(locString, _formatArguments);
        }

        #endregion
    }
}
