using System.IO;
using System.IO.Pipes;

namespace PodcastDesktop;

internal sealed class SingleInstance : IDisposable
{
    private static readonly string Id = $"PodcastDesktop.{Environment.UserName}";

    private readonly Mutex _mutex;
    private readonly CancellationTokenSource _cts = new();

    public bool IsPrimary { get; }

    public event Action<string>? CommandReceived;

    public SingleInstance()
    {
        _mutex = new Mutex(true, Id, out var createdNew);
        IsPrimary = createdNew;
    }

    public void StartListening()
    {
        Task.Run(async () =>
        {
            while (!_cts.IsCancellationRequested)
            {
                try
                {
                    using var server = new NamedPipeServerStream(Id, PipeDirection.In);
                    await server.WaitForConnectionAsync(_cts.Token);
                    using var reader = new StreamReader(server);
                    var command = await reader.ReadToEndAsync(_cts.Token);
                    CommandReceived?.Invoke(command.Trim());
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (IOException)
                {
                }
            }
        });
    }

    public static void SendToPrimary(string command)
    {
        using var client = new NamedPipeClientStream(".", Id, PipeDirection.Out);
        client.Connect(3000);
        using var writer = new StreamWriter(client);
        writer.Write(command);
    }

    public void Dispose()
    {
        _cts.Cancel();
        if (IsPrimary)
        {
            _mutex.ReleaseMutex();
        }
        _mutex.Dispose();
    }
}
