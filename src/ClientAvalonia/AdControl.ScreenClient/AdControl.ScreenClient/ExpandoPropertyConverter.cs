using System.Dynamic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace AdControl.ScreenClient;

public class ExpandoPropertyConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ExpandoObject expando && parameter is string propertyName)
        {
            var dict = (IDictionary<string, object>)expando;
            if (dict.TryGetValue(propertyName, out var result))
                return result?.ToString();
        }

        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}