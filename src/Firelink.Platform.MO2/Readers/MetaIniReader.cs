using System.Globalization;
using Firelink.Core.Models.Pack;

namespace Firelink.Platform.MO2.Readers;

/// <summary>
/// Чтение mods/&lt;Name&gt;/meta.ini — файла, который MO2 создаёт для каждого
/// установленного мода.
///
/// Читается только секция [General]. Секция [installedFiles] игнорируется:
/// она содержит абсолютные пути автора и бесполезна при воспроизведении.
///
/// Ключи case-sensitive (MO2 пишет modID, fileID, gameID — именно так).
/// Имя секции — case-insensitive ([General], [general], [GENERAL]).
///
/// Если файла нет — TryRead возвращает null.
/// Если файл есть, но секции [General] нет — возвращается ModMeta.Empty.
/// </summary>
public static class MetaIniReader
{
    private const string SectionGeneral = "General";

    public static ModMeta? TryRead(string path)
    {
        if (!File.Exists(path))
            return null;

        var lines = File.ReadAllLines(path);
        return Parse(lines);
    }

    public static ModMeta Parse(IEnumerable<string> lines)
    {
        string? gameName = null;
        string? gameId = null;
        int? modId = null;
        int? fileId = null;
        string? version = null;
        int? category = null;
        string? repository = null;
        string? url = null;
        string? comments = null;
        string? notes = null;

        var inGeneral = false;

        foreach (var raw in lines)
        {
            var line = Normalize(raw);
            if (line.Length == 0) continue;
            if (line[0] == ';' || line[0] == '#') continue;

            if (line[0] == '[')
            {
                var close = line.IndexOf(']');
                if (close < 0) continue;
                var section = line[1..close].Trim();
                inGeneral = section.Equals(SectionGeneral, StringComparison.OrdinalIgnoreCase);
                continue;
            }

            if (!inGeneral) continue;

            var eq = line.IndexOf('=');
            if (eq < 0) continue;

            var key = line[..eq].Trim();
            var value = line[(eq + 1)..].Trim();

            switch (key)
            {
                case "gameName" when gameName is null:
                    gameName = value;
                    break;
                case "gameID" when gameId is null:
                    gameId = value;
                    break;
                case "modID" when modId is null:
                    if (TryParseInt(value, out var mi)) modId = mi;
                    break;
                case "fileID" when fileId is null:
                    if (TryParseInt(value, out var fi)) fileId = fi;
                    break;
                case "version" when version is null:
                    version = value;
                    break;
                case "category" when category is null:
                    if (TryParseInt(value, out var cat)) category = cat;
                    break;
                case "repository" when repository is null:
                    repository = value;
                    break;
                case "url" when url is null:
                    url = value;
                    break;
                case "comments" when comments is null:
                    comments = value;
                    break;
                case "notes" when notes is null:
                    notes = value;
                    break;
            }
        }

        return new ModMeta
        {
            GameName = gameName,
            GameId = gameId,
            ModId = modId,
            FileId = fileId,
            Version = version,
            Category = category,
            Repository = repository,
            Url = url,
            Comments = comments,
            Notes = notes,
        };
    }

    private static bool TryParseInt(string value, out int result)
        => int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out result);

    private static string Normalize(string raw)
    {
        var line = raw.TrimEnd('\r', '\n');
        if (line.Length > 0 && line[0] == '\uFEFF')
            line = line[1..];
        return line.Trim();
    }
}
