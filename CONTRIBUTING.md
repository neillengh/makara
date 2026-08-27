# 贡献指南

感谢你对 Makara 项目的兴趣！本文档介绍如何参与贡献。

## 📋 目录

- [行为准则](#行为准则)
- [如何贡献](#如何贡献)
- [开发环境搭建](#开发环境搭建)
- [代码规范](#代码规范)
- [提交规范](#提交规范)
- [Pull Request 流程](#pull-request-流程)

## 行为准则

参与本项目的每位贡献者都应遵守 [行为准则](CODE_OF_CONDUCT.md)。请保持尊重和友善。

## 如何贡献

### 报告 Bug

1. 在 [Issues](https://github.com/neillengh/makara/issues) 中搜索是否已有相同问题
2. 如果没有，创建新 Issue，使用 Bug 报告模板
3. 请包含以下信息：
   - Makara 版本号
   - 操作系统和版本
   - 复现步骤
   - 预期行为和实际行为
   - 错误日志（如有）

### 提交功能建议

1. 先在 [Issues](https://github.com/neillengh/makara/issues) 中搜索是否已有类似建议
2. 创建新 Issue，使用功能建议模板
3. 描述使用场景和期望效果

### 提交代码

1. Fork 本仓库
2. 创建功能分支：`git checkout -b feature/your-feature-name`
3. 编写代码并测试
4. 提交：`git commit -m 'feat: 添加了 XXX 功能'`
5. 推送：`git push origin feature/your-feature-name`
6. 创建 Pull Request

## 开发环境搭建

详见 README 中的 [快速上手](README.md#-快速上手) 章节。

## 代码规范

### C# / .NET

- 遵循 [Microsoft C# 命名约定](https://learn.microsoft.com/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- 使用 4 空格缩进
- 公共成员使用 PascalCase
- 私有成员使用 _camelCase
- 接口以 I 开头
- 异步方法以 Async 结尾

### Python（AI 内核）

- 遵循 PEP 8
- 使用 4 空格缩进
- 函数和变量使用 snake_case
- 类名使用 PascalCase

### 通用

- 每个文件末尾保留一个空行
- 删除末尾空格
- 不要提交未使用的 import 或变量
- 注释只写"为什么"，不写"是什么"

## 提交规范

使用 [Conventional Commits](https://www.conventionalcommits.org/) 格式：

```
<type>(<scope>): <subject>
```

### Type 列表

| Type | 说明 |
|------|------|
| `feat` | 新功能 |
| `fix` | Bug 修复 |
| `docs` | 文档变更 |
| `style` | 代码格式（不影响功能） |
| `refactor` | 重构（不新增功能，不修复 Bug） |
| `perf` | 性能优化 |
| `test` | 测试相关 |
| `chore` | 构建/工具/依赖变更 |

### 示例

```
feat(workflow): 添加工作流画布拖拽功能
fix(datasource): 修复 SQL Server 连接池泄漏
docs(readme): 更新快速上手章节
refactor(core): 重构节点执行引擎
```

## Pull Request 流程

1. 确保 PR 标题符合提交规范
2. PR 描述中说明改了什么、为什么改
3. 如果是新功能，确保有对应的测试
4. 确保 CI 检查通过
5. 等待 Code Review
6. 根据反馈修改后，不要创建新 PR，直接 push 到同一分支

## 贡献者协议

提交代码即表示你同意将代码版权转让给项目维护者，以便项目可以在 PolyForm NonCommercial 协议和商业协议下双重分发。

---

如有任何问题，欢迎在 [Discussions](https://github.com/neillengh/makara/discussions) 中讨论。
