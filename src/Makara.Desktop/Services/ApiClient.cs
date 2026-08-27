using System.Net.Http;
using System.Net.Http.Json;

namespace Makara.Desktop.Services;

public class ApiClient
{
    private readonly HttpClient _http;
    public string BaseUrl { get; set; } = "http://localhost:5000";

    public ApiClient()
    {
        _http = new HttpClient { BaseAddress = new Uri(BaseUrl) };
    }

    public async Task<T?> GetAsync<T>(string path) =>
        await _http.GetFromJsonAsync<T>(path);

    public async Task<T?> PostAsync<T>(string path, object body) =>
        await (await _http.PostAsJsonAsync(path, body)).Content.ReadFromJsonAsync<T>();

    public async Task PutAsync(string path, object body) =>
        await _http.PutAsJsonAsync(path, body);

    public async Task DeleteAsync(string path) =>
        await _http.DeleteAsync(path);
}
