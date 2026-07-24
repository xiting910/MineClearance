# Changelog

本文件记录了项目的所有重要变更。每个版本的变更都应在发布时记录在此文件中。

格式基于 [Keep a Changelog](https://keepachangelog.com/zh-CN/1.1.0/),
版本号遵循 [Semantic Versioning](https://semver.org/lang/zh-CN/).

---

## [Unreleased]

### Added

- **可解性检查器**: `ISolvabilityChecker` 接口和 `SolvabilityChecker` 实现, 仅限 Core 层内部使用, 用于检查地雷布局是否可解
- **地雷生成器完善**: `MineGenerator` 从桩实现完善为完整实现 — 通过 `ShuffleEngine<T>` 洗牌引擎随机生成地雷位置, 确保首次点击位置及其相邻位置不为地雷, 并调用可解性检查器保证布局可解 (最多重试 1000 次)
- **Position.FromIndex()**: 新增静态方法, 支持从一维索引转换为二维位置 (按行优先)
- **DI 注册**: `ISolvabilityChecker` → `SolvabilityChecker` 注册为瞬态服务
- **相邻格子操作**: `IGame.OpenAdjacentCells(Position)` (双击数字格自动翻开周围) 和 `IGame.FlagAdjacentCells(Position)` (右键数字格自动标旗周围) 方法
- **游戏结果属性**: `IGame.Result` 属性 (nullable), 游戏结束后可获取 `GameResult` 结果
- **游戏管理方法**: `IGameManager.RestartCurrentGame()` 和 `IGameManager.ExitWithoutSaving()` 方法
- **自动保存**: `GameManager` 通过监听 `IGame.PropertyChanged`, 在 `Result` 变化时自动调用 `IGameDataRepository.AddGameResultAsync` 持久化游戏结果
- **警告数字检测**: `Game.CheckAndUpdateWarningStates()` 方法, 当标记数超过相邻地雷数时, 数字格切换为警告状态 (`WarningNumber`)
- **统一异常消息常量**: `Constants.CustomDifficultyMissingInfoMessage` 和 `MineField.MineFieldNotGeneratedMessage`
- **`InternalsVisibleTo`**: Infrastructure 项目添加 `[InternalsVisibleTo("MineClearance.Infrastructure.Tests")]`
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

- **代码现代化**: Core 层所有文件使用 `using` 导入替代完全限定类型名 (如 `IGame` 替代 `Interfaces.IGame`, `GameConfig` 替代 `Models.Records.GameConfig`, `GameDifficulty` 替代 `Enums.GameDifficulty` 等)
- **IMineGenerator 返回类型**: `GenerateMines()` 返回类型从 `IReadOnlyCollection<Position>` 改为 `IEnumerable<Position>`
- **GameConfig 属性化**: `GetTotalCellsToOpen()` 方法改为 `TotalCellsToOpen` 只读属性
- **GameManager 重命名**: `OnGameChanged` 方法重命名为 `OnGamePropertyChanged`, 更准确地描述其功能
- **.editorconfig 增强**: 新增 `trim_trailing_whitespace = true`, 为 `*.{csproj,json,slnx}` 文件配置 `indent_size = 2` 和 `end_of_line = crlf`
- `Game.FloodOpen` 重构: 添加游戏结束防护, 使用 `cell.AdjacentMineCount` 替代 `_mineField.GetAdjacentMineCount`; 地雷命中逻辑从 `OpenCell` 移至此处
- `Game.IsGameCompleted()` 重构为 `CheckGameCompletion()` + `UpdateCompletion()` (返回 bool), 完成判定使用 `Constants.MaxCompletion` 倍率
- `Game` 重复状态断言提取为 `AssertGamePerformable()` 方法, `CancelPause` 断言收紧为仅允许 `Paused` 状态
- `GameConfig.ToString()` 移除 `"GameConfig: "` 前缀
- `GameResult` / `GameSaveData` 硬编码异常消息替换为 `Constants.CustomDifficultyMissingInfoMessage`
- `GameManager.Game` 属性 setter 增强, 正确订阅/取消订阅 `PropertyChanged` 事件并清理旧游戏
- `Position.DirectionOffsets` 和 `GetAllPositions` 使用简化语法 (`new(...)`)
- XML 文档注释增强 (`<see cref="Core"/>`)
- using 指令重新排序 (`System` 在 `Microsoft.Extensions` 之后)
- `GameResult`/`GameSaveData` 自定义构造器替换为静态工厂方法
- `Cell.AdjacentMineCount` 改为 `init`（构造后不可变）
- `GameChangedEventArgs` 移至 `Models` 命名空间，`Game` 属性改为可空
- `IServiceCollectionExtensions.AddCore()` 完成所有 Core 层服务 DI 注册
- 项目结构: 删除 `Models/Args/` 文件夹，新增 `Services/` 文件夹

### Fixed

- `Game.OpenAdjacentCells()`: `CheckGameCompletion()` 从循环内移至循环外 — 修复每次打开相邻格子都重复检查游戏完成状态的问题, 现在只在所有相邻格子打开后检查一次
- `GameBoardDictionary.OpenedCount` 不再将 `Mine` 类型格子计入已打开数量

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
