using System;
using System.Globalization;
using System.Windows.Data;

namespace GitLog
{
    public class MailToUriConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string s = value as string;
            if (s != null)
                return new Uri("mailto:" + s);
            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Uri u = value as Uri;
            if (u != null && u.Scheme == "mailto")
                return string.Format("{0}@{1}", u.UserInfo, u.Host);
            return Binding.DoNothing;
        }
    }
}
