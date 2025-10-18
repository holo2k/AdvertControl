using System;
using System.Globalization;
using AdControl.ScreenClient.Services;
using Avalonia;
using Avalonia.Data.Converters;

namespace AdControl.ScreenClient
{
    public class MediaContentTemplateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ConfigItemDto item)
            {
                if (item.Type == "Image")
                {
                    return Application.Current.Resources["ImageTemplate"];
                }
                else if (item.Type == "Video")
                {
                    return Application.Current.Resources["VideoTemplate"];
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}