namespace Makara.Desktop.Models;

public record NodeTemplate(string Type, string Icon, string DisplayName, string Category)
{
    public static readonly NodeTemplate[] All =
    [
        new("trigger", "⚡", "触发器", "触发器"),
        new("datasource", "🔗", "数据源", "数据"),
        new("dataclean", "🧹", "数据清洗", "数据"),
        new("fieldmap", "📋", "字段映射", "数据"),
        new("qualitycheck", "✓", "质量检查", "数据"),
        new("datasetbuild", "📦", "数据集构建", "数据"),
        new("finetune", "🔥", "模型微调", "模型"),
        new("evaluate", "📊", "模型评估", "模型"),
        new("deploy", "🚀", "模型部署", "模型"),
        new("notify", "🔔", "通知", "其他"),
        new("condition", "🔀", "条件分支", "其他")
    ];
}
