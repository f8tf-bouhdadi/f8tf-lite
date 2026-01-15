using System.Net.Http.Json;
using DATA0_RawStore;

var argsList = args.ToList();

if (argsList.Count == 0)
{
    Console.WriteLine("Usage:");
    Console.WriteLine("  dotnet run -- server [--url http://localhost:5080]");
    Console.WriteLine("  dotnet run -- client <baseUrl> <topic> <payload>");
    return;
}

var mode = argsList[0].ToLowerInvariant();

if (mode == "server")
{
    var url = "http://localhost:5080";
    var urlIdx = argsList.IndexOf("--url");
    if (urlIdx >= 0 && urlIdx + 1 < argsList.Count) url = argsList[urlIdx + 1];

    var builder = WebApplication.CreateBuilder();
    builder.WebHost.UseUrls(url);

    var app = builder.Build();

    // ⚠️ Attention: ceci est un dossier runtime (dans bin/...), PAS ton dossier racine "data/"
    var runtimeDataDir = Path.Combine(AppContext.BaseDirectory, "runtime-data");
    var storeFile = Path.Combine(runtimeDataDir, "data0-rawstore.jsonl");
    var store = new RawStore(storeFile);

    app.MapGet("/health", () => Results.Ok("ok"));

    app.MapPost("/append", async (HttpContext ctx, CancellationToken ct) =>
    {
        var req = await ctx.Request.ReadFromJsonAsync<AppendRequest>(cancellationToken: ct);
        if (req is null) return Results.BadRequest(new { error = "Invalid JSON" });

        if (string.IsNullOrWhiteSpace(req.Topic))
            return Results.BadRequest(new { error = "topic required" });

        var payload = req.Payload ?? "";
        if (payload.Length > 10_000)
            return Results.BadRequest(new { error = "payload too large" });

        var rec = new RawRecord(
            ts: DateTimeOffset.UtcNow,
            id: Guid.NewGuid().ToString("N"),
            topic: req.Topic,
            payload: payload,
            meta: req.Meta
        );

        await store.AppendAsync(rec, ct);
        return Results.Ok(new { ok = true, rec.id, rec.ts });
    });

    app.MapGet("/tail", async (int n, CancellationToken ct) =>
    {
        n = Math.Clamp(n, 1, 200);
        var items = await store.TailAsync(n, ct);
        return Results.Ok(items);
    });

    app.Run();
    return;
}

if (mode == "client")
{
    if (argsList.Count < 4)
    {
        Console.WriteLine("Usage: dotnet run -- client <baseUrl> <topic> <payload>");
        return;
    }

    var baseUrl = argsList[1].TrimEnd('/');
    var topic = argsList[2];
    var payload = argsList[3];

    using var http = new HttpClient();

    var appendResp = await http.PostAsJsonAsync($"{baseUrl}/append",
        new AppendRequest(topic, payload, new Dictionary<string, object> { ["source"] = "client" }));

    var tailResp = await http.GetAsync($"{baseUrl}/tail?n=20");
    var tailText = await tailResp.Content.ReadAsStringAsync();

    Console.WriteLine($"APPEND status={(int)appendResp.StatusCode}");
    Console.WriteLine($"TAIL   status={(int)tailResp.StatusCode}");
    Console.WriteLine(tailText);

    if (!tailText.Contains(payload))
        Environment.Exit(2);

    Console.WriteLine("SMOKE: ok");
    return;
}

Console.WriteLine($"Unknown mode: {mode}");

public sealed record AppendRequest(string Topic, string Payload, Dictionary<string, object>? Meta);
