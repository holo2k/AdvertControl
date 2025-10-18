using System;
using System.Globalization;
using Avalonia.Data.Converters;

public class UrlToMediaPlayerConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string url && Uri.IsWellFormedUriString(url, UriKind.Absolute))
        {
            return new Uri(url); // Возвращаем URI для MediaPlayerControl
        }
        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return null;
    }
}