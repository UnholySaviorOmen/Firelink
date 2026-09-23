using Microsoft.Extensions.Logging;

namespace Firelink.Gui.Shared.Logging;

public sealed record LogEntry(
    DateTimeOffset Timestamp,
    LogLevel Level,
    string Message);
