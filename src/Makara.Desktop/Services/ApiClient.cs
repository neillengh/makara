using System.Net.Http;
using System.Net.Http.Json;
using Makara.Core.Interfaces;
using Makara.Core.Models;
using Makara.Desktop.Models;

namespace Makara.Desktop.Services;

public class ApiClient
{
    private readonly HttpClient _http = new();

    public string BaseUrl { get; private set; } = "http://localhost:5000";

    public ApiClient()
    {
        _http.BaseAddress = new Uri(BaseUrl);
        _http.Timeout = TimeSpan.FromSeconds(30);
    }

    /// <summary>切换服务端地址（重建 HttpClient）</summary>
    public void SetBaseUrl(string baseUrl)
    {
        if (string.IsNullOrWhiteSpace(baseUrl)) return;
        BaseUrl = baseUrl.TrimEnd('/');
        _http.BaseAddress = new Uri(BaseUrl);
    }

    /// <summary>服务端健康检查</summary>
    public async Task<bool> CheckHealthAsync()
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var resp = await _http.GetAsync("api/system/health", cts.Token);
            return resp.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    // === DataSources ===
    public Task<List<DataSource>?> GetDataSourcesAsync() =>
        _http.GetFromJsonAsync<List<DataSource>>("api/datasources");

    public Task<DataSource?> GetDataSourceAsync(string id) =>
        _http.GetFromJsonAsync<DataSource>($"api/datasources/{id}");

    public async Task<DataSource?> CreateDataSourceAsync(DataSource ds) =>
        await (await _http.PostAsJsonAsync("api/datasources", ds)).Content.ReadFromJsonAsync<DataSource>();

    public async Task UpdateDataSourceAsync(string id, DataSource ds) =>
        await _http.PutAsJsonAsync($"api/datasources/{id}", ds);

    public async Task DeleteDataSourceAsync(string id) =>
        await _http.DeleteAsync($"api/datasources/{id}");

    public async Task<bool> TestConnectionAsync(DataSource ds)
    {
        var resp = await _http.PostAsJsonAsync("api/datasources/test", ds);
        var result = await resp.Content.ReadFromJsonAsync<Dictionary<string, bool>>();
        return result?.GetValueOrDefault("connected") ?? false;
    }

    // === Workflows ===
    public Task<List<Workflow>?> GetWorkflowsAsync() =>
        _http.GetFromJsonAsync<List<Workflow>>("api/workflows");

    public Task<Workflow?> GetWorkflowAsync(string id) =>
        _http.GetFromJsonAsync<Workflow>($"api/workflows/{id}");

    public async Task<Workflow?> CreateWorkflowAsync(Workflow wf) =>
        await (await _http.PostAsJsonAsync("api/workflows", wf)).Content.ReadFromJsonAsync<Workflow>();

    public async Task UpdateWorkflowAsync(string id, Workflow wf) =>
        await _http.PutAsJsonAsync($"api/workflows/{id}", wf);

    public async Task DeleteWorkflowAsync(string id) =>
        await _http.DeleteAsync($"api/workflows/{id}");

    public async Task<string> RunWorkflowAsync(string id)
    {
        var resp = await _http.PostAsync($"api/workflows/{id}/run", null);
        var result = await resp.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        return result?.GetValueOrDefault("runId") ?? "";
    }

    public Task<WorkflowRun?> GetRunStatusAsync(string runId) =>
        _http.GetFromJsonAsync<WorkflowRun>($"api/workflows/runs/{runId}/status");

    // === Datasets ===
    public Task<List<DatasetInfo>?> GetDatasetsAsync() =>
        _http.GetFromJsonAsync<List<DatasetInfo>>("api/datasets");

    public Task<DatasetInfo?> GetDatasetAsync(string id) =>
        _http.GetFromJsonAsync<DatasetInfo>($"api/datasets/{id}");

    public Task<List<DatasetSample>?> GetDatasetSamplesAsync(string id, int skip = 0, int take = 10) =>
        _http.GetFromJsonAsync<List<DatasetSample>>($"api/datasets/{id}/samples?skip={skip}&take={take}");

    public async Task<bool> DeleteDatasetAsync(string id)
    {
        var resp = await _http.DeleteAsync($"api/datasets/{id}");
        return resp.IsSuccessStatusCode;
    }

    // === ETL ===
    public async Task<EtlResult?> ExecuteEtlAsync(EtlRequest req) =>
        await (await _http.PostAsJsonAsync("api/etl/execute", req)).Content.ReadFromJsonAsync<EtlResult>();

    public async Task<EtlPreview?> PreviewEtlAsync(EtlRequest req, int limit = 10) =>
        await (await _http.PostAsJsonAsync($"api/etl/preview?limit={limit}", req)).Content.ReadFromJsonAsync<EtlPreview>();

    // === Scheduler ===
    public Task<SchedulerStatus?> GetSchedulerStatusAsync() =>
        _http.GetFromJsonAsync<SchedulerStatus>("api/scheduler/status");

    public async Task EnableSchedulerAsync() =>
        await _http.PostAsync("api/scheduler/enable", null);

    public async Task DisableSchedulerAsync() =>
        await _http.PostAsync("api/scheduler/disable", null);
}
