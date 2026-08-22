using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GestionApp.Converters;

/// <summary>Hides an alert banner when its count is zero.</summary>
public class CountToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is int i && i > 0 ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
