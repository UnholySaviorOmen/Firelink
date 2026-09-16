namespace Firelink.Core.Models.Pack;

/// <summary>
/// Содержимое секции [General] из mods/&lt;Name&gt;/meta.ini.
///
/// meta.ini создаётся MO2 и содержит метаданные мода: источник (Nexus),
/// версию, категорию, заметки автора. Секция [installedFiles] не читается
/// и не хранится — она бесполезна при воспроизведении и может ввести MO2
/// в заблуждение (содержит абсолютные пути автора).
///
/// Поле newestVersion не читается: оно зависит от времени и при
/// воспроизведении не имеет смысла.
/// </summary>
public sealed record ModMeta
{
    /// <summary>gameName из meta.ini (например, "Skyrim Special Edition"). Справочно.</summary>
    public string? GameName { get; init; }

    /// <summary>
    /// gameID из meta.ini. На практике — Nexus game domain
    /// (например, "skyrimspecialedition"). Совпадает с meta.game пакета.
    /// </summary>
    public string? GameId { get; init; }

    /// <summary>modID на Nexus. Null, если мод не с Nexus.</summary>
    public int? ModId { get; init; }

    /// <summary>fileID на Nexus.</summary>
    public int? FileId { get; init; }

    /// <summary>Версия мода (строка, как её хранит MO2).</summary>
    public string? Version { get; init; }

    /// <summary>Категория мода в MO2.</summary>
    public int? Category { get; init; }

    /// <summary>Репозиторий-источник (обычно "Nexus").</summary>
    public string? Repository { get; init; }

    /// <summary>URL страницы мода.</summary>
    public string? Url { get; init; }

    /// <summary>Комментарий MO2.</summary>
    public string? Comments { get; init; }

    /// <summary>Заметки автора сборки.</summary>
    public string? Notes { get; init; }

    /// <summary>Пустая ModMeta (нет meta.ini или нет секции [General]).</summary>
    public static ModMeta Empty { get; } = new();

    /// <summary>Истина, если все поля пустые.</summary>
    public bool IsEmpty =>
        GameName is null && GameId is null &&
        ModId is null && FileId is null &&
        Version is null && Category is null &&
        Repository is null && Url is null &&
        Comments is null && Notes is null;
}
