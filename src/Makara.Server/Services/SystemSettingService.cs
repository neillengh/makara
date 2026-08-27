using Makara.Core.Interfaces;
using Makara.Core.Models;

namespace Makara.Server.Services;

public class SystemSettingService : ISystemSettingService
{
    private readonly IRepository<SystemSetting> _repo;

    public SystemSettingService(IRepository<SystemSetting> repo)
    {
        _repo = repo;
    }

    public async Task<Dictionary<string, string>> GetAllAsync()
    {
        var list = await _repo.GetAllAsync(null, 0, 10000, nameof(SystemSetting.Key), false);
        return list.ToDictionary(k => k.Key, k => k.Value, StringComparer.OrdinalIgnoreCase);
    }

    public async Task<string?> GetAsync(string key)
    {
        var list = await _repo.GetAllAsync(s => s.Key == key, 0, 1);
        return list.FirstOrDefault()?.Value;
    }

    public async Task SetAsync(Dictionary<string, string> keyValues)
    {
        var existing = await GetAllAsync();
        var now = DateTime.UtcNow;

        foreach (var kv in keyValues)
        {
            if (existing.ContainsKey(kv.Key))
            {
                var match = (await _repo.GetAllAsync(s => s.Key == kv.Key, 0, 1)).First();
                match.Value = kv.Value;
                match.UpdatedAt = now;
                await _repo.UpdateAsync(match);
            }
            else
            {
                await _repo.InsertAsync(new SystemSetting
                {
                    Key = kv.Key,
                    Value = kv.Value,
                    UpdatedAt = now
                });
            }
        }
    }
}
