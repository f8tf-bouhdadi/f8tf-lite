using System.Net;
using System.Net.Sockets;
using System.Text;

static class P0
{
    // ---- Configuration ----
    private const string Host = "127.0.0.1";
    private const int Port = 5001;

    public static async Task<int> Main(string[] args)
    {
        if (args.Length == 0)
        {
            PrintUsage();
            return 2;
        }

        var mode = args[0].Trim().ToLowerInvariant();

        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

        try
        {
            return mode switch
            {
                "server" => await RunServerAsync(cts.Token),
                "client" => await RunClientAsync(args.Skip(1).ToArray(), cts.Token),
                _ => UsageAndReturn()
            };
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Annulé (Ctrl+C).");
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur: {ex.GetType().Name}: {ex.Message}");
            return 1;
        }
    }

    private static int UsageAndReturn()
    {
        PrintUsage();
        return 2;
    }

    private static void PrintUsage()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  dotnet run -- server");
        Console.WriteLine("  dotnet run -- client \"bonjour\"");
    }

    // =========================
    // ========== SERVER =======
    // =========================
    private static async Task<int> RunServerAsync(CancellationToken ct)
    {
        var listener = new TcpListener(IPAddress.Parse(Host), Port);

        // Start() : demande à l'OS d'ouvrir un socket TCP en écoute sur (Host, Port)
        listener.Start();
        Console.WriteLine($"[SERVER] Listening on {Host}:{Port} (Ctrl+C pour arrêter)");

        while (!ct.IsCancellationRequested)
        {
            // AcceptTcpClientAsync : attend une connexion entrante (bloquant en async)
            var client = await listener.AcceptTcpClientAsync(ct);

            // On traite chaque client en tâche séparée (pour accepter plusieurs clients)
            _ = Task.Run(() => HandleClientAsync(client, ct), ct);
        }

        listener.Stop();
        return 0;
    }

    private static async Task HandleClientAsync(TcpClient client, CancellationToken ct)
    {
        using var _ = client;
        var remote = client.Client.RemoteEndPoint?.ToString() ?? "?";
        Console.WriteLine($"[SERVER] Client connected: {remote}");

        // GetStream() : donne un NetworkStream (abstraction "stream" sur le socket)
        using NetworkStream stream = client.GetStream();

        // ReadFrameAsync : lit un message "framed" (len + payload)
        var payload = await ReadFrameAsync(stream, ct);

        Console.WriteLine($"[SERVER] RX {payload.Length} bytes: {ToHex(payload)}");
        Console.WriteLine($"[SERVER] RX as UTF-8: \"{Encoding.UTF8.GetString(payload)}\"");

        // Réponse (echo + timestamp)
        var responseText = $"echo: {Encoding.UTF8.GetString(payload)} | serverTime={DateTimeOffset.Now:O}";
        var responseBytes = Encoding.UTF8.GetBytes(responseText);

        await WriteFrameAsync(stream, responseBytes, ct);
        Console.WriteLine($"[SERVER] TX {responseBytes.Length} bytes: {ToHex(responseBytes)}");

        Console.WriteLine($"[SERVER] Client done: {remote}");
    }

    // =========================
    // ========== CLIENT =======
    // =========================
    private static async Task<int> RunClientAsync(string[] args, CancellationToken ct)
    {
        var message = args.Length >= 1 ? args[0] : "bonjour";
        byte[] payload = Encoding.UTF8.GetBytes(message);

        using var client = new TcpClient();

        Console.WriteLine($"[CLIENT] Connecting to {Host}:{Port} ...");

        // ConnectAsync : établit une connexion TCP (3-way handshake géré par l'OS)
        await client.ConnectAsync(Host, Port, ct);

        Console.WriteLine("[CLIENT] Connected.");

        using NetworkStream stream = client.GetStream();

        // Envoi
        await WriteFrameAsync(stream, payload, ct);
        Console.WriteLine($"[CLIENT] TX {payload.Length} bytes: {ToHex(payload)}");
        Console.WriteLine($"[CLIENT] TX as UTF-8: \"{message}\"");

        // Réception
        var resp = await ReadFrameAsync(stream, ct);
        Console.WriteLine($"[CLIENT] RX {resp.Length} bytes: {ToHex(resp)}");
        Console.WriteLine($"[CLIENT] RX as UTF-8: \"{Encoding.UTF8.GetString(resp)}\"");

        return 0;
    }

    // ======================================
    // ========== FRAMING (len+payload) ======
    // ======================================

    // ReadExactAsync : lit exactement N octets (TCP peut livrer en plusieurs morceaux)
    private static async Task ReadExactAsync(NetworkStream stream, byte[] buffer, int offset, int count, CancellationToken ct)
    {
        int readTotal = 0;

        while (readTotal < count)
        {
            // ReadAsync : lit jusqu'à count-readTotal octets (peut retourner moins)
            int n = await stream.ReadAsync(buffer.AsMemory(offset + readTotal, count - readTotal), ct);

            if (n == 0)
                throw new IOException("Connexion fermée par le pair.");

            readTotal += n;
        }
    }

    // WriteFrameAsync : écrit [4 bytes longueur big-endian] + payload
    private static async Task WriteFrameAsync(NetworkStream stream, byte[] payload, CancellationToken ct)
    {
        // Longueur sur 4 octets (big-endian)
        byte[] len = new byte[4];
        int n = payload.Length;
        len[0] = (byte)((n >> 24) & 0xFF);
        len[1] = (byte)((n >> 16) & 0xFF);
        len[2] = (byte)((n >> 8) & 0xFF);
        len[3] = (byte)(n & 0xFF);

        // WriteAsync : envoie les octets sur le socket
        await stream.WriteAsync(len, ct);
        await stream.WriteAsync(payload, ct);

        // FlushAsync : pour certains streams c'est utile ; NetworkStream flush souvent "no-op"
        await stream.FlushAsync(ct);
    }

    // ReadFrameAsync : lit 4 bytes longueur puis lit payload
    private static async Task<byte[]> ReadFrameAsync(NetworkStream stream, CancellationToken ct)
    {
        byte[] len = new byte[4];
        await ReadExactAsync(stream, len, 0, 4, ct);

        int n =
            (len[0] << 24) |
            (len[1] << 16) |
            (len[2] << 8) |
            len[3];

        if (n < 0 || n > 1024 * 1024)
            throw new InvalidDataException($"Taille de message invalide: {n}");

        byte[] payload = new byte[n];
        await ReadExactAsync(stream, payload, 0, n, ct);
        return payload;
    }

    private static string ToHex(byte[] bytes)
        => BitConverter.ToString(bytes).Replace("-", " ");
}
