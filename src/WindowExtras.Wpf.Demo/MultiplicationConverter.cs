using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using static System.Convert;

namespace WindowExtras.Wpf.Demo;

public class MultiplicationConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var number = ToDouble(value, culture);
        var factor = ToDouble(parameter, culture);
        return number * factor;
    }

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return DependencyProperty.UnsetValue;
    }
}