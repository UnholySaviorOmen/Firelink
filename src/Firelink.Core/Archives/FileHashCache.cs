using System.Collections.Concurrent;
using Firelink.Core.Models.Hashing;

namespace Firelink.Core.Archives;

/// <summary>
/// In-memory кеш хешей файлов.
/// Ключ — (полный путь, размер, время последней записи).
/// Если файл не менялся — хеш берётся из кеша.
/// </summary>
public sealed class FileHashCache
{
    private readonly ConcurrentDictionary<FileKey, XxHash64Value> _cache = new();

    public XxHash64Value GetOrCompute(string path)
    {
        var info = new FileInfo(path);
        if (!info.Exists)
            throw new FileNotFoundException($"File not found: {path}", path);

        var key = new FileKey(path, info.Length, info.LastWriteTimeUtc);

        return _cache.GetOrAdd(key, _ => XxHash64Value.FromFile(path));
    }

    public int Count => _cache.Count;

    public void Clear() => _cache.Clear();

    private readonly record struct FileKey(string Path, long Length, DateTime LastWriteUtc);
}
