using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
                var culture = value;
                LocalizeDictionary.Instance.SetCultureCommand.Execute(culture);
                CultureInfo.DefaultThreadCurrentCulture = culture;
            }
        }

        public static implicit operator string(Loc loc)
        {
            return loc.ToString();
        }

        public override string ToString()
        {
            var locString = LocExtension.GetLocalizedValue<string>(_key);
            return string.Format(locString, _formatArguments);
        }
    }
}
