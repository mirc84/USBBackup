using System.Globalization;
using WPFLocalizeExtension.Engine;
using WPFLocalizeExtension.Extensions;

namespace USBBackup
{
    public class Loc
    {
        private string _key;
        private object[] _formatArguments;

        public Loc(string key, params object[] formatArguments)
        {
            _key = "USBBackup:StringResource:" + key;
            _formatArguments = formatArguments;
        }

        public static CultureInfo CurrentCulture
        {
            get { return CultureInfo.CurrentCulture; }
            set
            {
                CultureInfo.DefaultThreadCurrentCulture = value;
                LocalizeDictionary.Instance.Culture = value;
            }
        }

        public static implicit operator string(Loc loc)
        {
            return loc.ToString();
        }

        public override string ToString()
        {
            return string.Format(LocExtension.GetLocalizedValue<string>(_key), _formatArguments);
        }
    }
}
