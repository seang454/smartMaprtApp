using System;
using System.Globalization;
using System.Windows.Data;

namespace SmallMartApp.UI.Helpers;

/// <summary>
/// Converts numeric values to string and safely converts empty string "" back to 0 without throwing FormatException.
/// </summary>
public class EmptyToZeroConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null) return string.Empty;
        return value.ToString() ?? string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        string? str = value?.ToString()?.Trim();
        if (string.IsNullOrWhiteSpace(str))
        {
            if (targetType == typeof(int) || targetType == typeof(int?))
                return 0;
            if (targetType == typeof(decimal) || targetType == typeof(decimal?))
                return 0m;
            if (targetType == typeof(double) || targetType == typeof(double?))
                return 0.0;
            return 0;
        }

        if (targetType == typeof(int) || targetType == typeof(int?))
        {
            return int.TryParse(str, NumberStyles.Any, culture, out int intVal) ? intVal : 0;
        }

        if (targetType == typeof(decimal) || targetType == typeof(decimal?))
        {
            return decimal.TryParse(str, NumberStyles.Any, culture, out decimal decVal) ? decVal : 0m;
        }

        if (targetType == typeof(double) || targetType == typeof(double?))
        {
            return double.TryParse(str, NumberStyles.Any, culture, out double dblVal) ? dblVal : 0.0;
        }

        return value ?? 0;
    }
}
