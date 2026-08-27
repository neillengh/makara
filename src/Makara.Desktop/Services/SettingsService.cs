using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;

namespace Makara.Desktop.Services;

/// <summary>
/// 本地应用设置（设置页数据模型，持久化到 %APPDATA%\Makara\settings.json）
/// </summary>
public class AppSettings
{
    public string Theme { get; set; } = "dark";      // dark | light
    public string UiScale { get; set; } = "medium"; // small | medium | large

    // 默认参数
    public int DefaultEpochs { get; set; } = 10;
    public double DefaultLearningRate { get; set; } = 0.001;
    public double DefaultValSplit { get; set; } = 0.2;
    public int DefaultBatchSize { get; set; } = 32;

    // 数据安全
    public bool FieldEncryption { get; set; } = true;
    public bool DataMasking { get; set; } = true;
    public string SensitiveRegex { get; set; } = "(?i)(password|secret|token|key|ssn|phone|email|id_card)";

    // 通知
    public bool SsePush { get; set; } = true;
    public bool TaskReminder { get; set; } = true;
    public string NotifyEmail { get; set; } = string.Empty;
}

/// <summary>
/// 本地设置读写服务
/// </summary>
public class SettingsService
{
    private static readonly string Dir =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Makara");
    private static readonly string FilePath = Path.Combine(Dir, "settings.json");

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public AppSettings Settings { get; private set; } = new();

    public SettingsService()
    {
        Load();
    }

    public void Load()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                var json = File.ReadAllText(FilePath);
                var loaded = JsonSerializer.Deserialize<AppSettings>(json);
                if (loaded is not null) Settings = loaded;
            }
        }
        catch
        {
            // 配置损坏时回退默认值
            Settings = new AppSettings();
        }
    }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(Dir);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(Settings, JsonOpts));
        }
        catch
        {
            // 持久化失败不中断应用
        }
    }

    public void Reset() => Settings = new AppSettings();
}
