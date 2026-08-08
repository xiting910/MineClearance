# Changelog

本文件记录了项目的所有重要变更。每个版本的变更都应在发布时记录在此文件中。

格式基于 [Keep a Changelog](https://keepachangelog.com/zh-CN/1.1.0/),
版本号遵循 [Semantic Versioning](https://semver.org/lang/zh-CN/).

---

## [Unreleased]

---

## [1.0.0] - 2026-08-09

**首个正式发布版本**: 一个基于 Avalonia UI + Clean Architecture 的跨平台扫雷游戏, 提供经典扫雷玩法 (左键翻开/右键标旗/点击数字格展开/按住滑动连续操作), 游戏自动保存与继续, 历史记录统计与筛选, 主题切换与设置中心, 结构化日志与数据持久化.

下方为自项目初始化以来的完整变更记录.

### Added

- 项目初始化: 基于 Avalonia UI 的跨平台扫雷游戏, 采用 Clean Architecture, .NET 10.0
- Core 层: 领域模型 (Cell, Position, GameConfig, GameResult, GameSaveData), 枚举, 接口, 领域服务 (Game, MineField, MineGenerator, GameTimer, GameManager 等)
- Infrastructure 层: 基础设施项目骨架
- UI 层: Avalonia 桌面应用入口, 图标与清单资源
- 测试: Core / Infrastructure / UI 三层冒烟测试 (xUnit v3 + Moq)
- CI/CD: GitHub Actions 工作流 (CI, CodeQL, Dependency Review, Dependabot, Release 发布)
- 工程化: CPM 集中包管理, Directory.Build.props 全局构建属性, .editorconfig, 清理构建脚本
- Core 层: 棋盘尺寸上限校验 (高 30 × 宽 50), GameConfig 集中校验, 游戏结果/存档校验复用
- Core 层: 枚举 [Description] 特性与 GetDescription() 扩展 (C# 14 extension 块)
- Core 层: ILogger 结构化日志 (LoggerMessage 源代码生成), Game / GameManager 注入日志记录器
- UI 层: Avalonia 应用骨架 (App.axaml, ViewLocator, DI 容器初始化, InterFont)
- 工程化: 全局 AssemblyMetadata, CPM 补充 Configuration / Logging Abstractions 包
- Infrastructure 层: 游戏数据仓储实现 (GameDataRepository, 存档与历史记录 Json 持久化, BitArray / Position 自定义转换器, LoggerMessage 日志)
- Infrastructure 层: 文件日志记录器 (FileLoggerProvider, 日志级别可配置并持久化, 日志文件轮转)
- UI 层: 接入 Infrastructure 服务与配置加载, 应用数据目录初始化与日志轮转
- 工程化: CPM 补充 Configuration.Json 包
- UI 层: UI 模型 (ThemeMode / NavigationTarget / UIOptions), UIOptions 从 IConfiguration 读取初始值且 setter 变化自动保存 UISettings.json
- UI 层: 视图模型体系 (Shell 视图切换 / Main 主视图 / Settings 设置 / Toast 通知), Game / History 视图模型占位
- UI 层: 视图体系 (ShellWindow / ShellView 主窗口与根视图, MainView 主视图, SettingsWindow / SettingsView 设置窗口, ToastView 全局通知), GameView / HistoryView 占位
- UI 层: 主视图 (难度下拉框, 预设难度只读/自定义可编辑, 雷数上限随宽高联动, 开始新游戏 / 继续游戏 / 历史记录 / 设置导航)
- UI 层: 设置窗口 (主题即时切换并自动保存, Toast 显示时长, 日志级别, 打开日志文件夹, 关于信息与 GitHub 链接, 窗口单例)
- UI 层: 全局右下角 Toast 通知 (显示时长可配置, 新提示取代旧提示)
- UI 层: 浅色/深色主题资源 (ThemeDictionaries 颜色资源) 与枚举描述转换器, 启动时按配置应用主题
- UI 层: Program.cs 注册 UI 服务与全部 ViewModel (Shell / Main / Game / History / Toast 为单例, Settings 为 Transient)
- 工程化: CPM 补充 Avalonia.Controls.DataGrid 包
- 文档: TODO.MD UI 层开发计划 (分阶段任务清单)
- UI 层: 游戏视图完整实现 (固定 1500 格 CellViewModel 池 + Canvas 绝对定位渲染, 首击前使用占位格子, 重开/切换视图零控件重建)
- UI 层: CellViewModel (格子显示文本与经典数字配色, 踩中地雷深红突出)
- UI 层: 游戏指针交互 (左键开格/点击数字格展开周围, 右键三态循环/数字格一键插旗, 按住滑动连续操作)
- UI 层: 游戏信息栏 (状态/难度/剩余地雷/已打开/完成度/时间/种子) 与暂停覆盖层, 游戏结束通过 Toast 提示结果
- Core 层: 游戏失败时揭示全部地雷, 胜利时自动将未开地雷格标记为旗
- UI 层: 操作提示按钮 (Toast 展示首次点击机制与鼠标操作说明)
- UI 层: Toast 剩余时间进度条 (ScaleTransform 补间平滑缩短) 与鼠标悬停暂停倒计时
- UI 层: ShellWindow 关闭前自动保存进行中的游戏 (取消关闭 → SaveAndExitAsync → 再次关闭, 避免进程退出截断文件写入)
- UI 层: 历史记录视图完整实现 (6 组难度范围统计汇总 DataGrid, 列头点击排序且"全部"行固定置顶, 缺项数值映射排最后; 日期范围 DatePicker / 难度 CheckBox 多选 / 结果 ComboBox 筛选, 不选任何难度项表示全部)
- UI 层: 历史记录操作 (详细记录 DataGrid 内置排序与多选, 删除选中不做确认, 清空历史 3 秒二次确认按钮, 清除筛选一键恢复全部)
- UI 层: UI 模型 (GameResultRow 显示文本与内置/自定义棋盘尺寸, StatsRow 统计行, DifficultyFilterOption 难度多选选项, ResultFilterOption 结果选项, SortKeys 统计排序键常量)
- UI 层: 主视图退出按钮 (ExitCommand → 关闭主窗口, 进行中的游戏由窗口关闭事件自动保存)
- UI 层: 历史视图启动预热常驻布局 (ShellView 宿主层 Opacity + IsHitTestVisible 控制, 启动即预热 DataGrid, 切换到历史视图时延迟到空闲时刷新数据)
- Core 层: CellType 新增 ErrorFlag (错误插旗) / OpenedMine (被打开的地雷) 枚举
- Core 层: IGame 新增 IsPerformable / HasProgress 属性 (公开暴露可操作状态与是否有实际进度)
- UI 层: ShellWindow 按当前视图动态调整最小窗口尺寸 (主视图/历史视图固定常量, 游戏视图按棋盘尺寸) 并钳制窗口位置到工作区 (参照旧项目 WM_MOVING)
- UI 层: Toast 背景与进度条颜色主题资源化 (浅色/深色各一套)
- UI 层: SettingsWindow 补充最小尺寸约束

### Changed

- Infrastructure 层: 移除 IFileLoggerOptions 接口, FileLoggerOptions 改为 public 具体类并移至项目根目录, DI 注册直接使用具体类
- UI 层: 设置窗口打开日志文件夹 / GitHub 链接失败时通过 Toast 提示错误信息, 设置项控件改为拉伸布局并补充操作标签行与按钮 ToolTip
- UI 层: 日志轮转改为仅最新日志文件存在且非空时执行
- Core 层: IGameDataRepository 异步方法改为同步属性 (SaveData, GameResults), 移除 HasGameSaveData 与 GetGameSaveDataAsync / GetGameResultsAsync
- Core 层: IGameManager.RestoreFromSaveDataAsync 改为同步方法 RestoreFromSaveData
- Core 层: IGameManager 移除 GameChanged 事件, 改为实现 INotifyPropertyChanging/INotifyPropertyChanged
- Core 层: IGameDataRepository 新增 DeleteGameSaveDataAsync, SaveGameSaveDataAsync 不再接受 null 存档; 开始新游戏与游戏结束时自动清空存档
- Core 层: IGameTimer.SetInitialTime 改为 Initial (记录开始时间与已用时间), 支持存档恢复后继续计时
- UI 层: 视图切换由 ContentControl 重建改为三视图常驻 Panel 宿主层 IsVisible 切换
- UI 层: 主视图清空存档逻辑移入 GameManager.StartNewGame; Toast 默认时长 3→5 秒且上限 10→20 秒, 新增 GreenLabelBrush 并重命名 DifficultyLabelBrush 为 YellowLabelBrush
- UI 层: 游戏视图改为启动预热常驻布局 (ShellView 宿主层由 IsVisible 改为 Opacity + IsHitTestVisible 控制游戏视图, 启动即初始化最大棋盘尺寸并实例化全部格子控件)
- UI 层: Toast 倒计时由 Task.Delay + CancellationTokenSource 改为 DispatcherTimer 驱动 (按实际经过时间精确扣减, 支撑进度条与悬停暂停)
- UI 层: CellViewModel / GameViewModel / ToastViewModel 移除 IDisposable (单例常驻无需手动释放)
- UI 层: UpdateBoard / MarkHitMine 改用 Position.ToIndex 直接索引固定格子池, UpdateCell 相同引用时跳过重复订阅与刷新
- UI 层: Program.cs 服务容器改为 using 声明并设置 ShutdownMode.OnMainWindowClose (主窗口关闭即退出应用), 设置窗口最小化后重新打开时恢复 Normal 再激活
- Infrastructure 层: FileLoggerProvider 写入器启用 AutoFlush (日志即时落盘, 防止异常退出丢失日志)
- Core 层: 游戏失败揭示逻辑增强 (错误插旗以 ErrorFlag 浅红显示, 问号格按是否为雷还原为地雷或未打开, 踩中的地雷以 OpenedMine 深红显示)
- Core 层: 胜利判定时问号格的未开地雷同样自动标旗; AssertGamePerformable 私有方法改为公开 IsPerformable 属性, 标记类操作断言收紧为仅 InProgress
- UI 层: CellViewModel 移除 _isHitMine / SetHitMine, 踩中地雷改由 OpenedMine 枚举驱动; GameViewModel / ShellWindow 关闭逻辑改用 IsPerformable / HasProgress
- UI 层: ShellWindow 移除固定初始宽高, 启动时按主视图最小尺寸初始化

### Fixed

- Core 层: 游戏实例释放移入 Game 属性 setter, 修复游戏切换 (开始新游戏/恢复存档/退出) 时先 Dispose 旧实例再赋值新实例抛 ObjectDisposedException 的问题
- UI 层: 日志轮转提前捕获最新日志文件名, 修复 MoveTo 后 FileInfo.Name 变化导致旧日志误入清理范围的问题

[Unreleased]: https://github.com/xiting910/MineClearance/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/xiting910/MineClearance/releases/tag/v1.0.0
