namespace Firelink.Core.Models.Pack;

/// <summary>
/// Информация об инстансе MO2 в firelink-pack.json.
/// </summary>
public sealed record PackInstance
{
    /// <summary>
    /// Путь к корню инстанса (папке, содержащей MO2/ и Stock Game/)
    /// относительно firelink-pack.json.
    /// Пример: "NordicUI Overhaul".
    /// </summary>
    public required string Path { get; init; }
}
