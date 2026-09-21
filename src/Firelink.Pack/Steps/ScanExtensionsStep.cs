using System.Collections.Concurrent;
using Firelink.Core.Abstractions;
using Firelink.Core.Archives;
using Firelink.Core.Models.Pack;
using Microsoft.Extensions.Logging;

namespace Firelink.Pack.Steps;

/// <summary>
/// Сканирует extensions из config.Mo2.Extensions[].
///
/// Каждый элемент массива — относительный путь ОТ КОРНЯ MO2/.
/// Это либо файл, либо папка.
///
/// Для файла:      одна запись ScannedFile с RelativePath = entry path.
/// Для папки:      рекурсивный обход, RelativePath каждого файла =
///                 entry path + "/" + путь внутри папки.
///
/// Результат — EntryScanResult:
///   Entries[entryName] = список файлов.
///
/// Ошибки:
///   - Путь не существует → DirectoryNotFoundException / FileNotFoundException.
///     Автор написал в конфиге то, чего нет в инстансе. Молча скипать нельзя.
///
/// НЕ распаковывает архивы. НЕ матчит. Только сканирует файловую систему.
/// </summary>
public sealed class ScanExtensionsStep
    : IStep<ScanExtensionsStep.Input, EntryScanResult>
{
    private readonly FileHashCache _hashCache;
    private readonly ILogger<ScanExtensionsStep> _logger;

    public ScanExtensionsStep(
        FileHashCache hashCache,
        ILogger<ScanExtensionsStep> logger)
    {
        _hashCache = hashCache;
        _logger = logger;
    }

    public Task<EntryScanResult> ExecuteAsync(Input input, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var entries = input.Config.Mo2.Extensions;
        var rootPath = input.Snapshot.Mo2Path;

        _logger.LogInformation(
            "ScanExtensionsStep: scanning {Count} extension(s) from {Root}",
            entries.Count, rootPath);

        if (entries.Count == 0)
        {
            return Task.FromResult(new EntryScanResult
            {
                Entries = new Dictionary<string, IReadOnlyList<ScannedFile>>(
                    StringComparer.Ordinal),
            });
        }

        var results = new ConcurrentDictionary<string, IReadOnlyList<ScannedFile>>(
            StringComparer.Ordinal);

        Parallel.ForEach(
            entries,
            input.ParallelOptions,
            () => 0,
            (entry, _, _) =>
            {
                ct.ThrowIfCancellationRequested();

                var files = ScanEntry(rootPath, entry);

                results[entry] = files;

                _logger.LogDebug(
                    "Scanned extension '{Entry}': {Count} file(s)",
                    entry, files.Count);

                return 0;
            },
            _ => { });

        // Порядок — как в config (Parallel.ForEach не сохраняет порядок).
        var ordered = new Dictionary<string, IReadOnlyList<ScannedFile>>(
            StringComparer.Ordinal);

        foreach (var entry in entries)
        {
            if (results.TryGetValue(entry, out var files))
                ordered[entry] = files;
        }

        int totalFiles = ordered.Values.Sum(v => v.Count);

        _logger.LogInformation(
            "ScanExtensionsStep: {Entries} entries, {Files} files total",
            ordered.Count, totalFiles);

        return Task.FromResult(new EntryScanResult
        {
            Entries = ordered,
        });
    }

    // ------------------------------------------------------------------
    //  Сканирование одного entry
    // ------------------------------------------------------------------

    private IReadOnlyList<ScannedFile> ScanEntry(string rootPath, string entry)
    {
        var normalizedEntry = entry
            .Replace('\\', '/')
            .TrimEnd('/');

        var absolutePath = Path.Combine(
            rootPath,
            normalizedEntry.Replace('/', Path.DirectorySeparatorChar));

        if (File.Exists(absolutePath))
        {
            var file = ScanSingleFile(
                absolutePath,
                relativePathFromRoot: normalizedEntry);

            return new[] { file };
        }

        if (Directory.Exists(absolutePath))
        {
            return ScanDirectory(
                absolutePath,
                entryPrefix: normalizedEntry);
        }

        // Ни файла, ни папки — конфиг не соответствует инстансу.
        throw new FileNotFoundException(
            $"MO2 extension not found in instance: " +
            $"'{entry}' (resolved to '{absolutePath}'). " +
            $"Check config.mo2.extensions[] and the instance contents.",
            absolutePath);
    }

    private IReadOnlyList<ScannedFile> ScanDirectory(
        string absoluteDir,
        string entryPrefix)
    {
        var result = new List<ScannedFile>();

        var files = Directory.EnumerateFiles(
            absoluteDir,
            "*",
            new EnumerationOptions
            {
                RecurseSubdirectories = true,
                IgnoreInaccessible = false,
                ReturnSpecialDirectories = false,
            });

        foreach (var file in files)
        {
            var relativeInside = Path
                .GetRelativePath(absoluteDir, file)
                .Replace('\\', '/');

            var relativeFromRoot = entryPrefix + "/" + relativeInside;

            result.Add(ScanSingleFile(file, relativeFromRoot));
        }

        result.Sort((a, b) =>
            string.CompareOrdinal(a.RelativePath, b.RelativePath));

        return result;
    }

    private ScannedFile ScanSingleFile(string absolutePath, string relativePathFromRoot)
    {
        var hash = _hashCache.GetOrCompute(absolutePath);
        var size = new FileInfo(absolutePath).Length;

        return new ScannedFile
        {
            RelativePath = relativePathFromRoot,
            Hash = hash,
            Size = size,
        };
    }

    // ------------------------------------------------------------------
    //  Input
    // ------------------------------------------------------------------

    public sealed class Input
    {
        public required PackConfig Config { get; init; }
        public required InstanceSnapshot Snapshot { get; init; }
        public required ParallelOptions ParallelOptions { get; init; }
    }
}
