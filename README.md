<div align="center">

  <img src="docs/assets/logo-flat-black-sharp.png#gh-light-mode-only" alt="Makara Logo" width="200">
  <img src="docs/assets/logo-flat-white-sharp.png#gh-dark-mode-only" alt="Makara Logo" width="200">

# Makara · 摩羯

> **数据与模型交汇之处，魔法自然发生。**

*AI 模型工厂 — 从业务数据到专属小模型，只需要一条工作流。*

*WPF 客户端 · 可视化工作流编排 · 多数据源抽取 · 自动生成训练数据集 · 定时自动迭代*

*逐步演进为 Model-as-a-Service 基础设施：AI 应用通过 API 发出自然语言需求，Makara 自动分析需要哪些数据源、生成什么样的数据集、训练什么样的专属小模型，精准匹配用户与应用的真实需求。*

[![License](https://img.shields.io/badge/License-PolyForm%20NC-orange.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![WPF](https://img.shields.io/badge/WPF-Windows-512BD4?logo=windows&logoColor=white)](https://github.com/dotnet/wpf)
[![Python](https://img.shields.io/badge/Python-3.11+-blue.svg)](https://python.org)
[![CUDA](https://img.shields.io/badge/CUDA-12.1-76B900?logo=nvidia&logoColor=white)](https://developer.nvidia.com/cuda-toolkit)
[![Ascend](https://img.shields.io/badge/Ascend%20NPU-910B-red?logo=huawei&logoColor=white)](https://www.hiascend.com/)
[![Server](https://img.shields.io/badge/server-Windows%20%7C%20Linux%20%7C%20openEuler-lightgrey)](https://github.com/neillengh/makara)
[![GitHub Stars](https://img.shields.io/github/stars/neillengh/makara.svg?style=social)](https://github.com/neillengh/makara/stargazers)

[功能亮点](#-功能亮点) ·
[快速上手](#-快速上手) ·
[架构设计](#-架构设计) ·
[路线图](#-路线图) ·
[企业版](#-开源版-vs-企业版) ·
[贡献指南](#-贡献指南)

</div>

> ⚠️ **本项目正在开发中，MVP 尚未发布。** 预计 v0.1.0 将在半年内发布。欢迎 Star 关注进展！

---

## 🌊 关于 Makara

Makara 得名于印度神话中的**摩羯**——一种半陆半海的混合生物，上半身为象，下半身为鱼，象征着**不同世界的融合与创造**。

正如摩羯连接着陆地与海洋，**Makara 连接着数据与模型**：

> 你的业务系统里沉淀了大量数据——客服工单、知识库、合同文档、交易记录……但这些数据只能用来查报表，没法直接变成 AI 能力。

**Makara 帮你把业务数据变成专属小模型：**

1. 连接你的业务数据库 / API / 文件
2. 用可视化工作流编排数据抽取和处理流程
3. 自动生成训练数据集
4. （未来）自动微调、评测、部署
5. （最终）AI 应用通过 API 发出自然语言需求，Makara 自动分析需要哪些数据源、生成什么样的数据集、训练什么样的专属小模型，精准匹配用户与应用的真实需求

**效果：** 用你的业务数据训练的 7B 小模型，在特定领域效果接近 70B 大模型，但推理成本降到 1/10，速度提升数倍。

**目标客户：** 政府、大型企业、信创项目——买了 GPU/NPU 服务器，但缺乏 AI 团队，想把业务数据快速转化为 AI 能力。

---

## ✨ 功能亮点

### 🖥️ WPF 客户端
- **原生 Windows 体验** — 基于 WPF + MVVM 架构，流畅的交互和动画
- **工作流画布** — n8n 式拖拽编排，节点+连线，所见即所得
- **实时进度推送** — SSE 流式推送，任务进度、日志实时更新
- **多服务端切换** — 支持配置多个服务端地址，一键切换
- **暗色/亮色主题** — 内置主题切换，护眼模式

### 🔄 可视化工作流编排
- **拖拽式画布** — 节点+连线，像搭积木一样编排数据流水线
- **丰富节点类型** — 触发器、数据源、数据清洗、字段映射、数据集构建、条件判断、通知
- **Cron 定时触发** — 支持 Cron 表达式定时运行，实现周期性数据抽取
- **条件分支** — if/else 条件判断，根据数据质量决定流程走向
- **执行记录** — 每次运行都有完整日志、耗时统计、产物追踪
- **模板市场** — 内置常用模板，一键套用（如"每周从客服库生成问答数据集"）

### 📊 多数据源连接
- **关系型数据库** — SQL Server / MySQL / PostgreSQL
- **文件导入** — CSV / JSON / Excel
- **可视化字段映射** — 拖拽配置表字段到训练数据格式
- **SQL 自定义查询** — 支持自定义 SQL、增量抽取、条件过滤
- **连接测试** — 一键测试数据源连接是否正常
- **数据预览** — 抽取前预览样本数据，确认没问题再跑

### 🧹 数据处理 & 数据集生成
- **数据清洗** — 去重、格式标准化、空值过滤、异常值处理
- **多种输出格式** — 问答对（QA）、指令微调（Instruction）、纯文本、多轮对话
- **训练集/验证集划分** — 自定义比例，自动划分
- **数据质量统计** — 样本量、平均长度、分布情况一目了然
- **数据溯源** — 每个数据集都记录来源、抽取时间、转换规则

### 🔌 REST API + SSE 实时推送
- **标准 REST API** — 所有功能都有 HTTP 接口，方便集成到其他系统
- **SSE 流式推送** — 任务进度、日志实时推送，服务端有变化才通知
- **纯 HTTP 协议** — 无需 WebSocket，跨平台、易调试、信创友好
- **Swagger 文档** — 自动生成 API 文档，开箱即用

### 🇨🇳 信创 & 国产化支持（规划中）
- **双计算后端** — 同时支持 NVIDIA CUDA 和华为昇腾 NPU
- **国产 OS 兼容** — 服务端支持 openEuler、银河麒麟等国产操作系统
- **国产 CPU 兼容** — 支持鲲鹏、飞腾等 ARM64 架构处理器
- **设备无关抽象层** — 上层代码无需改动，底层自动适配

---

> 📸 截图和演示视频将在 MVP 发布时补充。

---

## 🚀 快速上手

### 环境要求

#### 客户端（用户电脑）
| 组件 | 最低要求 | 推荐配置 |
|------|---------|---------|
| **操作系统** | Windows 10 64位 | Windows 11 |
| **.NET 桌面运行时** | .NET 10.0 | .NET 10.0 |
| **内存** | 4GB | 8GB+ |
| **硬盘** | 1GB | 5GB+ |

#### 服务端（开发/测试环境）
| 组件 | 最低要求 | 推荐配置 |
|------|---------|---------|
| **操作系统** | Windows 10+ / Linux (Ubuntu 22.04+) | Windows Server 2022+ / Linux (Ubuntu 22.04+ / openEuler) |
| **.NET SDK** | .NET 10.0 | .NET 10.0 |
| **内存** | 8GB | 16GB+ |
| **硬盘** | 10GB | 50GB+（数据集 + 未来模型文件） |

> 💡 当前版本（MVP）主要完成**工作流 + 数据抽取 + 数据集生成**，不需要 GPU。模型微调功能在后续版本中加入。

### 安装服务端

```bash
# 克隆仓库
git clone https://github.com/neillengh/makara.git
cd makara

# 构建并启动服务端
dotnet run --project src/Makara.Server

# 服务端默认监听 http://localhost:5000
# Swagger 文档：http://localhost:5000/swagger
```

### 安装 WPF 客户端

```powershell
# 从源码构建
git clone https://github.com/neillengh/makara.git
cd makara
dotnet run --project src/Makara.Desktop
```

### 30 秒上手

```
1. 启动服务端
   dotnet run --project src/Makara.Server

2. 启动 WPF 客户端
   dotnet run --project src/Makara.Desktop

3. 在客户端中：
   - 添加服务端地址（默认 http://localhost:5000）
   - 创建工作流
   - 拖拽数据源节点，配置数据库连接
   - 拖拽字段映射节点，配置训练格式
   - 拖拽数据集构建节点，设置输出格式
   - 点击"运行"，实时查看进度
   - 运行完成，导出数据集
```

---

## 📖 使用指南

### WPF 客户端功能模块

| 模块 | 功能说明 |
|------|---------|
| **工作流画布** | n8n 式拖拽画布，编排数据抽取→清洗→数据集生成全流程 |
| **工作流模板** | 内置常用模板，一键套用 |
| **执行记录** | 查看工作流运行历史、执行日志、耗时统计 |
| **数据源管理** | 添加/编辑数据库连接、文件数据源，测试连接，预览数据 |
| **字段映射配置** | 可视化配置数据源字段到训练数据格式的映射 |
| **数据集管理** | 查看数据集统计、样本预览、数据溯源 |
| **服务端管理** | 添加/编辑/切换多个服务端连接 |
| **设置** | 主题切换、默认参数配置 |

### 工作流节点说明

| 节点类型 | 功能 | 输入 | 输出 |
|---------|------|------|------|
| **Trigger（触发器）** | 工作流启动入口，支持手动 / Cron 定时 | - | - |
| **DataSource（数据源）** | 连接数据库或文件，抽取原始数据 | 配置 + SQL/查询条件 | 原始数据表 |
| **DataClean（数据清洗）** | 去重、格式标准化、空值过滤、异常值处理 | 原始数据 | 清洗后数据 |
| **FieldMap（字段映射）** | 映射到训练格式（QA / 指令 / 文本 / 多轮对话） | 清洗后数据 | 格式化数据 |
| **DatasetBuild（数据集构建）** | 划分训练集/验证集，输出最终数据集 | 格式化数据 | 数据集文件 |
| **Condition（条件判断）** | if/else 分支，如"数据量大于阈值才继续" | 数据统计 | 分支流向 |
| **Notify（通知）** | 邮件 / 钉钉 / 企业微信 / 飞书通知 | 任意 | 通知结果 |

### 典型工作流示例：每周生成客服问答数据集

```
[Cron Trigger: 每周一 02:00]
         │
         ▼
[DataSource: 客服工单库 SQL Server]
         │  SELECT Question, Answer, Category
         │  FROM ServiceTickets
         │  WHERE Status = 'Closed'
         │    AND CreateTime >= DATEADD(day, -7, GETDATE())
         ▼
[DataClean: 去重 + 过滤空答案 + 格式标准化]
         │
         ▼
[FieldMap: Question→instruction, Answer→output]
         │  systemPrompt: "你是专业的客服助手..."
         ▼
[Condition: 数据量 > 100 条 ?]
        ╱ ╲
     是 ╱   ╲ 否
      ╱     ╲
     ▼       ▼
[DatasetBuild:   [Notify: 数据量不足，
 训练/验证 9:1   发送钉钉通知]
 输出: JSON]
     │
     ▼
[Notify: 飞书通知"客服问答数据集 v1.2 已生成，共 5,230 条"]
```

### 服务端 API

服务端提供 RESTful API + SSE 实时推送，可以不用 WPF 客户端，直接调用：

```bash
# 创建工作流
curl -X POST http://server:5000/api/workflows \
  -H "Content-Type: application/json" \
  -d '{"name":"客服数据集","nodes":[...]}'

# 手动触发工作流
curl -X POST http://server:5000/api/workflows/1/run

# 查看任务状态
curl http://server:5000/api/tasks/abc123

# SSE 实时订阅任务进度
curl -N http://server:5000/api/tasks/abc123/stream

# 下载数据集
curl -O http://server:5000/api/datasets/ds_001/train.json
```

---

## 🏗️ 架构设计

### 整体架构

```
┌─────────────────────────────────────────────────────────────┐
│                  客户端（Windows）                          │
│                                                             │
│  ┌──────────────────────────────────────────────────────┐   │
│  │           Makara Desktop (WPF + MVVM)                │   │
│  │  工作流画布 · 数据源管理 · 任务监控 · 数据集管理       │   │
│  │  科技风 UI · 流畅动画 · 原生桌面体验               │   │
│  └───────────────────────┬──────────────────────────────┘   │
└──────────────────────────┼──────────────────────────────────┘
                           │
                  REST API + SSE
                  （纯 HTTP 协议）
                           │
┌──────────────────────────┼──────────────────────────────────┐
│               服务端（ASP.NET Core / 全平台）                │
│                                                             │
│  ┌──────────────────────────────────────────────────────┐   │
│  │                   Makara Server                      │   │
│  │  ┌──────────────────────────────────────────────┐    │   │
│  │  │            工作流引擎（核心）                  │    │   │
│  │  │  节点调度 · 状态管理 · 数据流转 · 错误处理     │    │   │
│  │  └──────────────────────────────────────────────┘    │   │
│  │  ┌────────────┐  ┌──────────┐  ┌────────────────┐   │   │
│  │  │  数据源层   │  │ 数据处理 │  │  数据集生成     │   │   │
│  │  │  SQL/文件   │  │ 清洗·映射│  │  格式转换·划分  │   │   │
│  │  └────────────┘  └──────────┘  └────────────────┘   │   │
│  │  ┌────────────┐  ┌──────────┐  ┌────────────────┐   │   │
│  │  │ 任务调度    │  │ API 层   │  │  数据存储       │   │   │
│  │  │ Hangfire    │  │ REST+SSE │  │  SQLite + 文件  │   │   │
│  │  └────────────┘  └──────────┘  └────────────────┘   │   │
│  └───────────────────────┬──────────────────────────────┘   │
│                                                             │
│      Windows · Linux · openEuler · 麒麟 · Docker            │
└──────────────────────────┬──────────────────────────────────┘
                           │
              （未来）HTTP API 调用
                           │
┌──────────────────────────┼──────────────────────────────────┐
│              AI 训练内核（Python / 双计算后端）               │
│              （规划中，后续版本加入）                         │
│                                                             │
│  ┌─────────────────┐    ┌───────────────────────┐          │
│  │  CUDA 后端      │    │  Ascend NPU 后端      │          │
│  │  (NVIDIA GPU)   │    │  (华为昇腾 910B)      │          │
│  └─────────────────┘    └───────────────────────┘          │
│                                                             │
│  LoRA / QLoRA 微调 · 模型评测 · 模型导出 · 推理服务          │
└─────────────────────────────────────────────────────────────┘
```

### 项目结构

```
Makara/
├── src/
│   ├── Makara.Desktop/          # WPF 桌面客户端（Windows）
│   │   ├── ViewModels/          # MVVM 视图模型
│   │   ├── Views/               # 视图（Window / Page / UserControl）
│   │   │   ├── WorkflowCanvas/  # 工作流画布控件
│   │   │   └── ...
│   │   ├── Services/            # 客户端服务（API 调用、SSE 订阅、本地存储）
│   │   ├── Styles/              # 样式与主题（科技风）
│   │   ├── Converters/          # WPF 值转换器
│   │   └── App.xaml             # 应用入口
│   │
│   ├── Makara.Server/           # ASP.NET Core 服务端（跨平台）
│   │   ├── Controllers/         # REST API 控制器
│   │   ├── Middleware/          # SSE 流式响应中间件
│   │   └── Program.cs           # 服务端入口
│   │
│   ├── Makara.Core/             # 核心领域模型 & 接口（共享）
│   │   ├── Models/              # 实体模型
│   │   │   ├── Workflow/        # 工作流（节点/连线/定义/执行记录）
│   │   │   ├── DataSource/      # 数据源配置
│   │   │   ├── Dataset/         # 数据集相关
│   │   │   └── Task/            # 任务与执行状态
│   │   ├── Abstractions/        # 抽象接口
│   │   │   ├── IWorkflowEngine.cs
│   │   │   ├── INodeExecutor.cs
│   │   │   └── IDataSourceProvider.cs
│   │   ├── DTOs/                # 数据传输对象
│   │   └── Enums/               # 枚举定义
│   │
│   ├── Makara.Infrastructure/   # 基础设施层（服务端实现）
│   │   ├── Workflow/            # 工作流引擎实现
│   │   │   ├── WorkflowEngine.cs
│   │   │   ├── Scheduler/       # Hangfire 定时调度
│   │   │   └── Nodes/           # 内置节点实现
│   │   │       ├── Trigger/
│   │   │       ├── DataSource/
│   │   │       ├── DataProcess/
│   │   │       └── Notify/
│   │   ├── DataSources/         # 数据源连接器
│   │   │   ├── SqlServer/
│   │   │   ├── MySql/
│   │   │   ├── PostgreSql/
│   │   │   └── File/            # CSV / JSON / Excel
│   │   ├── Data/                # 数据访问（FreeSql + SQLite）
│   │   └── Services/            # 业务逻辑实现
│   │
│   └── Makara.AI/               # Python AI 训练内核（规划中）
│       └── ...
│
├── tests/
│   ├── Makara.Core.Tests/       # 核心层单元测试
│   └── Makara.Server.Tests/     # 服务端 API 测试
│
├── examples/                     # 示例工作流配置
├── docs/                         # 文档
└── deploy/                       # 部署脚本
```

### 设计原则

| 原则 | 说明 |
|------|------|
| **数据→模型为核心** | Makara 的本质是把业务数据转化为 AI 模型，技术栈只是实现手段 |
| **工作流驱动** | 所有能力以节点形式暴露，通过可视化工作流编排组合 |
| **客户端-服务端分离** | WPF 客户端负责交互，服务端负责计算，可独立部署扩展 |
| **纯 HTTP 通信** | REST API + SSE，标准 HTTP 协议，跨平台、易集成、信创友好 |
| **.NET 做骨架，Python 做血肉** | C#/.NET 做服务编排和客户端，Python 做 AI 训练，各司其职 |
| **渐进式 AI 内核** | 先把数据流水线做好，再逐步加入模型微调能力 |
| **信创就绪** | 架构设计考虑国产 OS、国产 CPU、国产 NPU 兼容性 |
| **插件化扩展** | 数据源、工作流节点、训练方法均可插件扩展 |

### 部署方式

| 部署模式 | 适用场景 | 说明 |
|---------|---------|------|
| **单机一体化** | 个人使用、开发测试 | 客户端 + 服务端都在同一台 Windows 机器上 |
| **客户端-服务端分离** | 团队使用 | 服务端部署在服务器（Linux/Windows），多人用 WPF 客户端连接 |
| **Docker 部署** | 生产环境 | Docker 一键部署服务端 |
| **纯 API 调用** | 系统集成 | 直接调用 HTTP API，不用 WPF 客户端 |

---

## 🛤️ 路线图

> 开发节奏：一个人业余时间，预计半年左右完成 MVP。

### ✅ Phase 0: 项目规划
- [x] 项目命名与定位
- [x] 架构设计
- [x] 技术选型
- [x] MVP 范围确定

### 🚧 Phase 1: 核心框架（第 1-2 个月）
- [ ] ASP.NET Core 服务端基础框架（API + SSE + FreeSql + SQLite）
- [ ] WPF 客户端基础框架（MVVM + 科技风主题 + API 客户端）
- [ ] 工作流引擎核心（节点抽象、执行引擎、状态管理、数据流转）
- [ ] 任务调度（Hangfire + SQLite）
- [ ] 核心数据模型与 DTO

### 🎨 Phase 2: WPF 工作流画布（第 2-3 个月）
- [ ] 工作流画布控件（节点拖拽、连线、缩放、平移）
- [ ] 节点属性面板
- [ ] 工作流保存/加载
- [ ] 执行记录查看
- [ ] 科技风 UI 打磨（动画、渐变、视觉效果）

### 📊 Phase 3: 数据源 & 数据处理（第 3-4 个月）
- [ ] 数据源抽象层（IDataSourceProvider 接口）
- [ ] SQL Server 连接器
- [ ] MySQL 连接器
- [ ] PostgreSQL 连接器
- [ ] CSV / JSON / Excel 文件连接器
- [ ] DataSource 工作流节点
- [ ] DataClean 节点（去重、空值、格式标准化）
- [ ] FieldMap 节点（字段映射到 4 种训练格式）
- [ ] DatasetBuild 节点（训练/验证集划分 + 输出）
- [ ] WPF 数据源管理界面
- [ ] 数据预览功能

### ⏰ Phase 4: 定时任务 & 通知（第 4-5 个月）
- [ ] Cron Trigger 节点
- [ ] Condition 条件节点
- [ ] Notify 节点（钉钉 / 企业微信 / 飞书）
- [ ] 工作流模板市场（内置常用模板）
- [ ] SSE 实时进度推送完善
- [ ] 执行日志与历史记录

### 🎯 Phase 5: MVP 发布（第 5-6 个月）
- [ ] Bug 修复与稳定性优化
- [ ] UI/UX 打磨
- [ ] 文档完善（安装指南、使用教程、API 文档）
- [ ] 示例工作流
- [ ] Demo 视频（AI 生成）
- [ ] v0.1.0 发布
- [ ] 知乎 / 掘金 / 微信公众号宣传

### 🔮 后续规划（MVP 之后）
- [ ] **Phase 6**: Python AI 内核 + LoRA 微调（CUDA 后端）
- [ ] **Phase 7**: 模型评测 + 模型版本管理
- [ ] **Phase 8**: 模型部署 + 推理 API
- [ ] **Phase 9**: 华为昇腾 NPU 适配（信创）
- [ ] **Phase 10**: 企业版功能（多租户、数据安全、RBAC、审计）
- [ ] **Phase 11**: 全自动 Model-as-a-Service（AI 应用通过自然语言 API 发出需求，Makara 自动分析数据源、生成数据集、训练专属小模型，精准匹配应用需求）

---

## 💼 开源版 vs 企业版

Makara 采用 **开源核心 + 企业版**（Open Core）模式。开源版基于 PolyForm NonCommercial 协议，免费供个人和非商业用途使用；企业版提供商业授权及企业级特性。

| 功能 | 开源版（PolyForm NC） | 企业版（商业授权） |
|------|:-------------------:|:-----------------:|
| WPF 桌面客户端 | ✅ | ✅ |
| 工作流引擎 & 画布 | ✅ | ✅ |
| 数据源连接器（SQL Server / MySQL / PostgreSQL / 文件） | ✅ | ✅ |
| 数据清洗 & 数据集生成 | ✅ | ✅ |
| 定时任务（Cron） | ✅ | ✅ |
| REST API + SSE | ✅ | ✅ |
| 单租户 | ✅ | ✅ |
| LoRA / QLoRA 微调 | ✅ | ✅ |
| 模型推理 API | ✅ | ✅ |
| **多租户隔离** | ❌ | ✅ |
| **数据脱敏节点** | ❌ | ✅ |
| **全链路审计日志** | ❌ | ✅ |
| **敏感配置加密存储** | ❌ | ✅ |
| **RBAC 权限管理** | ❌ | ✅ |
| **数据留存策略** | ❌ | ✅ |
| **灰度发布 & A/B 测试** | ❌ | ✅ |
| **模型版本对比** | ❌ | ✅ |
| **LDAP / SSO 集成** | ❌ | ✅ |
| **华为昇腾 NPU 深度优化** | 基础适配 | 深度优化 + 技术支持 |
| **Oracle / MongoDB 连接器** | ❌ | ✅ |
| **技术支持** | 社区 | 专属技术支持 + SLA |
| **定制开发** | ❌ | ✅ |

> 企业版咨询请联系：**neillengh@163.com**

---

## 🤝 贡献指南

我们欢迎任何形式的贡献！不管你是 .NET/WPF 开发者还是 Python 开发者，都能找到适合你的位置。

- 🐛 提交 Bug：[Issues](https://github.com/neillengh/makara/issues)
- 💡 功能建议：[Discussions](https://github.com/neillengh/makara/discussions)
- 🔧 提交代码：查看 [贡献指南](CONTRIBUTING.md)
- 🔒 **安全漏洞报告**：如发现安全漏洞，请发邮件至 **neillengh@163.com**，请勿公开提交 Issue。

### 开发环境搭建

请参考 [快速上手](#-快速上手) 章节。额外说明：

```powershell
# 构建整个解决方案
dotnet build Makara.slnx

# 运行测试
dotnet test Makara.slnx
```

### 你可以贡献的方向

| 方向 | 技能要求 | 难度 |
|------|---------|------|
| WPF UI 设计/优化 | C# + WPF + MVVM + XAML | ⭐ 入门 |
| 服务端 API 开发 | C# + ASP.NET Core + FreeSql | ⭐⭐ 中等 |
| 新的数据源集成 | C# | ⭐⭐ 中等 |
| 工作流画布增强 | C# + WPF + 图形编程 | ⭐⭐⭐ 进阶 |
| Docker / Linux 部署支持 | DevOps | ⭐⭐ 中等 |
| 安装包制作 | Inno Setup / Wix | ⭐⭐ 中等 |
| 文档/教程 | 中文写作 | ⭐ 入门 |

---

## ❓ FAQ

**Q: 为什么用 WPF 做客户端，不用 Web？**
A: Makara 的典型使用场景是：服务端部署在机房/服务器，用户在自己的 Windows 电脑上操作。WPF 作为原生 Windows 桌面应用，能提供更好的性能、更流畅的交互体验、更好的视觉效果。而且——市面上 AI 工具全是 Web UI，WPF 桌面客户端本身就是一个差异化亮点。

**Q: 现在就能做模型微调吗？**
A: 当前 MVP 版本专注于**数据抽取 + 数据集生成**的工作流。模型微调功能会在后续版本中加入。你可以先用 Makara 生成训练数据集，然后用 LLaMA-Factory 等工具进行微调。

**Q: 服务端支持 Linux 吗？**
A: 支持。Makara 的服务端（ASP.NET Core）可以在 Windows、Linux、openEuler 等系统上运行。只有 WPF 桌面客户端是 Windows 专用的。

**Q: 支持华为昇腾 NPU 吗？**
A: 规划中。架构设计上预留了双计算后端（CUDA + NPU），会在 MVP 之后的版本中逐步加入昇腾 NPU 支持。

**Q: 为什么用 .NET + Python 双栈？不用 Python 做全部？**
A: 因为 WPF 客户端需要 .NET，而且 .NET 做服务端 API 也很成熟稳定。Python 做 AI 训练内核，充分利用 PyTorch 生态的成熟度。两者通过 HTTP API 通信，各司其职。

**Q: 我必须懂 Python 才能用吗？**
A: 不需要。当前版本完全不需要 Python。未来加入微调功能后，Python 内核也是透明的，你通过 WPF 客户端就能使用所有功能。

**Q: 可以不用 WPF 客户端，直接调用 API 吗？**
A: 完全可以。服务端提供标准的 REST API + SSE，你可以用任何语言调用，或者集成到自己的系统中。

**Q: SSE 是什么？和 WebSocket 有什么区别？**
A: SSE（Server-Sent Events）是一种基于 HTTP 的服务端推送技术。和 WebSocket 相比，SSE 是单向的（只能服务端推客户端），但它更简单、就是标准 HTTP 请求、调试方便、跨平台兼容性好、信创环境下也完全没问题。对于任务进度推送这种场景，SSE 完全够用而且更轻量。

**Q: 支持哪些数据源？能自己扩展吗？**
A: MVP 版本支持 SQL Server、MySQL、PostgreSQL，以及 CSV/JSON/Excel 文件。数据源层是插件化设计的，实现 `IDataSourceProvider` 接口就能接入新的数据源。

**Q: 能定时自动从数据库抽数据生成数据集吗？**
A: 完全可以。配置一个 Cron 触发器 → 数据源节点 → 清洗 → 字段映射 → 数据集构建 → 通知。整条流水线全自动运行，不用人工干预。

**Q: 这个项目是什么开源协议？能商用吗？**
A: 本项目基于 PolyForm NonCommercial 协议，个人和非商业用途免费，商业用途需购买授权。详见 [License](#-license) 章节。

---

## 📄 License

本项目基于 [PolyForm NonCommercial License 1.0.0](https://polyformproject.org/licenses/noncommercial/1.0.0/) 协议（源码可见协议）。

### 协议概要

| 维度 | 非商业用途（免费） | 商业用途（需授权） |
|------|:------------------:|:-----------------:|
| 查看 / 下载源码 | ✅ | ✅ |
| 个人学习研究 | ✅ | — |
| 修改代码 | ✅（无需公开修改） | 需授权 |
| 分发 / 分享 | ✅（保留版权声明） | 需授权 |
| 商业产品中集成 | ❌ | 需授权 |
| 企业内部使用 | ❌ | 需授权 |
| 对外提供服务 | ❌ | 需授权 |
| 销售 / 二次售卖 | ❌ | 需授权 |

### 非商业用途（免费使用）

以下场景**无需购买商业授权**，直接免费使用：

| 场景 | 示例 | 是否需要授权 |
|------|------|:------------:|
| 个人学习研究 | 学生、开发者下载来学习工作流编排和数据集生成 | ✅ 免费 |
| 个人项目使用 | 开发者用自己的数据跑数据集生成流程 | ✅ 免费 |
| 开源社区贡献 | 提交 PR、改进代码、分享模板 | ✅ 免费 |
| 教学 / 培训 | 学校课程、技术分享会上的演示教学 | ✅ 免费 |
| 个人评测试用 | 评估 Makara 是否满足需求 | ✅ 免费 |

> 修改后的代码**无需公开**，你可以自由修改并在非商业范围内使用。

### 商业用途（需购买授权）

以下场景**必须获得商业授权**：

| 场景 | 示例 | 是否需要授权 |
|------|------|:------------:|
| 企业内部使用 | 公司用 Makara 处理业务数据、生成训练数据集、训练模型 | ⚠️ 需授权 |
| 政府项目使用 | 政府部门部署 Makara 用于信创 AI 平台 | ⚠️ 需授权 |
| 集成到商业产品 | 将 Makara 嵌入到你的商业软件或 SaaS 产品中 | ⚠️ 需授权 |
| 对外提供服务 | 用 Makara 为客户提供数据处理 / 模型训练服务 | ⚠️ 需授权 |
| 二次开发销售 | 基于 Makara 二次开发后销售或授权给第三方 | ⚠️ 需授权 |
| 内部部署为服务 | 在企业内部部署 Makara，多部门通过 API 调用 | ⚠️ 需授权 |

### 商业授权（企业版）

购买商业授权后，您将获得：

| 权益 | 说明 |
|------|------|
| **免除 NonCommercial 限制** | 可在企业环境内自由使用、部署、集成 |
| **多租户隔离** | 不同部门 / 不同客户之间数据和模型完全隔离 |
| **数据脱敏节点** | 自动识别和脱敏身份证、手机号等敏感信息 |
| **全链路审计日志** | 数据操作、模型训练、部署变更全记录可追溯 |
| **敏感配置加密存储** | 数据库连接、API 密钥等加密存储 |
| **RBAC 权限管理** | 细粒度角色权限控制 |
| **数据留存策略** | 按策略自动清理训练数据和临时文件 |
| **灰度发布 & A/B 测试** | 模型版本对比和灰度上线 |
| **LDAP / SSO 集成** | 对接企业统一身份认证 |
| **Oracle / MongoDB 连接器** | 额外的企业级数据源支持 |
| **华为昇腾 NPU 深度优化** | 深度适配 + 专属技术支持 |
| **专属技术支持 + SLA** | 工单响应、问题修复、定期版本更新 |
| **定制开发** | 按需定制功能和适配 |

### 如何获取商业授权

1. 发送邮件至 **neillengh@163.com**，说明您的使用场景和规模
2. 我们将在 2 个工作日内与您联系，提供报价方案
3. 签署商业授权协议后，获取企业版授权码及安装包

### 常见问题

**Q: 我是个人开发者，想用 Makara 做自己的项目，需要付费吗？**
A: 不需要。个人非商业用途完全免费。

**Q: 我的公司想先用开源版试一下，之后再买企业版，可以吗？**
A: 可以。个人评测试用是免费的。但企业内部实际使用 Makara 处理业务数据属于商业用途，需要购买授权。

**Q: 开源版和企业版功能有区别吗？**
A: 核心功能（工作流、数据源、数据集生成、定时任务）完全一致。企业版额外提供多租户、数据安全、审计、RBAC 等企业级功能。

**Q: 我修改了 Makara 的代码，需要公开吗？**
A: 不需要。PolyForm NC 协议不强制公开修改后的代码。但注意，修改后的版本仍受 NonCommercial 限制，不能用于商业用途。

**Q: 开源版为什么不是 Apache / MIT 等宽松协议？**
A: 因为 Makara 的核心用户群体是企业和政府，需要合理的商业模式来支撑长期开发和维护。PolyForm NC 既能保证个人免费使用，又能保护商业价值。

---

## 🙏 致谢

- 感谢 [LLaMA-Factory](https://github.com/hiyouga/LLaMA-Factory) 项目，微调参数设计参考了它的优秀实践
- 感谢 HuggingFace、PEFT、PyTorch 等项目，让 AI 变得触手可及
- 感谢 .NET 社区和 WPF 社区
- Makara 的命名灵感来自印度神话中的摩羯——融合与创造的象征

---

<div align="center">

**Star 本项目** 以获取最新动态 ⭐

*连接数据与模型 · 用 ❤️ + C# + Python 打造*

</div>
