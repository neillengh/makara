namespace Makara.Core.Interfaces;

public interface ISystemSettingService
{
    Task<Dictionary<string, string>> GetAllAsync();
    Task SetAsync(Dictionary<string, string> keyValues);
    Task<string?> GetAsync(string key);
}
