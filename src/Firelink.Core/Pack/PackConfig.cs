namespace Firelink.Core.Models.Pack;

/// <summary>
/// Корень firelink-pack.json — конфиг автора сборки.
/// </summary>
public sealed record PackConfig
{
    public required PackMeta Meta { get; init; }
    public required PackInstance Instance { get; init; }
    public required PackMo2 Mo2 { get; init; }
    public required PackStockGame StockGame { get; init; }

    /// <summary>
    /// Источники для архивов без .meta-файлов.
    /// Обязательное поле, но может быть пустым списком.
    /// </summary>
    public required IReadOnlyList<PackArchiveSource> ArchiveSources { get; init; }
}
