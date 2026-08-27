using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using Makara.Core.Interfaces;
using Makara.Core.Models;

namespace Makara.Server.Services;

public class ProgressHub : IProgressHub
{
    private readonly ConcurrentDictionary<string, Channel<WorkflowEvent>> _channels = new();

    public void Publish(WorkflowEvent evt)
    {
        if (_channels.TryGetValue(evt.RunId, out var channel))
            channel.Writer.TryWrite(evt);
    }

    public async IAsyncEnumerable<WorkflowEvent> Subscribe(
        string runId, [EnumeratorCancellation] CancellationToken ct = default)
    {
        var channel = _channels.GetOrAdd(runId, _ => Channel.CreateUnbounded<WorkflowEvent>());
        await foreach (var evt in channel.Reader.ReadAllAsync(ct))
            yield return evt;
    }

    public void Complete(string runId)
    {
        if (_channels.TryGetValue(runId, out var channel))
        {
            channel.Writer.TryComplete();
            _channels.TryRemove(runId, out _);
        }
    }
}
