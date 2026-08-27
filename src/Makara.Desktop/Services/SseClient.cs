using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using Makara.Core.Models;

namespace Makara.Desktop.Services;

public class SseClient
{
    private readonly HttpClient _http;

    public string BaseUrl { get; set; } = "http://localhost:5000";

    public event Action<WorkflowEvent>? OnEvent;

    private CancellationTokenSource? _cts;

    public SseClient()
    {
        _http = new HttpClient { BaseAddress = new Uri(BaseUrl) };
        _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));
    }

    public async Task SubscribeAsync(string runId, CancellationToken cancellationToken = default)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        using var stream = await _http.GetStreamAsync($"api/tasks/{runId}/stream", _cts.Token);
        using var reader = new StreamReader(stream);

        while (!_cts.Token.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(_cts.Token);
            if (line == null) break;
            if (line.StartsWith("data: "))
            {
                var json = line["data: ".Length..];
                try
                {
                    var evt = JsonSerializer.Deserialize<WorkflowEvent>(json);
                    if (evt != null)
                        OnEvent?.Invoke(evt);
                }
                catch { }
            }
        }
    }

    public void Unsubscribe()
    {
        _cts?.Cancel();
    }
}
