using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Makara.Desktop.Services;

public class SseClient
{
    private readonly HttpClient _http;

    public string BaseUrl { get; set; } = "http://localhost:5000";

    public event Action<string>? OnMessage;

    public SseClient()
    {
        _http = new HttpClient { BaseAddress = new Uri(BaseUrl) };
        _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));
    }

    public async Task SubscribeAsync(string runId, CancellationToken cancellationToken = default)
    {
        using var stream = await _http.GetStreamAsync($"api/tasks/{runId}/stream", cancellationToken);
        using var reader = new StreamReader(stream);

        while (!cancellationToken.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(cancellationToken);
            if (line == null) break;
            if (line.StartsWith("data: "))
            {
                var data = line["data: ".Length..];
                OnMessage?.Invoke(data);
            }
        }
    }
}
