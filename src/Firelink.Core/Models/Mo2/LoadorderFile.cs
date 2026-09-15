namespace Firelink.Core.Models.Mo2;

public sealed class LoadorderFile
{
    /// <summary>Порядок загрузки: сверху — раньше.</summary>
    public required IReadOnlyList<string> Plugins { get; init; }
}
