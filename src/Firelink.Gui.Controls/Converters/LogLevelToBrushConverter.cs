using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Microsoft.Extensions.Logging;

using AvaloniaColor = Avalonia.Media.Color;

namespace Firelink.Gui.Controls.Converters;

public sealed class LogLevelToBrushConverter : IValueConverter
{
    public static readonly LogLevelToBrushConverter Instance = new();

    private static readonly IBrush TraceBrush = new SolidColorBrush(AvaloniaColor.Parse("#4B5563"));
    private static readonly IBrush DebugBrush = new SolidColorBrush(AvaloniaColor.Parse("#6B7280"));
    private static readonly IBrush InfoBrush = new SolidColorBrush(AvaloniaColor.Parse("#D1D5DB"));
    private static readonly IBrush WarningBrush = new SolidColorBrush(AvaloniaColor.Parse("#FBBF24"));
    private static readonly IBrush ErrorBrush = new SolidColorBrush(AvaloniaColor.Parse("#F87171"));
    private static readonly IBrush CriticalBrush = new SolidColorBrush(AvaloniaColor.Parse("#DC2626"));

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not LogLevel level) return InfoBrush;

        return level switch
        {
            LogLevel.Trace => TraceBrush,
            LogLevel.Debug => DebugBrush,
            LogLevel.Information => InfoBrush,
            LogLevel.Warning => WarningBrush,
            LogLevel.Error => ErrorBrush,
            LogLevel.Critical => CriticalBrush,
            _ => InfoBrush,
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
