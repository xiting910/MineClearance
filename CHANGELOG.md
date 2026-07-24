# Changelog

本文件记录了项目的所有重要变更。每个版本的变更都应在发布时记录在此文件中。

格式基于 [Keep a Changelog](https://keepachangelog.com/zh-CN/1.1.0/),
版本号遵循 [Semantic Versioning](https://semver.org/lang/zh-CN/).

---

## [Unreleased]

### Added

- **`Directory.Build.props`**: 全局构建属性文件 — 统一 `TargetFramework` (`net10.0`)、`Nullable` (`enable`)、`ManagePackageVersionsCentrally` (启用 CPM); 通过 `MSBuildProjectName` 条件区分 src 项目 (生成 XML 文档) 和测试项目 (隐式 using + 不可打包 + 测试包引用 + Xunit 全局 using)
- **`Directory.Packages.props`**: NuGet CPM 集中包版本管理 — 16 个 NuGet 包的版本号统一在此文件管理, Dependabot 更新时仅修改此处
- **`.gitattributes`**: 规范化 Git 行尾处理 — 默认 LF 换行符, 为 C#/Markdown 等文件配置 diff 策略, 标记二进制文件类型
- **`ReBuild.bat`**: Windows 清理构建脚本 — 递归删除所有 `bin`/`obj` 文件夹后执行 `dotnet build`, 带进度提示和错误处理
- **领域服务实现**: Core 层完成所有接口实现
  - `Game` — 游戏核心逻辑（首次点击地雷生成、泛洪打开、状态管理、完成度计算、存档导出）
  - `GameBoardDictionary` — 基于字典的棋盘, `INotifyPropertyChanged` 驱动统计属性
  - `GameBoardDictionaryFactory` — 棋盘工厂
  - `GameFactory` — 基于 `IServiceScopeFactory` 的游戏对象图组装（每局一个 DI Scope）
  - `GameManager` — 游戏生命周期管理, 监听 `IGame.PropertyChanged` 在 `Result` 变化时自动持久化
  - `GameTimer` — 基于 `Stopwatch` 的计时器, 通过 `Refresh()` 由外部 UI 驱动
  - `MineField` — 地雷场, 支持随机生成和位图恢复
  - `MineGenerator` — 通过 `ShuffleEngine<T>` 洗牌引擎随机生成地雷位置, 确保首次点击及其相邻位置不为地雷, 调用可解性检查器保证布局可解（最多重试 1000 次）
  - `SolvabilityChecker` — 可解性检查器, 仅限 Core 层内部使用
- **游戏功能**:
  - `IGame.OpenAdjacentCells(Position)` — 双击数字格自动翻开周围
  - `IGame.FlagAdjacentCells(Position)` — 右键数字格自动标旗周围
  - `IGame.Result` 属性 (nullable) — 游戏结束后可获取 `GameResult`
  - `Game.CheckAndUpdateWarningStates()` — 标记数超过相邻地雷数时数字格切换为警告状态 (`WarningNumber`)
  - `IGameManager.RestartCurrentGame()` 和 `IGameManager.ExitWithoutSaving()` 方法
- **工具方法与常量**: `Position.DirectionOffsets`（8 方向偏移）、`GetAdjacentPositions()`、`GetAllPositions()`、`ToIndex()`、`FromIndex()`（一维索引转二维位置）; `Constants.MineValue = -1`
- **接口新增成员**: `IGame` 新增 `Config` / `GetSaveData()`; `IGameBoardDictionary` 新增 `GetCellStates()`; `IGameTimer` 新增 `FirstStartTime` / `Refresh()` / `SetInitialTime()`; `IGameDataRepository` 新增存档管理 (`HasGameSaveData` / `GetGameSaveDataAsync` / `SaveGameSaveDataAsync`) 和结果记录 CRUD
- **基础设施**: `ISolvabilityChecker` DI 注册为瞬态服务; Infrastructure 项目添加 `[InternalsVisibleTo("MineClearance.Infrastructure.Tests")]`; 统一异常消息常量
- **项目初始化**:
  - 搭建 Clean Architecture 三层架构（Core / Infrastructure / UI）, 使用 `.slnx` 解决方案格式
  - 目标框架 .NET 10.0, 全局启用可空引用类型与 XML 文档生成
  - 集成 Avalonia UI 跨平台桌面框架 + CommunityToolkit.Mvvm
  - 添加 xUnit + Moq 单元测试项目骨架
  - 配置 GitHub Actions CI/CD（CI 构建测试、CodeQL 安全分析、Release 发布）和 Dependabot 自动依赖更新
  - 添加 `.editorconfig` 代码风格配置、Issue / PR 模板、MIT 许可证、README.md

### Changed

- **NuGet CPM 迁移**: 所有 csproj 移除重复的 PropertyGroup 和 PackageReference `Version` 属性 — 测试项目精简为仅保留 `ProjectReference`; 包版本号全部集中到 `Directory.Packages.props`
- **`.slnx` 更新**: 新增 `.gitattributes`、`CHANGELOG.md`、`Directory.Build.props`、`Directory.Packages.props`、`README.md` 到解决方案文件, 方便在 IDE 中直接编辑
- **CI 工作流更新**: `ci` / `codeql-analysis` / `dependency-submission` / `release-publish` 四个工作流的 `cache-dependency-path` 新增 `**/*.props`, 确保 props 文件变更时正确失效 NuGet 缓存
- **`.editorconfig` 重构**: 全局默认设置提前到根级别, 新增 `*.axaml` (Avalonia UI 标记) 和 `*.bat` (Windows 批处理) 文件规则, 将各文件类型分离为独立配置节, 移除重复设置
- **`.gitignore` 重构**: 按类别组织忽略规则 (构建产物 / IDE & 编辑器 / .NET / 操作系统), 新增 `**/.idea/`、`*.swp`/`*.swo`/`*~`、`*.nupkg`、`TestResults/`、`.DS_Store`、`Thumbs.db` 等, 移除 `*.bat` 忽略规则
- **代码现代化**: Core 层所有文件使用 `using` 导入替代完全限定类型名; XML 文档增强; `using` 指令排序; `Position` 使用简化语法 (`new(...)`)
- **接口签名重构**:
  - `IGame`: 实现 `INotifyPropertyChanged` + `IDisposable`, 移除 `StatusChanged` 事件 / `Minefield` 属性, `Board` 可空
  - `IGameBoardDictionary`: 实现 `INotifyPropertyChanged`, 移除 `Rows`/`Columns`/`SetAdjacentMineCounts`
  - `IGameTimer`: 实现 `INotifyPropertyChanged`, 移除 `Tick` 事件 / `ReStart()` / `IDisposable`
  - `IGameManager`: 方法异步化 (`RestoreFromSaveDataAsync` / `SaveAndExitAsync`), `StartNewGame` 拆分重载
  - `IMineField`: 改为 `internal`, `Generate()` 返回 `int[]` 替代事件
  - `IMineGenerator.GenerateMines()`: 返回类型改为 `IEnumerable<Position>`, 新增 `seed` 参数
  - `IGameResultRepository` → 重命名为 `IGameDataRepository`
- **Game 内部重构**: `FloodOpen` 添加游戏结束防护, 地雷命中逻辑移入; `IsGameCompleted()` 重构为 `CheckGameCompletion()` + `UpdateCompletion()`, 完成判定使用 `Constants.MaxCompletion` 倍率; 重复状态断言提取为 `AssertGamePerformable()`; `CancelPause` 断言收紧为仅允许 `Paused` 状态
- **模型精化**: `Cell.AdjacentMineCount` 改为 `init`（构造后不可变）; `GameResult` / `GameSaveData` 自定义构造器替换为静态工厂方法; `GameChangedEventArgs` 移至 `Models` 命名空间, `Game` 属性改为可空; `GameConfig.ToString()` 移除前缀; `GameConfig.GetTotalCellsToOpen()` 改为 `TotalCellsToOpen` 只读属性; `GameResult` / `GameSaveData` 硬编码异常消息替换为常量; `GameManager.Game` 属性 setter 增强, 正确订阅/取消订阅 `PropertyChanged` 事件
- **项目结构**: `IServiceCollectionExtensions.AddCore()` 完成所有 Core 层服务 DI 注册; 删除 `Models/Args/` 文件夹, 新增 `Services/` 文件夹

### Fixed

- `Game.OpenAdjacentCells()`: `CheckGameCompletion()` 从循环内移至循环外 — 修复每次打开相邻格子都重复检查游戏完成状态的问题, 现仅在全部打开后检查一次
- `GameBoardDictionary.OpenedCount` 不再将 `Mine` 类型格子计入已打开数量

### Removed

- `GameStatusChangedEventArgs` / `GameTimerTickEventArgs` / `MineFieldGeneratedEventArgs` — 由 `INotifyPropertyChanged` 和返回值替代
- `HintStrategyType` / `IHintProvider` — 暂不实现
- `Constants.NeighborCount` — 由 `Position.DirectionOffsets` 替代

[Unreleased]: https://github.com/xiting910/MineClearance/commits/main
