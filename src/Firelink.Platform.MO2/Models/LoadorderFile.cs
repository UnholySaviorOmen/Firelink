namespace Firelink.Platform.MO2.Models;

public sealed class LoadorderFile
{
    /// <summary>Порядок загрузки: сверху — раньше.</summary>
    public required IReadOnlyList<string> Plugins { get; init; }
}
