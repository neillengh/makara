using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Makara.Core.Enums;
using Makara.Core.Models;

namespace Makara.Server.Services;

/// <summary>
/// 服务端启动时，若对应表为空则写入可演示的模拟数据。
/// 设计为幂等：重新启动不会重复插入。
/// </summary>
public static class SeedDataService
{
    private static int _seeded;

    public static async Task EnsureSeedAsync(IFreeSql fsql)
    {
        if (Interlocked.CompareExchange(ref _seeded, 1, 0) != 0)
            return;

        try
        {
            await EnsureUsers(fsql);
            await EnsureDataSources(fsql);
            await EnsureWorkflows(fsql);
            await EnsureWorkflowRuns(fsql);
            await EnsureDatasetInfos(fsql);
            await EnsureDatasetSamples(fsql);
            await EnsureFieldMappingConfigs(fsql);
            await EnsureSystemSettings(fsql);
            await EnsureAuditLogs(fsql);
        }
        catch (Exception ex)
        {
            // Seed 失败不影响服务启动（避免首次运行因表同步时机报错），记录后抛出由上层处理
            Console.WriteLine($"[SeedData] Failed: {ex.Message}");
            throw;
        }
    }

    private static string HashPassword(string pwd)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(pwd + "makara-salt-v1"));
        return Convert.ToBase64String(bytes);
    }

    private static async Task EnsureUsers(IFreeSql fsql)
    {
        if (await fsql.Select<User>().AnyAsync()) return;

        await fsql.Insert<User>().AppendData(new[]
        {
            new User
            {
                Id = "user_admin",
                Username = "admin",
                PasswordHash = HashPassword("admin123"),
                DisplayName = "系统管理员",
                Email = "admin@makara.local",
                Role = UserRole.Admin,
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                UpdatedAt = DateTime.UtcNow.AddDays(-2),
                LastLoginAt = DateTime.UtcNow.AddHours(-1)
            },
            new User
            {
                Id = "user_operator",
                Username = "operator",
                PasswordHash = HashPassword("123456"),
                DisplayName = "数据集运营",
                Email = "operator@makara.local",
                Role = UserRole.User,
                CreatedAt = DateTime.UtcNow.AddDays(-20),
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                LastLoginAt = DateTime.UtcNow.AddDays(-1)
            }
        }).ExecuteAffrowsAsync();
    }

    private static async Task EnsureDataSources(IFreeSql fsql)
    {
        if (await fsql.Select<DataSource>().AnyAsync()) return;

        await fsql.Insert<DataSource>().AppendData(new[]
        {
            new DataSource
            {
                Id = "ds_001",
                Name = "生产订单库 MySQL",
                Type = DataSourceType.MySql,
                ConnectionString = "Server=localhost;Port=3306;Database=orders;Uid=root;Pwd=mock;AllowPublicKeyRetrieval=True",
                Query = "SELECT * FROM orders WHERE created_at >= '2024-01-01'",
                IncrementalField = "updated_at",
                CreatedAt = DateTime.UtcNow.AddDays(-40),
                UpdatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new DataSource
            {
                Id = "ds_002",
                Name = "客户画像 SQL Server",
                Type = DataSourceType.SqlServer,
                ConnectionString = "Data Source=localhost,1433;Initial Catalog=crm;User ID=sa;Password=mock;TrustServerCertificate=True",
                Query = "SELECT user_id, tags, purchase_history FROM customer_profile",
                CreatedAt = DateTime.UtcNow.AddDays(-25),
                UpdatedAt = DateTime.UtcNow.AddDays(-3)
            },
            new DataSource
            {
                Id = "ds_003",
                Name = "对话原始 CSV",
                Type = DataSourceType.Csv,
                ConnectionString = "./data/dialogues.csv",
                CreatedAt = DateTime.UtcNow.AddDays(-15),
                UpdatedAt = DateTime.UtcNow.AddDays(-2)
            }
        }).ExecuteAffrowsAsync();
    }

    private static async Task EnsureWorkflows(IFreeSql fsql)
    {
        if (await fsql.Select<Workflow>().AnyAsync()) return;

        await fsql.Insert<Workflow>().AppendData([
            new Workflow
            {
                Id = "wf_001",
                Name = "订单微调数据集管线",
                Description = "从生产订单库拉取 -> 清洗 -> 字段映射 -> 构建 Jsonl 训练集",
                CronExpression = "0 3 * * *",
                Status = WorkflowStatus.Ready,
                Nodes =
                [
                    new WorkflowNode { Id = "n1", Type = nameof(NodeType.DataSource), Label = "数据源：订单库", X = 80, Y = 120, Config = new Dictionary<string, object> { ["dataSourceId"] = "ds_001" } },
                    new WorkflowNode { Id = "n2", Type = nameof(NodeType.DataClean), Label = "清洗-去重补全", X = 340, Y = 120, Config = new Dictionary<string, object> { ["dedupe"] = true, ["fillna"] = true } },
                    new WorkflowNode { Id = "n3", Type = nameof(NodeType.FieldMap), Label = "字段映射", X = 600, Y = 120, Config = new Dictionary<string, object> { ["mappingId"] = "fm_001" } },
                    new WorkflowNode { Id = "n4", Type = nameof(NodeType.DatasetBuild), Label = "构建 Jsonl 数据集", X = 860, Y = 120, Config = new Dictionary<string, object> { ["format"] = "Jsonl", ["outputDir"] = "./output/orders-v2.3" } }
                ],
                Edges =
                [
                    new WorkflowEdge { Id = "e1", SourceNodeId = "n1", TargetNodeId = "n2" },
                    new WorkflowEdge { Id = "e2", SourceNodeId = "n2", TargetNodeId = "n3" },
                    new WorkflowEdge { Id = "e3", SourceNodeId = "n3", TargetNodeId = "n4" }
                ],
                CreatedAt = DateTime.UtcNow.AddDays(-35),
                UpdatedAt = DateTime.UtcNow.AddDays(-4)
            },
            new Workflow
            {
                Id = "wf_002",
                Name = "客户画像构建流程",
                Description = "从 SQL Server CRM 构建客户画像训练集",
                CronExpression = "30 1 * * 1",
                Status = WorkflowStatus.Running,
                Nodes =
                [
                    new WorkflowNode { Id = "n1", Type = nameof(NodeType.DataSource), Label = "数据源：CRM", X = 80, Y = 150, Config = new Dictionary<string, object> { ["dataSourceId"] = "ds_002" } },
                    new WorkflowNode { Id = "n2", Type = nameof(NodeType.DataClean), Label = "清洗-打标签", X = 340, Y = 150 },
                    new WorkflowNode { Id = "n3", Type = nameof(NodeType.QualityCheck), Label = "质检", X = 600, Y = 150 },
                    new WorkflowNode { Id = "n4", Type = nameof(NodeType.DatasetBuild), Label = "构建 Parquet 训练集", X = 860, Y = 150 }
                ],
                Edges =
                [
                    new WorkflowEdge { Id = "e1", SourceNodeId = "n1", TargetNodeId = "n2" },
                    new WorkflowEdge { Id = "e2", SourceNodeId = "n2", TargetNodeId = "n3" },
                    new WorkflowEdge { Id = "e3", SourceNodeId = "n3", TargetNodeId = "n4" }
                ],
                CreatedAt = DateTime.UtcNow.AddDays(-18),
                UpdatedAt = DateTime.UtcNow.AddHours(-1)
            },
            new Workflow
            {
                Id = "wf_003",
                Name = "原始对话清洗 Demo",
                Description = "CSV 对话数据 -> 清洗 -> 字段映射 -> Jsonl",
                Status = WorkflowStatus.Draft,
                Nodes =
                [
                    new WorkflowNode { Id = "n1", Type = nameof(NodeType.DataSource), Label = "数据源：对话CSV", X = 80, Y = 180, Config = new Dictionary<string, object> { ["dataSourceId"] = "ds_003" } },
                    new WorkflowNode { Id = "n2", Type = nameof(NodeType.DataClean), Label = "清洗-短对话过滤", X = 340, Y = 180 },
                    new WorkflowNode { Id = "n3", Type = nameof(NodeType.FieldMap), Label = "字段映射", X = 600, Y = 180, Config = new Dictionary<string, object> { ["mappingId"] = "fm_003" } },
                    new WorkflowNode { Id = "n4", Type = nameof(NodeType.DatasetBuild), Label = "输出 Jsonl", X = 860, Y = 180 }
                ],
                Edges =
                [
                    new WorkflowEdge { Id = "e1", SourceNodeId = "n1", TargetNodeId = "n2" },
                    new WorkflowEdge { Id = "e2", SourceNodeId = "n2", TargetNodeId = "n3" },
                    new WorkflowEdge { Id = "e3", SourceNodeId = "n3", TargetNodeId = "n4" }
                ],
                CreatedAt = DateTime.UtcNow.AddDays(-8),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            }
        ]).ExecuteAffrowsAsync();
    }

    private static async Task EnsureWorkflowRuns(IFreeSql fsql)
    {
        if (await fsql.Select<WorkflowRun>().AnyAsync()) return;

        var now = DateTime.UtcNow;
        var runs = new List<WorkflowRun>
        {
            // 最近 4 条
            new()
            {
                Id = "run_new_success1", WorkflowId = "wf_001", Status = "succeeded", Progress = 100,
                CurrentNode = "构建 Jsonl 数据集",
                Result = JsonSerializer.Serialize(new { datasetId = "ds_info_001", totalSamples = 12450 }),
                StartedAt = now.AddHours(-6), FinishedAt = now.AddHours(-5).AddMinutes(42),
                Logs =
                [
                    new RunLog { RunId = "run_new_success1", Level = "info", Message = "工作流 订单微调数据集管线 开始执行", NodeId = "n1" },
                    new RunLog { RunId = "run_new_success1", Level = "info", Message = "节点 数据源：订单库 执行完成", NodeId = "n1" },
                    new RunLog { RunId = "run_new_success1", Level = "info", Message = "节点 清洗-去重补全 执行完成", NodeId = "n2" },
                    new RunLog { RunId = "run_new_success1", Level = "info", Message = "节点 字段映射 执行完成", NodeId = "n3" },
                    new RunLog { RunId = "run_new_success1", Level = "info", Message = "工作流执行完成：共 12450 条样本", NodeId = "n4" }
                ]
            },
            new()
            {
                Id = "run_new_success2", WorkflowId = "wf_001", Status = "succeeded", Progress = 100,
                StartedAt = now.AddDays(-1), FinishedAt = now.AddDays(-1).AddMinutes(38),
                Result = JsonSerializer.Serialize(new { datasetId = "ds_info_001_vprev", totalSamples = 12100 })
            },
            new()
            {
                Id = "run_new_failed1", WorkflowId = "wf_003", Status = "failed", Progress = 62,
                Error = "CSV 文件格式错误：第 32 行列数不一致",
                StartedAt = now.AddHours(-12), FinishedAt = now.AddHours(-12).AddMinutes(22)
            },
            new()
            {
                Id = "run_new_running", WorkflowId = "wf_002", Status = "running", Progress = 58,
                CurrentNode = "质检",
                StartedAt = now.AddHours(-1), FinishedAt = null
            },
            // 历史 4 条，跨度近 14 天
            new()
            {
                Id = "run_h1", WorkflowId = "wf_001", Status = "succeeded", Progress = 100,
                StartedAt = now.AddDays(-4), FinishedAt = now.AddDays(-4).AddMinutes(44),
                Result = JsonSerializer.Serialize(new { totalSamples = 11980 })
            },
            new()
            {
                Id = "run_h2", WorkflowId = "wf_002", Status = "succeeded", Progress = 100,
                StartedAt = now.AddDays(-7), FinishedAt = now.AddDays(-7).AddHours(1).AddMinutes(12),
                Result = JsonSerializer.Serialize(new { totalSamples = 254300 })
            },
            new()
            {
                Id = "run_h3", WorkflowId = "wf_001", Status = "succeeded", Progress = 100,
                StartedAt = now.AddDays(-10), FinishedAt = now.AddDays(-10).AddMinutes(39),
                Result = JsonSerializer.Serialize(new { totalSamples = 11320 })
            },
            new()
            {
                Id = "run_h4", WorkflowId = "wf_003", Status = "succeeded", Progress = 100,
                StartedAt = now.AddDays(-13), FinishedAt = now.AddDays(-13).AddMinutes(24),
                Result = JsonSerializer.Serialize(new { totalSamples = 7860 })
            }
        };

        // FreeSql 序列化 List<RunLog> 时可能需要特殊处理，这里直接整体插入
        await fsql.Insert<WorkflowRun>().AppendData(runs).ExecuteAffrowsAsync();
    }

    private static async Task EnsureDatasetInfos(IFreeSql fsql)
    {
        if (await fsql.Select<DatasetInfo>().AnyAsync()) return;

        await fsql.Insert<DatasetInfo>().AppendData([
            new DatasetInfo
            {
                Id = "ds_info_001",
                Name = "订单微调 v2.3",
                StorageFormat = DatasetStorageFormat.Jsonl,
                ContentFormat = DatasetFormat.Instruction,
                SourceWorkflowId = "wf_001",
                SourceDataSourceId = "ds_001",
                SampleCount = 12450,
                SchemaJson = "[\"instruction\",\"input\",\"output\"]",
                StoragePath = "./output/orders-v2.3/train.jsonl",
                QualityScore = 92,
                Status = DatasetStatus.Ready,
                CreatedBy = "user_admin",
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                UpdatedAt = DateTime.UtcNow.AddHours(-5)
            },
            new DatasetInfo
            {
                Id = "ds_info_002",
                Name = "客服对话微调集",
                StorageFormat = DatasetStorageFormat.Jsonl,
                ContentFormat = DatasetFormat.MultiTurn,
                SourceWorkflowId = "wf_003",
                SourceDataSourceId = "ds_003",
                SampleCount = 8200,
                SchemaJson = "[\"conversation_id\",\"turns\"]",
                StoragePath = "./output/dialogues-v1/train.jsonl",
                QualityScore = 87,
                Status = DatasetStatus.Ready,
                CreatedBy = "user_operator",
                CreatedAt = DateTime.UtcNow.AddDays(-8),
                UpdatedAt = DateTime.UtcNow.AddDays(-6)
            },
            new DatasetInfo
            {
                Id = "ds_info_003",
                Name = "客户画像训练集",
                StorageFormat = DatasetStorageFormat.Parquet,
                ContentFormat = DatasetFormat.PlainText,
                SourceWorkflowId = "wf_002",
                SourceDataSourceId = "ds_002",
                SampleCount = 258000,
                SchemaJson = "[\"user_id\",\"feature_vector\",\"text\"]",
                StoragePath = "./output/crm-v2/train.parquet",
                QualityScore = 74,
                Status = DatasetStatus.Building,
                CreatedBy = "user_admin",
                CreatedAt = DateTime.UtcNow.AddHours(-1),
                UpdatedAt = DateTime.UtcNow.AddHours(-1)
            },
            new DatasetInfo
            {
                Id = "ds_info_004",
                Name = "历史订单-废弃",
                StorageFormat = DatasetStorageFormat.Csv,
                ContentFormat = DatasetFormat.PlainText,
                SourceWorkflowId = null,
                SourceDataSourceId = "ds_001",
                SampleCount = 3400,
                SchemaJson = "[\"order_id\",\"content\"]",
                StoragePath = "./output/old/orders.csv",
                QualityScore = 41,
                Status = DatasetStatus.Failed,
                CreatedBy = "user_operator",
                CreatedAt = DateTime.UtcNow.AddDays(-32),
                UpdatedAt = DateTime.UtcNow.AddDays(-24)
            }
        ]).ExecuteAffrowsAsync();
    }

    private static async Task EnsureDatasetSamples(IFreeSql fsql)
    {
        if (await fsql.Select<DatasetSample>().AnyAsync()) return;

        var samples = new List<DatasetSample>();

        // ds_info_001：30 条指令微调样本
        var orders = new (string orderId, string query, string answer)[]
        {
            ("ORD20241001", "我上周买的商品到哪了？", "您好，订单 ORD20241001 已到达【上海浦东营业部】，预计今天下午送达。"),
            ("ORD20241002", "能帮我改收货地址吗？", "已为您将收货地址修改为北京市朝阳区望京SOHO T3-1205。"),
            ("ORD20241003", "这个商品可以退款吗？", "订单 ORD20241003 已支付未发货，已为您发起全额退款，预计 1-3 个工作日原路返回。"),
            ("ORD20241004", "物流太慢了，我要投诉！", "非常抱歉给您带来不好的体验。订单 ORD20241004 物流轨迹出现异常，已为您联系快递方加急处理，订单号 EX67234。"),
            ("ORD20241005", "有没有发票？", "您好，订单 ORD20241005 的电子普通发票已开具，收件邮箱为 admin@example.com。")
        };
        for (int i = 0; i < 30; i++)
        {
            var o = orders[i % orders.Length];
            samples.Add(new DatasetSample
            {
                Id = "samp_001_" + i,
                DatasetId = "ds_info_001",
                RecordIndex = i,
                JsonData = JsonSerializer.Serialize(new
                {
                    instruction = $"客户咨询：{o.query}",
                    input = $"订单号 {o.orderId}",
                    output = o.answer
                })
            });
        }

        // ds_info_002：25 条多轮对话样本
        string[] multiTurnSeeds =
        [
            @"[{""role"":""user"",""content"":""我的宽带断网了""},{""role"":""assistant"",""content"":""好的，请您先确认光信号灯是否正常闪烁？""},{""role"":""user"",""content"":""红灯一直在闪""},{""role"":""assistant"",""content"":""收到，已为您派发维修师傅，今日下午 3 点前到府，请保持电话畅通""}]",
            @"[{""role"":""user"",""content"":""我想办理停机保号""},{""role"":""assistant"",""content"":""好的，请告知需要保留多久？""},{""role"":""user"",""content"":""3 个月""},{""role"":""assistant"",""content"":""已为您办理，每月 5 元，3 个月后自动复机。""}]"
        ];
        for (int i = 0; i < 25; i++)
        {
            samples.Add(new DatasetSample
            {
                Id = "samp_002_" + i,
                DatasetId = "ds_info_002",
                RecordIndex = i,
                JsonData = JsonSerializer.Serialize(new
                {
                    conversation_id = "conv_" + i.ToString("D6"),
                    turns = multiTurnSeeds[i % multiTurnSeeds.Length]
                })
            });
        }

        // ds_info_003：20 条画像样本
        for (int i = 0; i < 20; i++)
        {
            samples.Add(new DatasetSample
            {
                Id = "samp_003_" + i,
                DatasetId = "ds_info_003",
                RecordIndex = i,
                JsonData = JsonSerializer.Serialize(new
                {
                    user_id = "U" + (10000 + i),
                    age = 20 + (i * 3) % 50,
                    city = new[] { "上海", "北京", "深圳", "杭州", "成都", "广州" }[i % 6],
                    tags = string.Join(",", new[] { "高频购买", "母婴用户", "数码控", "敏感价格", "健身爱好者" }.Take(i % 5)),
                    lifetime_value = 500 + i * 120
                })
            });
        }

        // ds_info_004：15 条废弃样本
        for (int i = 0; i < 15; i++)
        {
            samples.Add(new DatasetSample
            {
                Id = "samp_004_" + i,
                DatasetId = "ds_info_004",
                RecordIndex = i,
                JsonData = JsonSerializer.Serialize(new
                {
                    order_id = "ORDOLD" + i,
                    content = $"历史订单 {i} 原始内容（样本质量不达标）"
                })
            });
        }

        await fsql.Insert<DatasetSample>().AppendData(samples).ExecuteAffrowsAsync();
    }

    private static async Task EnsureFieldMappingConfigs(IFreeSql fsql)
    {
        if (await fsql.Select<FieldMappingConfig>().AnyAsync()) return;

        await fsql.Insert<FieldMappingConfig>().AppendData([
            new FieldMappingConfig
            {
                Id = "fm_001",
                WorkflowId = "wf_001",
                SourceDataSourceId = "ds_001",
                TargetSchemaJson = "[\"instruction\",\"input\",\"output\"]",
                MappingsJson = "[{\"source\":\"customer_query\",\"target\":\"instruction\"},{\"source\":\"order_no\",\"target\":\"input\"},{\"source\":\"agent_reply\",\"target\":\"output\"}]",
                CreatedAt = DateTime.UtcNow.AddDays(-20),
                UpdatedAt = DateTime.UtcNow.AddDays(-4)
            },
            new FieldMappingConfig
            {
                Id = "fm_003",
                WorkflowId = "wf_003",
                SourceDataSourceId = "ds_003",
                TargetSchemaJson = "[\"conversation_id\",\"turns\"]",
                MappingsJson = "[{\"source\":\"session_id\",\"target\":\"conversation_id\"},{\"source\":\"dialogue\",\"target\":\"turns\"}]",
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                UpdatedAt = DateTime.UtcNow.AddDays(-2)
            }
        ]).ExecuteAffrowsAsync();
    }

    private static async Task EnsureSystemSettings(IFreeSql fsql)
    {
        if (await fsql.Select<SystemSetting>().AnyAsync()) return;

        var now = DateTime.UtcNow;
        var entries = new Dictionary<string, string>
        {
            ["security.encryption.enabled"] = "true",
            ["security.encryption.algorithm"] = "AES256",
            ["security.masking.enabled"] = "true",
            ["security.masking.sensitiveFields"] = "[\"phone\",\"email\",\"id_card\"]",
            ["training.default.epochs"] = "3",
            ["training.default.learningRate"] = "2e-4",
            ["training.default.batchSize"] = "8",
            ["training.default.validationRatio"] = "0.1",
            ["training.default.optimizer"] = "AdamW",
            ["notification.email.smtpHost"] = "smtp.makara.local"
        };
        await fsql.Insert<SystemSetting>().AppendData(
            entries.Select(kv => new SystemSetting { Key = kv.Key, Value = kv.Value, UpdatedAt = now })
        ).ExecuteAffrowsAsync();
    }

    private static async Task EnsureAuditLogs(IFreeSql fsql)
    {
        if (await fsql.Select<AuditLog>().AnyAsync()) return;

        var now = DateTime.UtcNow;
        await fsql.Insert<AuditLog>().AppendData([
            new AuditLog { Id = "al_1", UserId = "user_admin", Action = "Login", IpAddress = "127.0.0.1", CreatedAt = now.AddHours(-1) },
            new AuditLog { Id = "al_2", UserId = "user_admin", Action = "CreateWorkflow", ResourceType = "Workflow", ResourceId = "wf_001", IpAddress = "127.0.0.1", CreatedAt = now.AddDays(-35) },
            new AuditLog { Id = "al_3", UserId = "user_operator", Action = "Login", IpAddress = "10.0.0.21", CreatedAt = now.AddDays(-1) },
            new AuditLog { Id = "al_4", UserId = "user_operator", Action = "ExportDataset", ResourceType = "DatasetInfo", ResourceId = "ds_info_002", IpAddress = "10.0.0.21", CreatedAt = now.AddDays(-5) }
        ]).ExecuteAffrowsAsync();
    }
}
