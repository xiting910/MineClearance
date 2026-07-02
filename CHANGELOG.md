# Changelog

本文件记录了项目的所有重要变更。每个版本的变更都应在发布时记录在此文件中。

格式基于 [Keep a Changelog](https://keepachangelog.com/zh-CN/1.1.0/),
版本号遵循 [Semantic Versioning](https://semver.org/lang/zh-CN/).

---

## [Unreleased]

### Added

- **领域模型设计**: 定义完整的扫雷游戏领域模型
  - 枚举: `CellType`（格子类型）、`GameDifficulty`（难度）、`GameStatus`（游戏状态）、`HintStrategyType`（提示策略）
  - 接口: `IGame`（游戏核心）、`IGameBoard`（棋盘）、`IGameManager`（游戏管理器）、`IGameResultRepository`（结果仓储）、`IGameTimer`（计时器）、`IHintProvider`（提示提供者）、`IMineField`（地雷场）、`IMineGenerator`（地雷生成器）
  - 模型: `Cell`（格子，含 `INotifyPropertyChanged`）、`Position`（位置 record struct）、`GameConfig`（游戏配置 record）、`GameResult`（游戏结果 record）、`GameSaveData`（存档数据 record）
  - 事件参数: `GameChangedEventArgs`、`GameStatusChangedEventArgs`、`GameTimerTickEventArgs`、`MineFieldGeneratedEventArgs`
- 添加 `Constants` 常量类，包含内置难度对应的 `GameConfig` 预置实例
- 项目初始化，搭建 Clean Architecture 三层架构（Core / Infrastructure / UI）
- 使用 `.slnx` 解决方案文件格式（新版 .NET XML 格式）
- 引入 .NET 10.0 目标框架，全局启用可空引用类型与 XML 文档生成
- 集成 Avalonia UI 跨平台桌面框架（Windows / Linux / macOS）
- 集成 CommunityToolkit.Mvvm 工具包
- Core 层添加 `IServiceCollectionExtensions.AddCore()` DI 注册扩展方法
- Infrastructure 层添加项目骨架及 Core 层引用
- UI 层添加 Avalonia 桌面应用入口项目及 `Microsoft.Extensions.Hosting` 通用主机
- 添加 xUnit + Moq 单元测试项目骨架
- 配置 GitHub Actions CI/CD 工作流（CI 构建测试、CodeQL 安全分析、Release 发布）
- 配置 Dependabot 自动依赖更新（NuGet + GitHub Actions 分组策略）
- 添加 `.editorconfig` 代码风格统一配置（含大量 CA 代码分析规则）
- 添加 Issue / PR 模板（Bug 报告、功能建议、PR 清单）
- 添加 MIT 许可证
- 添加 README.md 文档，包含项目特性、结构、快速开始指南
- 增强 `.gitignore`，添加 `*.user` 和 `*.bat` 忽略规则

[Unreleased]: https://github.com/xiting910/MineClearance/commits/main
