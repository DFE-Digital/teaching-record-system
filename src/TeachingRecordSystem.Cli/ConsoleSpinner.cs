using System.Diagnostics;

namespace TeachingRecordSystem.Cli;

/// <summary>
/// Renders an animated spinner with a message and elapsed time on stderr until disposed.
/// </summary>
/// <remarks>
/// When stderr is redirected the message is written once without any animation.
/// </remarks>
public sealed class ConsoleSpinner : IAsyncDisposable
{
    private static readonly string[] _frames = ["⠋", "⠙", "⠹", "⠸", "⠼", "⠴", "⠦", "⠧", "⠇", "⠏"];
    private static readonly TimeSpan _frameInterval = TimeSpan.FromMilliseconds(80);

    private readonly string _message;
    private readonly CancellationTokenSource _cts = new();
    private readonly Task _renderTask;

    private ConsoleSpinner(string message)
    {
        _message = message;
        _renderTask = RenderAsync(_cts.Token);
    }

    public static ConsoleSpinner Start(string message) => new(message);

    public async ValueTask DisposeAsync()
    {
        await _cts.CancelAsync();
        await _renderTask;
        _cts.Dispose();
    }

    private async Task RenderAsync(CancellationToken cancellationToken)
    {
        if (Console.IsErrorRedirected)
        {
            Console.Error.WriteLine(_message);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        using var timer = new PeriodicTimer(_frameInterval);
        var frame = 0;
        var lastLineLength = 0;

        try
        {
            do
            {
                var line = $"{_frames[frame++ % _frames.Length]} {_message} ({(int)stopwatch.Elapsed.TotalSeconds}s)";
                Console.Error.Write($"\r{line.PadRight(lastLineLength)}");
                lastLineLength = line.Length;
            }
            while (await timer.WaitForNextTickAsync(cancellationToken));
        }
        catch (OperationCanceledException)
        {
        }

        Console.Error.Write($"\r{new string(' ', lastLineLength)}\r");
    }
}
