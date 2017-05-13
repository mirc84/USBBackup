using System.Windows;
using System.Windows.Media;

namespace USBBackup
{
    public static class UIElementExtension
    {
        public static T FindAncestor<T>(this FrameworkElement element) where T : class
        {
            var parent = element.Parent;
            while (parent != null)
            {
                var result = parent as T;
                if (result != null)
                    return result;

                parent = VisualTreeHelper.GetParent(parent);
            }
            return null;
        }
    }
}