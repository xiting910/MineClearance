# Changelog

本文件记录了项目的所有重要变更。每个版本的变更都应在发布时记录在此文件中。

格式基于 [Keep a Changelog](https://keepachangelog.com/zh-CN/1.1.0/),
版本号遵循 [Semantic Versioning](https://semver.org/lang/zh-CN/).

---

## [Unreleased]

### Added

- **领域服务实现**: Core 层完成所有接口实现
  - `Game` — 游戏核心逻辑（首次点击地雷生成、泛洪打开、状态管理、完成度计算、存档导出）
  - `GameBoardDictionary` — 基于字典的棋盘实现，`INotifyPropertyChanged` 驱动计数属性
  - `GameBoardDictionaryFactory` — 棋盘工厂
  - `GameFactory` — 基于 `IServiceScopeFactory` 的游戏对象图组装（每局一个 DI Scope）
  - `GameManager` — 游戏生命周期管理（开始新游戏、从存档恢复、保存并退出）
  - `GameTimer` — 基于 `Stopwatch` 的计时器，通过 `Refresh()` 由外部 UI 驱动
  - `MineField` — 地雷场实现，支持随机生成和位图恢复
  - `MineGenerator` — 地雷生成器（桩，待实现）
- **接口重构**: `INotifyPropertyChanged` 替代事件，`IServiceScopeFactory` 管理游戏生命周期
  - `IGame`: 实现 `INotifyPropertyChanged` + `IDisposable`，移除 `StatusChanged` 事件 / `Minefield` 属性，`Board` 可空，新增 `Config` / `GetSaveData()`
  - `IGameBoardDictionary`: 实现 `INotifyPropertyChanged`，移除 `Rows`/`Columns`/`SetAdjacentMineCounts`，新增 `GetCellStates()`
  - `IGameTimer`: 实现 `INotifyPropertyChanged`，移除 `Tick` 事件 / `ReStart()` / `IDisposable`，新增 `FirstStartTime` / `Refresh()` / `SetInitialTime()`
  - `IGameManager`: 方法签名重构（`RestoreFromSaveDataAsync`/`SaveAndExitAsync` 异步化，`StartNewGame` 拆分重载）
  - `IMineField`: 改为 `internal`，`Generate()` 返回 `int[]` 替代事件
  - `IMineGenerator.GenerateMines()`: 新增 `seed` 参数
  - `IGameResultRepository` → 重命名为 `IGameDataRepository`
- **Position 工具方法**: `DirectionOffsets`（8 方向静态偏移）、`GetAdjacentPositions()`、`GetAllPositions()`、`ToIndex()`
- **Constants 新增**: `MineValue = -1`
- **GameConfig 新增**: `GetTotalCellsToOpen()` 方法

### Changed

- `GameResult`/`GameSaveData` 自定义构造器替换为静态工厂方法
- `Cell.AdjacentMineCount` 改为 `init`（构造后不可变）
- `GameChangedEventArgs` 移至 `Models` 命名空间，`Game` 属性改为可空
- `IServiceCollectionExtensions.AddCore()` 完成所有 Core 层服务 DI 注册
- 项目结构: 删除 `Models/Args/` 文件夹，新增 `Services/` 文件夹

### Removed

- `GameStatusChangedEventArgs` / `GameTimerTickEventArgs` / `MineFieldGeneratedEventArgs` — 由 `INotifyPropertyChanged` 和返回值替代
- `HintStrategyType` / `IHintProvider` — 暂不实现
- `Constants.NeighborCount` — 由 `Position.DirectionOffsets` 替代

---

- **领域模型精化**: 重构和增强核心领域模型
  - `IGame` 接口: 用 `IGameBoardDictionary` 替代 `IGameBoard`，新增 `Rows`/`Columns` 只读属性
  - `IGameBoardDictionary` 接口: 棋盘格子字典，继承 `IReadOnlyDictionary<Position, Cell>`，提供 `OpenedCount`/`FlagCount`/`QuestionCount` 统计属性
  - `IGameBoardDictionaryFactory` 接口 (internal): 棋盘字典工厂
  - `IGameFactory` 接口 (internal): 游戏工厂，支持从 `GameConfig` 或 `GameSaveData` 创建游戏
  - `IMineField` 接口 (internal): 内部地雷场接口，提供地雷生成和查询方法
  - `IGameDataRepository` 接口: 新增存档数据管理（`HasGameSaveData`/`GetGameSaveDataAsync`/`SaveGameSaveDataAsync`）和结果记录 CRUD 方法
  - `Position` record struct: 新增 `DirectionOffsets` 只读属性和 `GetAdjacentPositions`/`GetAllPositions` 方法
  - `GameResult`/`GameSaveData` record: 重构自定义构造函数为静态工厂方法模式
  - `Cell` 类: `AdjacentMineCount` 属性改为只读
  - `Constants` 类: 移除未使用的 `NeighborCount` 常量
  - 移除 `HintStrategyType`/`IHintProvider`（暂不实现），移除 `IGameBoard`/`MineFieldGeneratedEventArgs`（由新接口替代）
- Core 项目添加 `Microsoft.Extensions.DependencyInjection.Abstractions` NuGet 包引用
- **领域模型设计**: 定义完整的扫雷游戏领域模型
  - 枚举: `CellType`（格子类型）、`GameDifficulty`（难度）、`GameStatus`（游戏状态）
  - 接口: `IGame`（游戏核心）、`IGameBoardDictionary`（棋盘格子字典）、`IGameManager`（游戏管理器）、`IGameResultRepository`（结果仓储）、`IGameTimer`（计时器）、`IMineField`（地雷场，internal）
  - 模型: `Cell`（格子，含 `INotifyPropertyChanged`）、`Position`（位置 record struct）、`GameConfig`（游戏配置 record）、`GameResult`（游戏结果 record）、`GameSaveData`（存档数据 record）
  - 事件参数: `GameChangedEventArgs`、`GameStatusChangedEventArgs`、`GameTimerTickEventArgs`
- 添加 `Constants` 常量类，包含 `MaxCompletion` 常量和内置难度对应的 `GameConfig` 预置实例
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
