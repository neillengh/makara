using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Makara.Desktop.ViewModels;

public class WorkflowTemplate
{
    public string Title { get; set; } = "";
    public string Category { get; set; } = "";
    public string Description { get; set; } = "";
    public int NodeCount { get; set; }
}

/// <summary>
/// 工作流模板页：模板卡片网格 + 搜索 + 分类筛选 + 一键套用
/// </summary>
public partial class WorkflowTemplatesViewModel : ObservableObject
{
    public ObservableCollection<WorkflowTemplate> All { get; }
    public ObservableCollection<WorkflowTemplate> Filtered { get; } = new();

    public List<string> Categories { get; } = new() { "全部", "数据同步", "模型训练", "自动化部署", "多租户" };

    [ObservableProperty] private string _searchText = "";
    [ObservableProperty] private string _activeFilter = "全部";
    [ObservableProperty] private string _statusMessage = "";

    public WorkflowTemplatesViewModel()
    {
        All = new ObservableCollection<WorkflowTemplate>
        {
            new() { Title = "SQL → 数据集 → 微调", Category = "数据同步", Description = "从 SQL 数据库抽取数据，自动转换为训练数据集并触发模型微调，适合快速构建领域模型。", NodeCount = 5 },
            new() { Title = "API 自动训练流水线", Category = "自动化部署", Description = "监听外部 API 回调，自动完成数据拉取、训练、评测与模型发布，实现端到端自动化。", NodeCount = 7 },
            new() { Title = "CSV 一键入模", Category = "数据同步", Description = "上传 CSV 文件后自动解析字段、生成数据集并完成模型训练，降低业务人员使用门槛。", NodeCount = 4 },
            new() { Title = "定时增量微调", Category = "模型训练", Description = "按 Cron 表达式定时执行增量数据同步与模型微调，保持模型持续进化与业务知识更新。", NodeCount = 6 },
            new() { Title = "多数据源融合训练", Category = "数据同步", Description = "汇聚数据库、对象存储与 API 等多源数据，统一清洗、对齐后进入模型训练环节。", NodeCount = 8 },
            new() { Title = "模型评测与部署", Category = "自动化部署", Description = "训练完成后自动运行 Benchmark 评测，通过阈值后触发模型打包与推理服务部署。", NodeCount = 5 },
            new() { Title = "数据安全脱敏流程", Category = "数据同步", Description = "在数据入模前自动识别敏感字段并进行脱敏或加密处理，满足合规与隐私保护要求。", NodeCount = 5 },
            new() { Title = "国产化 Ascend 训练", Category = "模型训练", Description = "针对华为昇腾 NPU 环境优化的训练流水线，支持 CANN 与 MindSpore 生态一键适配。", NodeCount = 6 },
        };
        Refresh();
    }

    partial void OnSearchTextChanged(string value) => Refresh();
    partial void OnActiveFilterChanged(string value) => Refresh();

    private void Refresh()
    {
        var q = SearchText.Trim();
        var f = ActiveFilter;
        var result = All.Where(t =>
            (f == "全部" || t.Category == f) &&
            (q == "" || t.Title.Contains(q, System.StringComparison.OrdinalIgnoreCase) ||
             t.Description.Contains(q, System.StringComparison.OrdinalIgnoreCase)));
        Filtered.Clear();
        foreach (var t in result) Filtered.Add(t);
    }

    [RelayCommand]
    private void SetFilter(string? filter)
    {
        if (!string.IsNullOrWhiteSpace(filter)) ActiveFilter = filter!;
    }

    [RelayCommand]
    private void UseTemplate(WorkflowTemplate? template)
    {
        if (template is null) return;
        StatusMessage = $"已套用模板「{template.Title}」（演示）";
    }
}
