using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WindowExtras.Wpf.Converters;

internal class ShadowMarginConverter : IMultiValueConverter
{
    /// <inheritdoc />
    public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length == 3 &&
            values[0] is double radius &&
            values[1] is double offsetX &&
            values[2] is double offsetY)
        {
            return new Thickness(
                radius - offsetX,
                radius - offsetY,
                radius + offsetX,
                radius + offsetY);
        }

        return null;
    }

    /// <inheritdoc />
    public object[]? ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        return null;
    }
}