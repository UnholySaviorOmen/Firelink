using System.Collections.Concurrent;
using Firelink.Core.Abstractions;
using Firelink.Core.Archives;
using Firelink.Core.Models.Pack;
using Microsoft.Extensions.Logging;

namespace Firelink.Pack.Steps;

/// <summary>
/// Сканирует папку mods/ — считает хеши всех файлов включённых модов.
/// Пропускает отключённые моды и моды с [NoDelete] в имени.
/// Параллельно, с использованием FileHashCache.
/// </summary>
public sealed class ScanModsStep : IStep<ScanModsStep.Input, ModScanResult>
{
    private readonly FileHashCache _hashCache;
    private readonly ILogger<ScanModsStep> _logger;

    public ScanModsStep(FileHashCache hashCache, ILogger<ScanModsStep> logger)
    {
        _hashCache = hashCache;
        _logger = logger;
    }

    public Task<ModScanResult> ExecuteAsync(Input input, CancellationToken ct)
    {
        var entries = input.Snapshot.Modlist.Entries;

        // Фильтр: включённые, без [NoDelete]
        var toScan = entries
            .Where(e => e.Enabled)
            .Where(e => !e.Name.Contains("[NoDelete]", StringComparison.OrdinalIgnoreCase))
            .ToList();

        _logger.LogInformation(
            "ScanModsStep: scanning {Count} enabled mods (out of {Total})",
            toScan.Count, entries.Count);

        // Проверка, что папки существуют — заранее, до параллельного скана
        foreach (var entry in toScan)
        {
            var path = Path.Combine(input.Snapshot.ModsPath, entry.Name);
            if (!Directory.Exists(path))
            {
                throw new DirectoryNotFoundException(
                    $"Mod directory not found: {path} " +
                    $"(mod '{entry.Name}' is enabled in modlist.txt)");
            }
        }

        var results = new ConcurrentDictionary<string, IReadOnlyList<ScannedFile>>(
            StringComparer.Ordinal);

        Parallel.ForEach(
            toScan,
            input.ParallelOptions,
            () => 0,
            (entry, _, _) =>
            {
                ct.ThrowIfCancellationRequested();

                var modPath = Path.Combine(input.Snapshot.ModsPath, entry.Name);
                var files = ScanOneMod(modPath, ct);

                results[entry.Name] = files;

                _logger.LogDebug(
                    "Scanned mod '{Mod}': {Count} files",
                    entry.Name, files.Count);

                return 0;
            },
            _ => { });

        // Стабильный порядок ключей для детерминизма
        var ordered = results
            .OrderBy(kvp => kvp.Key, StringComparer.Ordinal)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value, StringComparer.Ordinal);

        var totalFiles = ordered.Values.Sum(v => v.Count);
        _logger.LogInformation(
            "ScanModsStep: {Mods} mods, {Files} files total",
            ordered.Count, totalFiles);

        return Task.FromResult(new ModScanResult
        {
            Mods = ordered,
        });
    }

    private IReadOnlyList<ScannedFile> ScanOneMod(string modPath, CancellationToken ct)
    {
        var result = new List<ScannedFile>();

        var files = Directory.EnumerateFiles(
            modPath,
            "*",
            new EnumerationOptions
            {
                RecurseSubdirectories = true,
                IgnoreInaccessible = false,
                ReturnSpecialDirectories = false,
            });

        foreach (var file in files)
        {
            ct.ThrowIfCancellationRequested();

            var relative = Path.GetRelativePath(modPath, file).Replace('\\', '/');
            var hash = _hashCache.GetOrCompute(file);
            var size = new FileInfo(file).Length;

            result.Add(new ScannedFile
            {
                RelativePath = relative,
                Hash = hash,
                Size = size,
            });
        }

        // Детерминированный порядок файлов
        result.Sort((a, b) => string.CompareOrdinal(a.RelativePath, b.RelativePath));

        return result;
    }

    public sealed class Input
    {
        public required InstanceSnapshot Snapshot { get; init; }
        public required ParallelOptions ParallelOptions { get; init; }
    }
}
