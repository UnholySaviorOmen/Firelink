using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Firelink.Gui.Shared.ViewModels;

/// <summary>
/// VM экрана Settings. Пока — About-блок (имя, версия, копирайт,
/// лицензия) и переключатель DevMode.
///
/// DevMode — in-memory, default false. При включении в сайдбаре
/// появляются пункты Install/Pack/Verify/Logs; при выключении
/// остаются только Home и Settings. Persist — отдельный шаг (v0.2.0).
///
/// SettingsVM — singleton в DI (AddGuiShared). MainWindowVM резолвит
/// его напрямую и передаёт в NavigationVM.
/// </summary>
public sealed partial class SettingsVM : ViewModel
{
    /// <summary>
    /// DevMode. In-memory, default false. Меняется через UI
    /// (CheckBox в SettingsView). NavigationVM подписан на
    /// PropertyChanged и перестраивает Items.
    /// </summary>
    [ObservableProperty]
    private bool _isDevMode;

    /// <summary>Отображаемое имя приложения.</summary>
    public string ProductName => "Firelink";

    /// <summary>Версия (из Directory.Build.props через assembly).</summary>
    public string Version { get; } = GetVersion();

    /// <summary>Копирайт.</summary>
    public string Copyright => "Copyright (C) 2026 omen";

    /// <summary>SPDX-идентификатор лицензии.</summary>
    public string License => "AGPL-3.0-or-later";

    /// <summary>Строка "Firelink v0.1.0 · AGPL-3.0-or-later · Copyright (C) 2026 omen".</summary>
    public string Footer =>
        $"{ProductName} v{Version} · {License} · {Copyright}";

    private static string GetVersion()
    {
        var informational = typeof(SettingsVM).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;

        if (!string.IsNullOrWhiteSpace(informational))
        {
            var plus = informational.IndexOf('+');
            return plus >= 0 ? informational[..plus] : informational;
        }

        return "0.0.0";
    }
}
