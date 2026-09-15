namespace Firelink.Platform.MO2.Models;

/// <summary>
/// Строка plugins.txt: имя плагина + флаг "включён".
/// </summary>
public sealed record PluginEntry(string Name, bool Enabled);
