# Changelog

本文件记录了项目的所有重要变更。每个版本的变更都应在发布时记录在此文件中。

格式基于 [Keep a Changelog](https://keepachangelog.com/zh-CN/1.1.0/),
版本号遵循 [Semantic Versioning](https://semver.org/lang/zh-CN/).

---

## [Unreleased]

**更新状态机修复**: 修复检查更新后状态死锁 (已是最新/需要更新等终态无法再次发起检查, 发布新版本后同运行期内检测不到), 新增检查失败状态显式反馈, 下载取消状态由下载续体按真实结果收尾 (修复取消瞬间下载完成时状态被覆盖导致引导更新不执行), 历史记录清空后刷新回归 UI 线程.

### Fixed

- Infrastructure 层: 检查更新状态机修复 (检查入口放宽为仅拦截检查中/下载中, 已是最新/需要更新/下载失败/检查失败等状态均可重新发起检查; 取消检查恢复之前状态; DownloadCompleted 状态下检查且本地更新包完整时保持下载完成)
- Infrastructure 层: 下载取消竞态修复 (CancelDownload 仅发送取消请求, 状态由下载续体按真实结果收尾, 修复取消瞬间下载完成时状态被覆盖为需要更新导致引导更新不执行的问题)
- UI 层: 历史记录清空后刷新回归 UI 线程 (移除 ClearGameResultsAsync 的 ConfigureAwait(false), 续体回到 UI 线程执行 Refresh 与 Toast, 避免跨线程更新界面)

### Changed

- Infrastructure 层: UpdateState 新增 CheckFailed 检查失败状态 (检查失败从隐式的空闲+异常改为显式状态, UI 手动检查失败提示基于该状态)
- Infrastructure 层: State 状态属性改为 Interlocked 原子读写 (原子读与交换+事件去重, 移除 CAS 门控与 volatile 后备字段; 检查与下载方法续体统一 ConfigureAwait(false), 服务不依赖调用方线程)
- Infrastructure 层: TryFindUpdateAsset 纯函数化 (out 参数返回下载地址与大小并附加 MaybeNullWhen 流分析注解, TargetName 改为静态只读字段)
- Infrastructure 层: LatestVersion / TotalBytes 移除属性变化通知 (UI 消费点均为事件驱动的同步读取, 减少无谓刷新)
- Infrastructure 层: 更新日志事件 ID 重排 (UpdatePackageAlreadyComplete 由 9 移至 3, 与事件分组顺序对应)
- UI 层: UpdateViewModel 状态反馈重构 (状态转换反馈与进度刷新分离为 HandleStateTransition / RefreshFromDownloadProgress, 事件按属性名路由; 检查反馈统一由检查方法按手动/自动区分, 下载失败按悬浮球开关分流提示)

---

## [1.1.3] - 2026-08-11

**下载抽屉与窗口交互修复**: 悬浮球被禁用时下载抽屉仅在状态转换 (关闭开关/下载开始/下载失败) 时弹出一次, 进度更新不再重复弹出, 用户可正常关闭; 常驻隐藏视图 (游戏/历史) 宿主 Panel 新增 IsEnabled 可见性绑定, 隐藏时禁用控件, 防止 Tab 焦点导航错乱与空格误触发隐藏视图控件; 同时禁止空格/Tab/回车作为索引热键; 窗口位置钳制参数防护, 修复最大化时布局中断卡死.

### Fixed

- UI 层: 下载抽屉弹出策略修复 (悬浮球被禁用时, 仅在关闭悬浮球开关/下载开始/下载失败时自动弹出抽屉一次, 高频进度更新不再重复弹出, 修复抽屉无法正常关闭的问题)
- UI 层: ShellWindow 位置钳制修复 (移除 WindowState 最大化跳过检查, Clamp 上界用 Math.Max 钳制到工作区边界, 修复最大化瞬间窗口尺寸先变全屏而状态未同步时 Math.Clamp 因上界小于下界抛 ArgumentException, 布局中断导致视图不缩放与窗口卡死)
- UI 层: ShellView 游戏/历史视图宿主 Panel 新增 IsEnabled 可见性绑定 (隐藏时禁用控件, 防止 Tab 焦点导航进入隐藏视图与空格误触发隐藏视图控件, 不影响常驻布局预热)
- UI 层: KeyExtensions 将 Enter/Return/Space/Tab 加入无效热键集合 (避免索引热键与按钮激活/焦点导航等内部按键行为冲突)
- UI 层: 设置抽屉两处 ToolTip 文案精简
- 工程化: TagPush.bat 版本号解析改为整行提取后字符串替换, 兼容非标准格式

### Changed

- UI 层: 发现新版本提示点击下载改为直接调用下载服务, 移除 StartDownloadAsync 中间封装 (回调触发时状态恒为需要更新, 行为等价)

---

## [1.1.2] - 2026-08-11

**格子索引显示与复制**: 新增格子索引功能, 等待开始时按住可配置热键显示全部格子索引 (松开隐藏), 首次点击格子可自动复制索引到剪贴板并提示, 设置抽屉新增热键录制 (点击按钮后按键, Esc 取消, Backspace/Delete 清除, 系统保留键禁用).

### Added

- UI 层: KeyExtensions 按键有效性校验扩展 (IsValidHotKey, 排除功能键/Windows 键/状态锁定键/系统拦截键/IME 内部键)
- UI 层: 格子索引显示功能 (UIOptions 新增 ShowIndexHotKey 热键配置, ShellWindow 等待开始时按住热键显示全部格子索引/松开隐藏, 游戏开始后强制隐藏, 格子模板新增不拦截指针的索引叠加层)
- UI 层: 首点复制索引功能 (UIOptions 新增 CopyIndexOnFirstClick 开关默认关闭, 首次点击格子时写入系统剪贴板并 Toast 提示结果)
- UI 层: 设置抽屉新增首点复制索引开关与显示索引热键录制按钮 (录制状态按钮文本变为 >键名<, 无效按键 Toast 提示)
- 工程化: TagPush.bat 发布脚本 (校验 Git 仓库/工作区干净/版本号格式/tag 与 CHANGELOG 条目, 创建 tag 并推送触发 Release 发布)

### Changed

- UI 层: UIOptions 由主构造函数改为普通构造函数统一读取配置, 新增配置项持久化
- UI 层: CellViewModel 构造函数参数顺序调整 (cell 在前), GameViewModel 棋盘订阅改为订阅字段管理 (替换棋盘时先退订旧棋盘)
- UI 层: Cell / Shell / Main / History / Settings / Update 视图模型成员顺序按 VM 组织规则重排 (字段 → 属性 → 事件 → 构造函数 → 方法)

---

## [1.1.1] - 2026-08-10

**首次启动提示与更新维护**: 新增首次启动欢迎提示 (作者 / 操作指引 / 自动更新说明, 展示后自动关闭), 设置抽屉新增清除更新缓存按钮, 并优化 ToolTip 显示细节.

### Added

- UI 层: 首次启动欢迎提示 (UIOptions 新增 ShowFirstLaunchTip 配置默认开启, 启动时 Toast 展示作者 / Esc 设置抽屉 / 种子雷区 / 自动更新 / 数据目录指引, 展示后自动关闭配置并保存)
- UI 层: 设置抽屉更新分组新增清除更新缓存按钮 (删除更新数据目录, 成功/失败 Toast 反馈, ToolTip 提醒更新过程中请勿清理)
- UI 层: 全局 ToolTip 样式限制最大宽度 500, 日志文件夹按钮 ToolTip 直接显示日志文件夹实际路径

### Changed

- UI 层: UIOptions 各属性补充 XML 注释 (替换 inheritdoc), UpdateViewModel 字节单位常量与基数移至类顶部
- UI 层: 设置项 ToolTip 文案精简 (移除"修改后立即生效并自动保存"赘述), Toast 文本最大宽度 400 提升至 500
- 工程化: RePublish.bat 脚本标题与提示文案更新为 MineClearance 清理和发布脚本

---

## [1.1.0] - 2026-08-10

**自动更新版本**: 引入应用自动更新系统 (启动后台检查 GitHub 新版本, 断点续传下载与更新包完整性校验, 下载进度悬浮球与详情抽屉, 退出后引导更新并在失败时自动回滚), 设置窗口改为壳视图内动画抽屉, Toast 重构为多条目堆叠提示, 历史记录支持双击复制种子, 窗口尺寸变化时钳制抽屉宽度.

下方为自 1.0.0 以来的完整变更记录.

### Added

- Infrastructure 层: 应用自动更新基础设施 (IUpdateService 接口与 UpdateService 占位实现, UpdateInfo 更新信息记录, Downloader 下载工具包)
- Infrastructure 层: BootstrapUpdateHelper 引导更新辅助 (启动参数请求引导更新, 等待同名进程退出, 备份原始目录, 解压更新包并失败自动恢复备份, 成功后重启更新后的程序, 全程更新日志与 UpdateInfo 结果文件)
- Infrastructure 层: 更新相关常量 (Update 数据目录 / 备份目录 / 引导副本目录 / 更新包与新版本号文件 / 更新信息与日志文件)
- UI 层: AppMetadata 应用元数据封装 (基于 AssemblyMetadata 按键读取)
- UI 层: Toast 提示支持点击回调 (点击立即关闭并执行回调)
- UI 层: 历史记录详细表格行头显示当前显示顺序序号, 双击行复制该局种子到剪贴板并 Toast 提示
- Infrastructure 层: UpdateState 更新状态枚举 (空闲/检查中/已是最新/需要更新/下载中/下载完成/下载失败), UpdateService 状态机实现 (INotifyPropertyChanged + volatile/Interlocked 状态守卫, CheckNewestAsync / DownloadAsync / CancelDownload 完整实现, PerformBootstrapUpdateIfNecessary 与 GetLastUpdateInfoAndCleanUp 转交引导辅助, UpdateService.Logging 新增强类型日志)
- Infrastructure 层: BootstrapUpdateHelper 引导更新准备与清理 (PrepareBootstrapUpdate 复制程序目录到引导副本目录并启动副本程序, GetLastUpdateInfoAndCleanUp 删除引导副本/读取更新信息, 更新成功后清理更新包/新版本号/日志/备份残留), 更新日志改为追加模式
- UI 层: App.ExitCts 程序退出取消令牌源 (desktop.Exit 时取消), 游戏保存/历史删除与清空/二次确认延迟接入取消令牌
- UI 层: 设置抽屉 (移除 SettingsWindow, 壳视图内左侧抽屉 + 半透明遮罩, 关闭按钮/遮罩点击/Esc 呼出与收起, 游戏视图内打开自动暂停游戏, 关闭时恢复)
- UI 层: ShellWindow 关闭放行时执行引导更新 (PerformBootstrapUpdateIfNecessary), Esc 键切换设置抽屉
- Infrastructure 层: UpdateService 检查更新完整实现 (GitHub API 查询最新 release 并解析, 版本比较, 按平台匹配更新包资产, 已是最新/发现新版本状态流转)
- Infrastructure 层: UpdateService 下载完整实现 (Downloader 并发分块与断点续传, 下载进度/速度属性通知, 取消保留断点供续传, 失败可重试, 完整更新包校验后跳过)
- Infrastructure 层: UpdateService.Private 私有成员拆分 (平台更新包名/HttpClient/资产查找/版本校验辅助方法), UpdateService.Logging 新增强类型日志, IUpdateService 继承 IDisposable, 新增下载临时文件后缀常量
- UI 层: ToastItem 单条提示模型 (剩余时间进度/入场动画/悬停暂停/点击回调), ToastViewModel 重构为多条目集合 (满员顶掉最早, 每条独立倒计时, 集合为空停止计时器)
- UI 层: Toast 同时显示最大条数设置 (UIOptions MaxToastCount 自动保存, 设置抽屉 1-5 配置项, 默认 2), 新增 Toast 刷新间隔/入场偏移常量
- UI 层: UpdateViewModel 更新视图模型 (启动更新流程消费上次更新信息并后台检查, 手动检查更新各状态 Toast 反馈, 下载悬浮球与详情抽屉状态驱动, 高频进度事件合并刷新)
- UI 层: DownloadBallView 下载进度悬浮球 (左下角显示, 实心填充随进度增长, 点击呼出或关闭下载抽屉, 配置可关闭且关闭时下载中自动弹出抽屉兜底)
- UI 层: DownloadDrawerView 下载详情抽屉 (版本/进度条/已下载/速度/状态文本, 取消按钮, 失败异常信息区, 右边界可拖动调整宽度)
- UI 层: 下载悬浮球显示开关 (UIOptions ShowDownloadBall 自动保存默认开启, 设置抽屉配置项) 与设置抽屉手动检查更新按钮
- UI 层: 设置抽屉与下载抽屉滑入滑出淡入淡出动画 (共用遮布点击关闭当前抽屉, 关闭动画版本号防过期任务误关, 游戏暂停计数机制支持双抽屉叠加恢复)

### Changed

- UI 层: ShellWindow 首次打开时启动更新流程, Esc 键优先关闭下载抽屉, Toast 增加手型光标
- UI 层: 常量抽取 (MaxRatio/PercentBase 统一比例与百分比转换, 新增 DrawerWidth/DownloadBallSize/抽屉动画时长, ToastItem MaxProgress/MaxOpacity 移除)
- UI 层: GameViewModel.PauseIfPerformable 改为 void, 抽屉暂停/恢复游戏的计数逻辑移入 ShellViewModel
- Infrastructure 层: 下载完成后校验更新包完整性 (文件大小与服务器资产一致且版本标识匹配), 移除下载配置 HttpClientTimeout
- Core 层: 完成度文档与字面量 0.0 统一为 0

- Infrastructure 层: FileLoggerOptions 移至 Models 目录并调整命名空间为 MineClearance.Infrastructure.Models
- Infrastructure 层: 常量重命名 (DataDirectory / LogDirectory / SettingsDirectory → 加 Name 后缀, SettingFileSuffix → JsonFileSuffix), 日志轮转路径比较器提取为 Constants.PathComparer
- UI 层: Program.Main 入口检查引导更新请求并转交 BootstrapUpdateHelper, SettingsViewModel 改用 AppMetadata 读取关于信息
- Core 层: IGameDataRepository 全部方法与 IGameManager.SaveAndExitAsync 增加 CancellationToken 参数 (默认值 default), GameDataRepository 透传至 JsonSerializer 序列化
- UI 层: ShellWindow 移至根命名空间 MineClearance.UI, Program.cs 移除 ShutdownMode.OnMainWindowClose (设置窗口删除后主窗口关闭即退出应用)
- UI 层: NavigationTarget.SettingsWindow 重命名为 SettingsDrawer, 设置打开逻辑由壳视图窗口单例改为 ShellViewModel 抽屉状态 (Settings / IsSettingsOpen)
- 工程化: Directory.Build.props 移除显式 AssemblyVersion / FileVersion (跟随 Version), ReBuild.bat 重命名为 RePublish.bat
- UI 层: ToastView 由单条提示改为多条目堆叠展示 (ItemsControl, 新提示淡入并从下方滑入)

### Fixed

- UI 层: 窗口尺寸变化时钳制设置/下载抽屉宽度, 防止抽屉超出窗口范围 (窗口变窄时压缩抽屉, 变宽时保持用户拖动设定的宽度)

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

[Unreleased]: https://github.com/xiting910/MineClearance/compare/v1.1.1...HEAD
[1.1.1]: https://github.com/xiting910/MineClearance/releases/tag/v1.1.1
[1.1.0]: https://github.com/xiting910/MineClearance/releases/tag/v1.1.0
[1.0.0]: https://github.com/xiting910/MineClearance/releases/tag/v1.0.0
