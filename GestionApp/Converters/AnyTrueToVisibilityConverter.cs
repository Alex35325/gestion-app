using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace GestionApp.Converters;

/// <summary>Visible if ANY bound bool is true — used on dashboard widget cards so
/// a hidden-but-not-yet-saved widget still shows (dimmed, with its checkbox) while
/// the "Personnaliser" panel is open, matching IsWidgetVisible OR IsCustomizing.</summary>
public class AnyTrueToVisibilityConverter : IMultiValueConverter
{
    public object Convert(object?[] values, Type targetType, object? parameter, CultureInfo culture)
        => values.Any(v => v is true) ? Visibility.Visible : Visibility.Collapsed;

    public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
