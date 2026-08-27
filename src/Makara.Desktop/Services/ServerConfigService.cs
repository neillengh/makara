using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;

namespace Makara.Desktop.Services;

/// <summary>
/// 多服务端配置服务：管理服务端列表、当前选中、持久化到 %APPDATA%\Makara\servers.json
/// </summary>
public class ServerConfigService
{
    private static readonly string Dir =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Makara");
    private static readonly string FilePath = Path.Combine(Dir, "servers.json");

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public ObservableCollection<Models.ServerProfile> Servers { get; } = [];

    public event Action<Models.ServerProfile>? CurrentServerChanged;

    public Models.ServerProfile Current { get; private set; } = new();

    public ServerConfigService()
    {
        Load();
        if (Servers.Count == 0)
        {
            Servers.Add(new Models.ServerProfile
            {
                Name = "本地开发",
                Address = "http://localhost:5000",
                Description = "本地开发调试使用的 Makara 服务",
                IsDefault = true
            });
            Save();
        }
        Current = Servers.FirstOrDefault(s => s.IsDefault) ?? Servers[0];
    }

    /// <summary>切换当前服务端</summary>
    public void SetCurrent(Models.ServerProfile server)
    {
        Current = server;
        CurrentServerChanged?.Invoke(server);
    }

    /// <summary>设为默认服务端</summary>
    public void SetDefault(Models.ServerProfile server)
    {
        foreach (var s in Servers) s.IsDefault = ReferenceEquals(s, server);
        Save();
    }

    public void Add(Models.ServerProfile server)
    {
        if (Servers.Count == 0) server.IsDefault = true;
        Servers.Add(server);
        Save();
    }

    public void Remove(Models.ServerProfile server)
    {
        var wasCurrent = ReferenceEquals(server, Current);
        Servers.Remove(server);
        if (Servers.Count > 0 && (wasCurrent || server.IsDefault))
        {
            var next = Servers[0];
            next.IsDefault = true;
            if (wasCurrent) SetCurrent(next);
        }
        Save();
    }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(Dir);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(Servers.ToList(), JsonOpts));
        }
        catch
        {
            // 持久化失败不中断应用
        }
    }

    private void Load()
    {
        try
        {
            if (!File.Exists(FilePath)) return;
            var json = File.ReadAllText(FilePath);
            var list = JsonSerializer.Deserialize<List<Models.ServerProfile>>(json);
            if (list is null) return;
            foreach (var s in list) Servers.Add(s);
        }
        catch
        {
            // 配置损坏时忽略，稍后重建默认配置
        }
    }
}
