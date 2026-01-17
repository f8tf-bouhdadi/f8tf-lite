using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
{
    WriteIndented = false
};

builder.Services.AddSingleton(new RawStore());

var app = builder.Build();

app.MapPost("/append", async (HttpContext ctx, RawStore store) =>
{
    var req = await ctx.Request.ReadFromJsonAsync<AppendRequest>(jsonOptions);

    if (req is null || string.IsNullOrWhiteSpace(req.Payload))
        return Results.BadRequest(new { error = "InvalidRequest", message = "payload required" });

    var offset = await store.AppendAsync(req.Payload, ctx.RequestAborted);

    return Results.Ok(new { offset });
});

app.MapGet("/health", () => Results.Ok(new { ok = true }));

app.Run();

public sealed record AppendRequest(string Payload);

public sealed class RawStore
{
    private readonly SemaphoreSlim _ioLock = new(1, 1);
    private readonly string _path =
        Path.Combine(Path.GetTempPath(), $"data0-rawstore-{Environment.ProcessId}.log");

    public async Task<long> AppendAsync(string payload, CancellationToken ct)
    {
        await _ioLock.WaitAsync(ct);
        try
        {
            long offset = 0;
            if (File.Exists(_path))
                offset = File.ReadLines(_path).LongCount();

            var lineObj = new { offset, ts = DateTimeOffset.UtcNow.ToString("O"), payload };
            var line = JsonSerializer.Serialize(lineObj);
            await File.AppendAllTextAsync(_path, line + Environment.NewLine, ct);

            return offset;
        }
        finally
        {
            _ioLock.Release();
        }
    }
}
