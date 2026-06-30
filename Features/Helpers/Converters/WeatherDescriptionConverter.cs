using System.Globalization;

namespace TravelSecure.Mobile.Helpers.Converters;

public class WeatherDescriptionConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo? culture)
    {
        if (value is string desc && !string.IsNullOrEmpty(desc))
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(desc);
        }
        return "Cargando...";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
    {
        throw new NotImplementedException();
    }
}

public class InverseBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo? culture)
    {
        return value is bool b ? !b : true;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
    {
        return value is bool b ? !b : true;
    }
}