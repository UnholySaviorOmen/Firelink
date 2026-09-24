using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Firelink.Gui.Shared.Navigation;

namespace Firelink.Gui.Converters;

/// <summary>
/// Маппит ScreenType на SVG-путь иконки (Lucide, MIT).
/// Используется в NavigationView: PathIcon.Data={Binding Screen, Converter=...}.
///
/// Иконки — 24x24 viewBox, stroke-based, но PathIcon в Avalonia
/// работает с fill. Поэтому пути адаптированы: fill-версии,
/// сохранён силуэт (без stroke).
///
/// Если Lucide-иконка в оригинале не подходит в fill-варианте
/// (например, home — это контур с крышей), используются
/// Lucide-варианты, специально предназначенные для fill
/// (в Lucide есть некоторые иконки с "solid" версией).
///
/// Fallback для неизвестного ScreenType — пустая Geometry.
/// </summary>
public sealed class ScreenIconConverter : IValueConverter
{
    // Lucide "home" (адаптирован под fill)
    private const string Home =
        "M12 3 L2 12 H5 V20 H10 V14 H14 V20 H19 V12 H22 Z";

    // Lucide "package" / "download" — используем "package"
    private const string Install =
        "M12 2 L21 7 V17 L12 22 L3 17 V7 Z M12 4.2 L5.2 8 L12 11.8 L18.8 8 Z " +
        "M5 10 V16 L11 19.2 V13.2 Z M13 13.2 V19.2 L19 16 V10 Z";

    // Lucide "archive"
    private const string Pack =
        "M3 4 H21 V8 H3 Z M4 8 H20 V20 H4 Z M9 12 H15 V14 H9 Z";

    // Lucide "shield-check" (щит с галочкой)
    private const string Verify =
        "M12 2 L20 5 V11 C20 16 16 20 12 22 C8 20 4 16 4 11 V5 Z " +
        "M10.5 12.5 L9 11 L7.6 12.4 L10.5 15.3 L16.4 9.4 L15 8 Z";

    // Lucide "file-text" (документ со строками) — для Logs
    private const string Logs =
        "M6 2 H14 L20 8 V22 H6 Z " +
        "M14 2 V8 H20 " +
        "M9 12 H17 V13.5 H9 Z " +
        "M9 15.5 H17 V17 H9 Z " +
        "M9 19 H15 V20.5 H9 Z";

    // Lucide "settings" (шестерёнка)
    private const string Settings =
        "M12 8 C9.8 8 8 9.8 8 12 C8 14.2 9.8 16 12 16 C14.2 16 16 14.2 16 12 C16 9.8 14.2 8 12 8 Z " +
        "M10.3 2 L10.7 4.3 C10.3 4.4 9.9 4.6 9.5 4.8 L7.7 3.3 L5.3 5.7 L6.8 7.5 " +
        "C6.6 7.9 6.4 8.3 6.3 8.7 L4 9.1 L4 11.9 L6.3 12.3 " +
        "C6.4 12.7 6.6 13.1 6.8 13.5 L5.3 15.3 L7.7 17.7 L9.5 16.2 " +
        "C9.9 16.4 10.3 16.6 10.7 16.7 L11.1 19 L13.9 19 L14.3 16.7 " +
        "C14.7 16.6 15.1 16.4 15.5 16.2 L17.3 17.7 L19.7 15.3 L18.2 13.5 " +
        "C18.4 13.1 18.6 12.7 18.7 12.3 L21 11.9 L21 9.1 L18.7 8.7 " +
        "C18.6 8.3 18.4 7.9 18.2 7.5 L19.7 5.7 L17.3 3.3 L15.5 4.8 " +
        "C15.1 4.6 14.7 4.4 14.3 4.3 L13.9 2 Z";

    public object? Convert(
        object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not ScreenType screen)
            return null;

        var path = screen switch
        {
            ScreenType.Home => Home,
            ScreenType.Install => Install,
            ScreenType.Pack => Pack,
            ScreenType.Verify => Verify,
            ScreenType.Logs => Logs,
            ScreenType.Settings => Settings,
            _ => null,
        };

        if (path is null)
            return null;

        return Geometry.Parse(path);
    }

    public object ConvertBack(
        object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
