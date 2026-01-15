using System.Text;
using System.Text.Json;
using System.Linq;

namespace DATA0_RawStore;

public sealed class RawStore
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _ioLock = new(1, 1);

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        WriteIndented = false
    };

    public RawStore(string filePath)
    {
        _filePath = filePath;
        Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
    }

    public async Task AppendAsync(RawRecord record, CancellationToken ct = default)
    {
        var line = JsonSerializer.Serialize(record, JsonOpts) + "\n";
        var bytes = Encoding.UTF8.GetBytes(line);

        await _ioLock.WaitAsync(ct);
        try
        {
            await using var fs = new FileStream(
                _filePath,
                FileMode.Append,
                FileAccess.Write,
                FileShare.Read,
                bufferSize: 4096,
                useAsync: true
            );

            await fs.WriteAsync(bytes, ct);
            await fs.FlushAsync(ct);
        }
        finally
        {
            _ioLock.Release();
        }
    }

    public async Task<IReadOnlyList<RawRecord>> TailAsync(int n, CancellationToken ct = default)
    {
        if (n <= 0) return Array.Empty<RawRecord>();
        if (!File.Exists(_filePath)) return Array.Empty<RawRecord>();

        var lines = await File.ReadAllLinesAsync(_filePath, ct);
        var slice = lines.Skip(Math.Max(0, lines.Length - n));

        var list = new List<RawRecord>();
        foreach (var line in slice)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            try
            {
                var rec = JsonSerializer.Deserialize<RawRecord>(line, JsonOpts);
                if (rec is not null) list.Add(rec);
            }
            catch
            {
                // ignore corrupted line (DATA0 tolérant)
            }
        }
        return list;
    }
}

public sealed record RawRecord(
    DateTimeOffset ts,
    string id,
    string topic,
    string payload,
    Dictionary<string, object>? meta
);
