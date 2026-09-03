# Changelog

本文件记录了项目的所有重要变更。每个版本的变更都应在发布时记录在此文件中。

格式基于 [Keep a Changelog](https://keepachangelog.com/zh-CN/1.1.0/),
版本号遵循 [Semantic Versioning](https://semver.org/lang/zh-CN/).

---

## [Unreleased]

---

## [1.6.2] - 2026-09-03

**日志与异常处理修复, Toast 重构与工作流简化**: 修复未处理异常发生在应用初始化创建目录之前时兜底日志静默丢失的问题 (HandleException 写入前创建应用数据根目录); 根治日志轮转空文件误判 (根因为写入端使用带 BOM 的 Encoding.UTF8, 全程无日志写入时退出 flush 仍会落盘 3 字节 UTF-8 BOM 占位文件, 产生无实际内容却有文件大小的日志文件; 写入端改用 new UTF8Encoding(false) 无 BOM 编码, 空日志文件恢复 0 字节, 轮转判断由 1.6.1 的读取首字节恢复为 Length > 0); Toast 倒计时计时改为 Stopwatch 高精度测量并简化 HasItems 通知; CodeQL 工作流 autobuild 配置简化.

### Changed

- 工程化: CodeQL 工作流简化 (autobuild 模式由单独步骤改为初始化步骤内 build-mode: autobuild 配置, 移除冗余的 Autobuild 步骤)
- UI 层: Toast 倒计时计时重构 (计时由 DateTime.Now 差值改为 Stopwatch 高精度测量, 不受系统时钟调整影响; ToastItem.Tick 悬停暂停期间不再更新进度条; InvokeClick 改为先移除条目再执行点击回调; Toast 显示日志级别由 Information 降为 Debug)
- UI 层: Toast HasItems 属性改为可观察属性 (从计算属性改为 [ObservableProperty] 源生成, 集合变化时直接赋值触发通知, 简化绑定逻辑)

### Fixed

- Infrastructure 层: 未处理异常兜底日志丢失修复 (HandleException 写入前创建应用数据根目录, 修复异常发生在应用初始化创建目录之前时兜底日志静默丢失的问题)
- Infrastructure 层: 无日志运行时生成 BOM 占位文件修复 (写入端由带 BOM 的 Encoding.UTF8 改为 new UTF8Encoding(false) 无 BOM 编码, 从未写入日志时文件保持 0 字节, 不再残留 3 字节 UTF-8 BOM 造成无实际内容却有文件大小的日志文件)
- UI 层: 日志轮转空文件判断恢复为长度判断 (写入端不再产生 BOM 占位文件后, 空日志文件恒为 0 字节, 由 1.6.1 的读取首字节判断恢复为 Length > 0, 无实际内容的文件不再被轮转)

---

## [1.6.1] - 2026-08-31

**日志文件轮转空文件判断修复**: 修复日志文件轮转时误将无实际内容但文件大小为 1KB 的游戏日志轮转的问题 (原逻辑使用 FileInfo.Length > 0 判断文件是否为空, 改为实际读取文件首字节判断, 确保无内容的文件不被轮转).

### Fixed

- UI 层: 日志文件轮转空文件判断修复 (原逻辑使用 FileInfo.Length > 0 判断文件是否为空, 游戏日志无实际内容但文件大小为 1KB 时会被错误轮转; 改为实际读取文件首字节判断, 确保无内容的文件不被轮转)

---

## [1.6.0] - 2026-08-31

**棋盘配色主题化**: 棋盘格子颜色全面主题化 (CellViewModel 配色从硬编码画刷改为主题资源字典动态读取, App.axaml Light/Dark 主题字典新增 22 组格子色板资源), 深色主题格子色板独立适配 (未翻开/已翻开/数字/旗/警告/雷各有专属深色配色), 索引叠加层文字颜色主题化 (GameView 前景改用 DynamicResource CellIndexBrush), 主题切换时实时刷新棋盘配色 (GameViewModel 订阅 ActualThemeVariantChanged 统一遍历格子刷新).

### Changed

- 文档: README 全面重写
- 工程化: .gitattributes 精简 (移除标记文件与多数二进制规则, 保留 ico/png)
- 工程化: .gitignore 精简 (移除 IDE 与操作系统忽略规则, 构建产物规则不再递归)
- 工程化: 新增 .vscode/settings.json (dotnet.defaultSolution)
- UI 层: 棋盘格子颜色全面主题化 (CellViewModel 配色从硬编码画刷改为主题资源字典动态读取, App.axaml Light/Dark 主题字典新增 22 组格子色板资源)
- UI 层: 深色主题格子色板适配 (未翻开 #444A58, 已翻开/数字格与卡片背景 #232733 融入, 数字 1-8 换亮色系, 旗 #2F5D3A, 警告 #C05E22, 雷 #7A2E2E)
- UI 层: 索引叠加层文字颜色主题化 (GameView 前景由硬编码 #4B5563 改为 DynamicResource CellIndexBrush)
- UI 层: 主题切换实时刷新棋盘配色 (GameViewModel 订阅 ActualThemeVariantChanged, 统一遍历格子刷新)

---

## [1.5.4] - 2026-08-30

**历史记录统计修正与视图背景透明化**: 修复历史记录统计平均完成度仅计算失败局导致数据失真的问题 (改为计算全部局); 视图背景全面透明化 (历史记录表格列头、主视图参数卡片容器、游戏视图顶部信息栏), 消除白色色块与页面背景的割裂感; CI 工作流优化 (dotnet test 跳过冗余构建、Dependabot 合并简化) 与发布脚本加固 (构建测试前置验证); .editorconfig 全面重构 (补充详细中文注释、按功能分类整理、补充 tab_width 等缺失配置); .gitattributes 排除脚本文件的 linguist 语言检测.

### Fixed

- UI 层: 历史记录统计平均完成度修正 (从仅计算失败局完成度改为计算全部局完成度)

### Changed

- 工程化: .gitattributes 排除脚本文件类型检测 (bat/sh 文件添加 linguist-detectable=false, 避免 GitHub 统计语言占比时被脚本文件干扰)
- 工程化: .editorconfig 全面重构 (补充详细中文注释、按功能分类整理为十五个章节、补充 tab_width / 多行空行 / CA 诊断规则等缺失配置, 移除 VB.NET 独立配置段)
- 工程化: CI 工作流 `dotnet test` 添加 `--no-build` 跳过冗余构建步骤 (CI 已在前置步骤完成构建)
- 工程化: Dependabot 自动合并工作流移除分支更新步骤 (简化工作流, 依赖 GitHub 原生合并策略)
- 工程化: TagPush.bat 发布前新增构建与测试验证步骤 (创建 tag 前执行 `dotnet build` + `dotnet test`, 构建或测试失败时中止发布)
- UI 层: 视图背景透明化 (历史记录表格列头、主视图参数卡片容器、游戏视图顶部信息栏, 背景由 CardBackgroundBrush 白色改为 Transparent, 悬停/聚焦/禁用等状态同样透明, 消除白色色块与页面背景的割裂感)

---

## [1.5.3] - 2026-08-28

**构建属性完善**: Directory.Build.props 新增 PackageLicenseExpression (MIT) 与 Copyright 全局构建属性, 为 NuGet 包发布和程序集元数据提供标准化的许可证与版权信息.

### Changed

- 工程化: Directory.Build.props 新增 PackageLicenseExpression 与 Copyright 全局构建属性 (许可证表达式与版权声明)

---

## [1.5.2] - 2026-08-28

**应用图标现代化与窗口布局修复**: 应用图标从通用符号升级为地雷造型, 背景由纯色平涂改为靛蓝对角渐变 + 球面径向渐变 + 柔和投影, 整体风格更现代精致, 16/24px 小尺寸手工逐像素绘制保证清晰辨识; 修复窗口最大化时内容区与屏幕左边缘之间的缝隙问题; 移除窗口位置钳制逻辑, 窗口位置交由系统管理, 拖动行为与标准 Windows 窗口一致, 最小窗口尺寸仍按当前屏幕工作区钳制上限.

### Changed

- UI 层: 应用图标现代化 (替换 Assets/logo.ico, 地雷造型 + 靛蓝渐变背景 + 球面高光投影, 9 个尺寸 16~256px, 小尺寸手工像素画优化)

### Fixed

- UI 层: 窗口最大化时左侧缝隙修复 (移除最大化瞬间将窗口外框从系统位置 (-8,-8) 拉回工作区原点 (0,0) 的位置钳制, 系统最大化布局不再被破坏, 内容区可完整填满工作区)

### Removed

- UI 层: 窗口位置钳制移除 (删除 AdjustPositionToWorkingArea / OnPositionChanged 事件处理与 _isAdjustingPosition 递归防护, 以及 WindowClampRightMargin / WindowClampBottomMargin 钳制边距常量, 窗口位置交由系统管理; 手动拖动窗口允许超出屏幕, 最小尺寸仍按工作区钳制上限)

---

## [1.5.1] - 2026-08-25

**更新服务状态守卫原子化**: 新增 AtomicEnum 枚举原子操作封装结构体, UpdateService 检查/下载入口的状态守卫由"读取-判断-写入"改为自旋谓词条件交换原子置位, 消除并发调用时的竞态窗口; 下载入口根据更新包完整性原子置为下载中或下载完成状态.

### Added

- Infrastructure 层: 新增 AtomicEnum<TEnum> 原子枚举封装结构体 (Value 原子读取 / Set 原子写入返回是否变更 / SpinPredicateAndSet 自旋谓词条件交换, 固定值与工厂生成值两个重载)

### Changed

- Infrastructure 层: UpdateService 状态守卫原子化 (CheckNewestAsync / DownloadAsync 入口的读取-判断-写入改为 SpinPredicateAndSet 原子条件交换, 消除并发检查/下载请求通过守卫的竞态窗口; 下载入口按更新包完整性由工厂生成下载中或下载完成状态, 下载完成分支统一清空异常; State 属性读写改用 AtomicEnum 封装, 状态通知语义不变)

---

## [1.5.0] - 2026-08-24

**窗口失焦/最小化自动暂停游戏**: 游戏进行中窗口最小化或失去焦点时自动暂停游戏, 避免玩家离开时游戏继续计时.

### Added

- UI 层: 窗口最小化时自动暂停游戏 (OnPropertyChanged 监听 WindowState 变化, 最小化时调用 PauseIfPerformable)
- UI 层: 窗口失去焦点时自动暂停游戏 (OnDeactivated 事件处理, 失焦时调用 PauseIfPerformable)

### Changed

- Core 层: MineSolver 搜索逻辑重构 (TrySafeOpen 内部搜索逻辑提取为 SearchState 内部类, 封装赋值状态/约束计数/搜索节点计数等中间变量与回溯逻辑; CollectConstraints 收集数字格约束与前沿格, CollectFreeCells 收集自由格, BuildSolverIndex 构建变量索引与约束邻接结构; CanBeMine 提取为独立私有静态方法)
- UI 层: 退出请求处理简化 (移除 OnExitRequested 中间方法, ExitRequested 事件直接订阅 Close; ShellWindow 方法按功能重排)

---

## [1.4.0] - 2026-08-23

**失败时揭示必安全格**: 游戏失败揭示雷局时, 将可推定为必定安全的格子标记为绿色 ✓, 提示玩家本有确定动作; MineSolver.TrySafeOpen 重构为 bool 返回值 + out 参数, 存在必安全格时输出必安全格集合供失败揭示使用 (不挽救玩家猜测, 拒救行为不变).

### Changed

- Core 层: CellType 新增 GuaranteedSafe 必安全格枚举
- Core 层: IMineSolver.TrySafeOpen 重构 (返回 BitArray? 改为 bool + rearrangedMines/guaranteedSafePositions 两个 out 参数, 参数顺序 target 置前, 补充 [NotNullWhen] 流分析注解与参数文档)
- Core 层: MineSolver 存在必安全格时输出全部必安全格集合 (此前直接返回 null), 其余失败分支同步补齐 out 参数
- Core 层: Game 失败揭示适配 (Unopened/Question 的必安全格标记为 GuaranteedSafe, 失败分支重构为 switch 模式匹配)
- UI 层: CellViewModel 新增必安全格显示 (绿色背景 + ✓ 符号)
- 测试: GameTests 新增 CreateMineSolverMock 模拟 (按接口契约失败时输出非空安全格集合); MineSolverTests 适配新签名并新增必安全格集合内容断言

---

## [1.3.2] - 2026-08-23

**内置背景图片显示名改为序号**: 设置抽屉内置背景图片选项显示名由文件名改为序号 (内置背景图片 N), 不再直接展示图片文件名.

### Changed

- UI 层: 内置背景图片选项显示名由文件名改为序号 (SettingsViewModel 使用 Index() 生成"内置背景图片 N"显示名, FileName 保持不变)

---

## [1.3.1] - 2026-08-23

**内置背景图片与来源开关, Toast 显示结构化日志**: 新增 3 张内置背景图片打包为应用资源; 设置抽屉新增"使用自定义背景图片"开关, 勾选后从程序目录 Pictures 文件夹加载图片, 取消勾选恢复内置图片列表; 背景图片加载按来源区分 (内置走 avares 资源, 自定义走文件路径), 加载失败 Toast 提示; Toast 显示记录结构化日志 (ToastViewModel 注入 ILogger, Show 时经 LoggerMessage 源生成器记录 ToastShown 日志事件).

### Added

- UI 层: 内置背景图片 (Constants 新增 BuiltInBackgroundImageUriPrefix avares 资源 URI 前缀 / BuiltInBackgroundImageFileNames 内置图片文件名列表 / DefaultBackgroundImageFileName 默认图片, Assets/Backgrounds 新增 3 张内置图片资源)
- UI 层: 背景来源开关 (UIOptions 新增 UseCustomBackgroundImage 配置项, 设置抽屉新增"使用自定义背景图片"开关, 勾选后从自定义图片文件夹加载图片, 取消勾选恢复内置图片列表)
- 测试: UI 层单元测试适配 (SettingsViewModel 内置图片列表/来源开关切换与配置同步, UIOptions 来源开关默认值/解析/持久化, Constants 目录常量重命名)
- UI 层: Toast 显示日志 (ToastViewModel 构造函数新增 ILogger 注入, Show 方法经 LoggerMessage 源生成器记录 ToastShown 事件, 日志级别 Information)

### Changed

- UI 层: BackgroundImageOption FileName 改为非空 (不使用背景图片以空字符串表示), BackgroundImageChanged 事件新增是否使用自定义图片参数
- UI 层: 背景图片加载支持内置资源 (ShellViewModel 按来源使用 AssetLoader 加载 avares 资源或文件路径加载, 加载失败 Toast 提示)
- UI 层: BackgroundImageDirectory 重命名为 CustomBackgroundImageDirectory, 设置抽屉"创建并打开图片文件夹"按钮文案与 ToolTip 同步更新
- 测试: ToastViewModelTests 适配 ILogger 注入

---

## [1.3.0] - 2026-08-23

**自定义背景图片功能**: 新增背景图片支持 (将图片放入程序目录 Pictures 文件夹, 设置抽屉中可选择背景图片/拉伸方式/透明度, 壳视图底层显示); 背景透明度拖动实时预览, 配置保存节流防滑块抖动频繁写文件; 打开文件夹/链接逻辑重构为共用辅助方法.

### Added

- UI 层: 背景图片功能 (Constants 新增 BackgroundImageDirectory 程序目录 Pictures 文件夹; BackgroundImageOption 背景图片选项模型; UIOptions 新增背景图片文件名/拉伸方式/透明度配置, 透明度读取钳制到 0-1, 拉伸无效值回退 UniformToFill; ShellView 底层新增 Image 显示背景图片, Game/History/Main 视图背景改为透明露出背景; 设置抽屉新增背景图片选择/拉伸方式/透明度滑块/刷新背景图片/创建并打开图片文件夹按钮)
- UI 层: 背景透明度保存节流 (Constants 新增 OpacitySaveThrottleMilliseconds 节流延迟, 滑块停止变化 300ms 后才写入配置文件, 版本号丢弃过期保存任务)
- 测试: UI 层单元测试新增背景图片覆盖 (Constants 背景图片目录断言; SettingsViewModel 背景图片列表识别与排序/配置选中同步/修改触发事件/透明度节流保存; UIOptions 背景配置解析/钳制/持久化)

### Changed

- UI 层: 打开文件夹与链接共用 OpenPath 辅助方法 (日志文件夹/背景图片文件夹/GitHub 链接, 打开背景图片文件夹前自动创建目录)
- UI 层: 抽屉关闭版本号字段 volatile 修饰 (ShellViewModel/UpdateViewModel, 跨线程读写安全)
- UI 层: App.axaml 新增 InfoButton 主题色资源 (亮/暗主题各一组), 设置抽屉日志文件夹按钮改用该样式

---

## [1.2.3] - 2026-08-23

**全局异常处理与数据根目录解析修复, 代码风格优化**: 修复 UI 层全局异常处理静默忽略非 Exception 类型异常对象的问题 (新增 UnknownException 包装非 Exception 异常对象后统一处理); 修复应用数据根目录在目录缺失时解析为相对路径的问题 (GetFolderPath 启用 SpecialFolderOption.Create 自动创建目录并返回绝对路径); 代码风格优化与测试数据根目录环境变量常量重命名.

### Changed

- 代码风格优化 (BootstrapUpdateHelper 模式匹配语法, App.axaml.cs 缩进格式, KeyExtensions 集合初始化简化, Program.cs 注释精简)
- Infrastructure 层: AppDataRootDirectoryEnvironmentVariableName 内部常量重命名为 AppDataRootDirectoryEnvironmentVariable (测试数据根目录环境变量名, 测试引用同步更新)

### Fixed

- UI 层: Program.cs 未处理异常处理修复 (ExceptionObject 非 Exception 类型时静默忽略, 新增 UnknownException 包装非 Exception 异常对象后统一传递给 HandleException)
- Infrastructure 层: 应用数据根目录路径解析修复 (GetFolderPath 改用 SpecialFolderOption.Create, ApplicationData 目录不存在时自动创建并返回绝对路径, 避免返回空字符串导致 AppDataRootDirectory 退化为相对路径)

---

## [1.2.2] - 2026-08-20

**优化日志和 UI 显示**: GameDataRepository 游戏存档保存日志添加删除状态标识; UI 层操作提示帮助文本优化.

### Changed

- Infrastructure 层: GameDataRepository 游戏存档保存日志添加删除状态标识 (LogGameSaveDataSaved 新增 isDeleted 参数, 日志消息记录存档是否被删除)
- UI 层: 操作提示帮助文本优化 (右键操作限制说明合并至首次点击描述, 新增显示索引热键/首点复制索引/无猜挽救机制说明)

---

## [1.2.1] - 2026-08-20

**历史记录排序修复与 xunit.v3 4.0.0 适配**: 修复历史记录表格完成度列排序异常 (胜利时 Result.Completion 为 null 导致排序失效, 新增 CompletionForSort 属性胜利固定 1.0 排在失败前); 统计行排序改用 Difficulty 枚举值代替 DifficultyText 文本排序; 宽度/高度列补充 SortMemberPath 支持列头排序; 百分比常量统一迁入 Core 层; 适配 xunit.v3 4.0.0 过时 API, 修复 CI 构建错误; 单实例服务器客户端断开场景测试等待时长提升, 修复偶发超时.

### Changed

- Core 层: Constants 新增百分比公共常量 (PercentBase 百分比基数 / PercentSign 百分号符号 / FloatFormat 浮点两位小数格式), Infrastructure 层 PercentBase 迁移至 Core 层消除重复定义
- UI 层: 全部百分比显示改用 Core 常量格式化 (GameResultRow / GameViewModel / StatsRowBuilder / UpdateViewModel, 消除硬编码 $"{... * 100:0.##}%" 插值)
- UI 层: StatsRow 新增 GameDifficulty? Difficulty 属性, StatsRowBuilder.ToRow 参数由 string 改为 GameDifficulty?, 全部难度文本由枚举 Description 驱动
- UI 层: GameResultRow 新增 CompletionForSort 排序属性 (胜利固定 1.0, 失败取实际完成度)
- UI 层: GameResultRow Config 属性 switch 异常改为含参数名/值/消息的详细 ArgumentOutOfRangeException
- 测试: StatsRowBuilder 测试适配 ToRow 参数变更 (string → GameDifficulty?), 补充 Difficulty 断言; GameResultRow 测试新增 CompletionForSort 测试用例
- 测试: 程序集级并行配置迁移至 xunit.v3 4.0.0 新 API (CollectionBehaviorAttribute.DisableTestParallelization 在 4.0.0 过时, 改用 ParallelizationAttribute.Mode = ParallelMode.None 禁用并行, 修复 CI 构建错误 CS0619)
- 工程化: dotnet test 切换 Microsoft.Testing.Platform 新体验 (新增 global.json 的 test.runner 配置, 移除 TestingPlatformDotnetTestSupport 属性, CI 与发布工作流测试命令改用 --solution / --report-xunit-trx 等 MTP 原生参数, 修复 .NET 10 SDK 下 MTP 应用不再支持 VSTest target 导致的测试阶段失败)
- UI 层: Program.cs 引导更新检查与执行局部变量重命名 (originalDirectory/originalVersion → dir/version) 并单行化

### Fixed

- UI 层: 历史记录表格完成度列排序异常修复 (SortMemberPath 由 Result.Completion 改为 CompletionForSort, 胜利时 Completion 为 null 导致排序失效; CompletionForSort 胜利固定 1.0 排在失败前)
- UI 层: 历史记录统计行排序修复 (排序键由 DifficultyText 文本改为 Difficulty 枚举值, 文本排序不符合难度顺序预期)
- UI 层: 历史记录表格宽度/高度列排序修复 (补充 SortMemberPath 为 Config.BoardWidth / Config.BoardHeight, 此前列头点击错误排序)
- Infrastructure 层: 单实例服务器客户端在连接建立前断开时管道实例不可恢复修复 (客户端在服务器接受连接前断开会使 WaitForConnectionAsync 持续抛 IOException 且 Disconnect 无法复位, 服务器陷入死循环导致后续激活请求全部超时; 连接异常时释放并重建管道实例, 客户端断开后继续等待后续激活请求)
- 测试: 单实例服务器客户端断开后继续等待激活请求测试偶发超时修复 (等待服务器复位管道时长由 300ms 提升至 1000ms, 两处)

---

## [1.2.0] - 2026-08-16

**雷数数据源迁移与棋盘非空化重构, 无猜挽救功能**: 格子周围雷数数据源从 Cell 快照迁移到 MineField 单一查询 (Cell 删除 AdjacentMineCount, IMineField / IGame 新增 GetAdjacentMineCount); 游戏棋盘改为创建即生成 (IGame.Board 非空, 构造器注入棋盘, 首次点击仅生成雷位, 是否开始过改以计时器 FirstStartTime 判断); 移除生成阶段的可解性检查 (删除 SolvabilityChecker, MineGenerator 回归纯随机) 为运行时无猜方案铺路; 新增 MineSolver 地雷求解器, 玩家被迫猜测选到雷格时尝试重排雷位使该格安全翻开并继续游戏, 实现运行时无猜体验 (以已揭示数字格为约束、邻域未开格为边界变量的回溯搜索, 雷数守恒且目标格强制安全, 已揭示数字计数保持不变; 搜索节点上限 100 万防止极端局面卡顿, 确定性输出优先保持原雷位减少移动); 存在必安全格时玩家并非被迫猜测 (有确定安全动作, 失误不救), 打开必死格时无合法重排无法挽救, 两种情况均按原逻辑判负; UI 层棋盘订阅与格子池绑定重构, 首次启动提示文案同步说明种子复现需保证猜测点击位置一致, 单实例服务器测试消除连接竞速.

### Added

- Core 层: 地雷求解器 MineSolver 与 IMineSolver 接口 (TrySafeOpen 约束求解重排雷位, 已开数字格计数作为约束, 邻域格与自由格按目标距离排序作为变量; 目标远离数字区时与最近自由格交换雷位快速路径, 必死格预检逐变量剪枝, 搜索节点上限 100 万)
- Core 层: 无猜挽救集成 (Game.FloodOpen 踩雷时先尝试 MineSolver.TrySafeOpen, 成功则替换内部地雷场并继续泛洪展开, 失败才按原逻辑揭示雷局判负; 新增 LogMineFieldReplaced 日志事件, GameFactory 构造注入 IMineSolver, DI 注册 AddScoped)
- UI 层: 首次启动提示文案更新 (种子固定雷区说明补充进行猜测时的点击位置需一致, 因无猜挽救会重排雷位)
- 测试: Core 层单元测试新增 1 组 (MineSolverTests, 覆盖二选一僵局两侧挽救、必死格拒绝、自由格快速交换、必安全格拒救、错旗不当约束、无数字格快速路径、相同输入结果一致)

### Changed

- Core 层: 格子周围雷数数据源迁移 (Cell 删除 AdjacentMineCount 属性, 雷数统一由 MineField.GetAdjacentMineCount 按位置查询; IGame 新增 GetAdjacentMineCount 转发供 UI 显示; 泛洪判空、警告数字判定、数字格展开与一键插旗改由 MineField 查询, 消除棋盘快照与雷位不一致的风险)
- Core 层: 游戏棋盘非空化 (IGame.Board 改为非空, 游戏创建时即生成棋盘, 首次点击仅生成雷位; HasProgress / CancelPause / GetSaveData 改以 Timer.FirstStartTime 判断游戏是否开始过; 移除全部棋盘空值分支)
- Core 层: 移除生成时可解性检查 (删除 SolvabilityChecker / ISolvabilityChecker; MineGenerator 移除重试循环与 ShuffleEngine 回归纯随机生成; IMineField.Generate 改为 void 返回值, 存档恢复改用 Apply 应用雷位图)
- UI 层: 棋盘订阅与格子池绑定重构 (棋盘事件订阅移入绑定/解绑并移除 _subscribedBoard 字段与永不复发的棋盘变化事件分支; CellViewModel 通过 IGame 查询雷数显示; MainViewModel 开始新游戏命令同步化)
- 测试: Core / UI 层测试适配雷数查询与棋盘非空化

### Fixed

- 测试: 单实例服务器客户端断开后继续等待激活请求测试偶发超时修复 (客户端断开后服务器复位管道需时, 立即发送第二个激活请求会与复位竞速导致连接超时; 断开后等待服务器复位完成再发送激活请求)

---

## [1.1.11] - 2026-08-14

**单实例运行收尾修复与单实例服务器测试补全**: 修复单实例服务器客户端断开后无法接受后续连接 (连接处理改为无条件 Disconnect 复位管道) 与单实例激活时窗口被遮挡无法置前 (WindowsHelper.BringToFront 绕过 Windows 前台激活限制) 两个问题; 新增 SingleInstanceServerTests 单元测试组覆盖客户端断开后继续等待激活请求等场景; AppMetadata 异常文档换行排版, MineClearance.UI 启用 AllowUnsafeBlocks.

### Fixed

- Infrastructure 层: 单实例服务器客户端断开后无法接受后续连接修复 (客户端连接后未发送激活请求即断开时 IsConnected 已为 false, 原条件判断跳过 Disconnect 导致管道实例未复位, 后续激活请求全部超时; 改为连接处理内层 try/finally 无条件 Disconnect 复位管道, 移除 IsConnected 条件判断, 客户端异常断开或复位失败由外层 IOException 捕获后继续等待)
- UI 层: 单实例激活时窗口被遮挡无法置前修复 (Windows 前台激活限制拦截后台进程的 SetForegroundWindow 请求, 窗口仅任务栏闪烁无法置前; 新增 WindowsHelper.BringToFront 通过 AttachThreadInput 挂接输入队列绕过前台锁, 再 SetForegroundWindow + SetFocus 置前并获取键盘焦点, 挂接失败时 SetWindowPos 置顶还原兜底; 最小化窗口仍先恢复 Normal 再置前, 非 Windows 平台保持 Activate)

### Added

- 测试: Infrastructure 层单元测试新增 1 组 (SingleInstanceServerTests, 覆盖单实例创建判定、激活请求端到端传递、非激活字节不触发回调、取消令牌退出等待循环、客户端断开后继续等待后续激活请求)

### Changed

- UI 层: AppMetadata 异常文档换行排版并改用 langword null 引用
- 工程化: MineClearance.UI 项目启用 AllowUnsafeBlocks (LibraryImport 源码生成 P/Invoke 的要求)

---

## [1.1.10] - 2026-08-13

**单实例运行与异常处理加固, 三层单元测试补全**: 新增单实例运行 (命名管道服务器, 已有实例时请求激活窗口并退出) 与未处理异常处理 (独立日志文件, UI 线程异常 Toast 提示日志路径); Core / Infrastructure / UI 三层共新增 28 组单元测试与程序集级测试夹具, 移除全部占位冒烟测试, 非测试项目对测试程序集与 Moq 动态代理程序集开放内部可见性.

### Added

- 功能: 单实例运行 (Infrastructure 层新增 SingleInstanceServer 命名管道服务器, TryCreate 创建失败表示已有实例在运行, SendActivateRequest 请求已有实例激活; UI 层 Program.cs 启动时检查单实例, 已有实例时发送激活请求后退出, App.axaml.cs 收到激活请求时恢复最小化窗口并激活)
- 功能: 未处理异常处理 (Infrastructure 层新增 UnhandledExceptionHelper 将未处理异常写入独立日志文件; UI 层挂接 AppDomain.UnhandledException / Dispatcher.UnhandledException / UnobservedTaskException, UI 线程异常 Toast 提示日志文件路径)
- 测试: Core 层单元测试新增 9 组 (Game / GameManager / GameBoardDictionary / MineField / GameConfig / GameResult / GameSaveData / GameTimer / Position, 覆盖状态机流转、格子操作、胜负判定、地雷布局生成、相邻雷数计算、存档与结果校验、计时与位置计算)
- 测试: Infrastructure 层单元测试新增 8 组 (BootstrapUpdateHelper / Constants / FileLoggerOptions / FileLoggerProvider / GameDataRepository / ServiceRegistration / UpdateService, 覆盖引导更新参数解析与残留清理、数据目录重定向、日志级别配置解析与持久化、日志级别过滤与内容写入、存档与结果记录的加载/保存/删除/清空、服务注册行为、更新服务状态守卫与检查/下载状态机流转)
- 测试: UI 层单元测试新增 11 组 (AppMetadata / Constants / GameResultRow / HistoryViewModel / KeyExtensions / MainViewModel / SettingsViewModel / StatsRowBuilder / ToastItem / UIOptions / UpdateViewModel, 覆盖应用元数据读取、设置文件路径重定向、游戏结果行显示文本格式化、历史记录统计聚合与筛选排序、热键有效性校验、主视图难度参数联动与开始/继续/导航、设置项配置同步与热键录制、统计行胜率用时聚合、Toast 倒计时与悬停暂停与点击回调、UI 配置解析钳制与持久化、更新视图模型状态反馈与悬浮球/抽屉/缓存清理)
- 测试: 程序集级测试夹具 TestEnvironmentFixture (AssemblyFixture 注册, 通过环境变量将数据根目录重定向到临时目录, 结束后恢复并清理; 文件系统测试禁用并行避免测试间文件冲突)
- 工程化: Directory.Build.props 为非测试项目添加 DynamicProxyGenAssembly2 InternalsVisibleTo (允许 Moq 动态代理程序集模拟内部类型)

### Changed

- 测试: 全部测试类的 XML 文档注释类名/方法名纯文本引用改为 `<see cref="..."/>` 引用 (Core 9 组 + Infrastructure 7 组, IDE 内可点击跳转并跟随重命名)
- 测试: Infrastructure 层测试类 XML 文档注释补充 param/returns 参数与返回值说明 (FileLoggerOptions / FileLoggerProvider / GameDataRepository / UpdateService, 与上轮 cref 引用重构配套)
- 工程化: Infrastructure 项目新增 InternalsVisibleTo MineClearance.UI.Tests (UI 层单元测试访问内部常量与类型)
- 工程化: UI.Tests 项目新增 Microsoft.Extensions.Configuration.Json 包引用 (UI 配置解析测试), Directory.Packages.props 移除无消费的 Microsoft.Extensions.Configuration 包版本
- Core 层: IGameTimer 接口精简 (移除 INotifyPropertyChanged 继承与 IsRunning/Refresh/ReStart 无消费成员, 计时器显示改由 UI 轮询 Elapsed 驱动; GameTimer 实现同步移除事件通知, 字段初始化替代构造函数)
- Core 层: IMineField 移除 GetAdjacentMineCount 无消费成员 (生产侧相邻雷数统一经 Cell.AdjacentMineCount 读取; MineField 实现同步移除)
- UI 层: GameViewModel 计时刷新移除 Timer.Refresh() 调用 (轮询直接读取 Elapsed)
- Infrastructure 层: BootstrapUpdateHelper 目标可执行文件路径提取为局部变量复用 (File.Move 与 Process.Start 共用同一路径表达式)
- Infrastructure 层: Constants 新增 AppDataRootDirectoryEnvironmentVariableName 内部常量, AppDataRootDirectory 支持环境变量重定向数据根目录 (测试用, 避免测试触碰真实数据目录)
- Infrastructure 层: UpdateService 主构造函数新增可选 HttpClient 参数 (为 null 时按默认配置创建, 供测试注入模拟消息处理程序), 类声明移除接口已继承的 IDisposable
- Infrastructure 层: FileLoggerProvider 日志写入加锁 (Lock 对象保护 StreamWriter 并发写入, 多线程写日志安全)
- Infrastructure 层: Constants 新增未处理异常日志文件路径与激活请求常量 (UnhandledExceptionLogFilePath / MaxWaitTimeForActivationRequest / ActivateRequestByte)

### Removed

- 测试: 移除 Core 层占位冒烟测试 SmokeTest (由真实单元测试替代)
- 测试: 移除 Infrastructure 层占位冒烟测试 SmokeTest (由真实单元测试替代)
- 测试: 移除 UI 层占位冒烟测试 SmokeTest (由真实单元测试替代)

---

## [1.1.9] - 2026-08-12

**引导更新重命名兼容与发布工程化修复**: 修复用户重命名可执行文件后引导更新失效的问题 (解压更新包后检测可执行文件名是否被重命名, 若重命名则将更新包导出的原始可执行文件重命名为用户自定义文件名再启动, 修复更新后启动残留旧版本; 等待进程退出超时提取为 MaxWaitTimeForProcessExit 常量, 原始可执行文件名提取为 OriginalExecutableName 常量按平台区分后缀); 调整 CI 测试报告与发布说明 (TestResults 忽略规则放行 trx 报告, 修复 PR 测试报告缺失; 发布说明移除 .NET 10 运行时安装要求, 自包含发布解压即用).

### Fixed

- Infrastructure 层: 用户重命名可执行文件后引导更新失效修复 (解压更新包后检测可执行文件名是否被重命名, 若重命名则将更新包导出的原始可执行文件重命名为用户自定义文件名再启动, 修复更新后启动残留旧版本的问题; 等待进程退出超时提取为 MaxWaitTimeForProcessExit 常量, 原始可执行文件名提取为 OriginalExecutableName 常量按平台区分后缀)
- 工程化: .gitignore TestResults 忽略规则调整 (TestResults/ 整目录忽略改为 TestResults/* 并放行 !TestResults/*.trx, 使 dorny/test-reporter 能解析 PR 生成的 trx 测试报告, 修复 No test report files were found)

### Changed

- 工程化: Release 发布说明移除 .NET 10 运行时安装要求 (publish 指定 runtime 为自包含发布, 下载对应平台 zip 解压即可运行)

---

## [1.1.8] - 2026-08-11

**游戏结束状态漏洞与低分屏棋盘适配修复**: 修复游戏失败/胜利后暂停按钮仍可点击, 继续后可继续操作已结束游戏 (甚至可将失败局继续完成并覆盖结果为胜利) 的问题, 暂停命令增加 CanExecute 终局条件; 修复低分辨率屏幕下大师棋盘超出窗口无法完整查看的问题, 棋盘外层增加滚动容器, 窗口最小尺寸按屏幕工作区钳制上限, 位置钳制边距统一为逻辑像素语义.

### Fixed

- UI 层: 终局后暂停/继续漏洞修复 (PauseResume 命令 CanExecute 绑定 !IsGameEnded, IsGameEnded 变化时通知刷新命令可用性, 游戏失败/胜利后暂停按钮禁用, 无法再继续操作已结束的游戏)
- UI 层: 低分屏棋盘显示不全修复 (棋盘外层增加 ScrollViewer 滚动容器, 窗口小于棋盘时出现滚动条, 点击位置换算基于控件自身坐标不受滚动影响)
- UI 层: 窗口最小尺寸钳制到屏幕工作区 (最小尺寸上限 = 工作区逻辑尺寸 - 钳制边距, 主视图/历史视图/游戏视图统一经 ApplyMinSize 钳制, 窗口客户区不超出屏幕)

### Changed

- UI 层: 窗口位置钳制边距统一为逻辑像素语义 (AdjustPositionToWorkingArea 按屏幕缩放系数换算后钳制, 与最小尺寸钳制口径一致), 移除 WindowDecorationMargin 边框计算, 钳制边距常量值调整 (20/60 → 8/30)

---

## [1.1.7] - 2026-08-11

**更新服务接口去可空化重构**: IUpdateService 全部可空属性改为非空契约 (LatestVersion/Exception 未就绪时访问抛 InvalidOperationException, 其余属性以 0 为初始与哨兵值), 状态机各分支重排属性写入顺序 (异常与版本信息在状态通知前就绪, 检查失败重置版本信息而下载失败保留版本号供重试), 下载进度属性终态化 (开始/取消归零, 完成置 100% 与总大小, 失败速度归零), 百分比基数常量迁入基础设施层, UI 层移除属性断言与抽屉内容手动初始化, 抽屉内容完全由服务端属性驱动.

### Changed

- Infrastructure 层: IUpdateService 接口去可空化 (LatestVersion/Exception 改为非空契约, 未就绪访问抛 InvalidOperationException; TotalBytes/DownloadedBytes/ProgressPercentage/SpeedBytesPerSecond 改为非空值类型, 0 为初始与哨兵值)
- Infrastructure 层: UpdateService 状态机属性写入顺序重排 (异常清空/记录先于状态通知, 检查失败重置下载地址/版本/总大小, 下载失败保留版本号供重试)
- Infrastructure 层: 下载进度属性终态化 (下载开始与取消归零, 完成置 100% 与总大小, 失败速度归零), 完整包识别前置到下载入口
- Infrastructure 层: 内部字段去可空化 (_currentVersion/_downloadUri 空串初始), 更新包临时文件路径提取为静态字段, 完整包校验改用哨兵值
- UI 层: PercentBase 百分比基数常量由 UI 层迁入基础设施层 (服务端完成态百分比与 UI 统计行共用)
- UI 层: UpdateViewModel 简化 (移除属性非空断言与抽屉内容手动初始化, 下载状态转换时同步刷新进度与大小文本, OpenUpdateLogFolder 改私有, 下载完成 Toast 文案调整)

---

## [1.1.6] - 2026-08-11

**下载抽屉内容初始化修复**: 进入下载/下载失败状态时立即更新抽屉内容 (版本/进度/已下载/速度/异常文本), 不再依赖首次进度或属性变化事件刷新, 修复抽屉打开瞬间版本文本为空或残留上次下载异常文本的问题.

### Fixed

- UI 层: 下载开始时抽屉内容初始化 (版本文本/进度归零/已下载与速度文本/异常清空移入下载状态转换分支, 进度刷新移除版本文本赋值, 修复抽屉打开瞬间内容为空或残留上次异常)
- UI 层: 下载失败时异常文本即时写入 (异常断言与赋值移入失败状态转换分支, 进度刷新移除异常文本赋值, 不再依赖属性变化事件刷新)

---

## [1.1.5] - 2026-08-11

**历史记录视图卡顿修复**: 点击进入历史记录视图时 UI 冻结半秒, 根因为 Background 优先级的延迟刷新仍在视图切换渲染前同步执行全量重建; 改为切换后同步刷新, Refresh 新增数据指纹 (记录数与首条记录引用) 数据未变化时跳过重建, 统计聚合改为单次遍历累积 (新增 StatsRowBuilder, 移除多次全量遍历与 ToList 分配), 反复进出历史视图零开销.

### Fixed

- UI 层: 历史记录视图切换卡顿修复 (Dispatcher.Background 延迟刷新未推迟到渲染之后, 全量重建仍阻塞在视图切换渲染前; 移除延迟刷新改为切换后同步调用)
- UI 层: Refresh 数据指纹跳过 (记录数与首条记录引用, 数据未变化时不重建统计与记录行, 反复进入历史视图零开销)

### Changed

- UI 层: 统计聚合单次遍历重构 (新增 StatsRowBuilder 累加器, 6 组难度统计一次遍历累积, 移除 CreateStats 多次全量遍历与 ToList 分配, 减轻 GC 压力)
- UI 层: GameViewModel 行数/列数变化通知 partial 方法由表达式体改为块体 (风格统一)

---

## [1.1.4] - 2026-08-11

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

[Unreleased]: https://github.com/xiting910/MineClearance/compare/v1.6.2...HEAD
[1.6.2]: https://github.com/xiting910/MineClearance/releases/tag/v1.6.2
[1.6.1]: https://github.com/xiting910/MineClearance/releases/tag/v1.6.1
[1.6.0]: https://github.com/xiting910/MineClearance/releases/tag/v1.6.0
[1.5.4]: https://github.com/xiting910/MineClearance/releases/tag/v1.5.4
[1.5.3]: https://github.com/xiting910/MineClearance/releases/tag/v1.5.3
[1.5.2]: https://github.com/xiting910/MineClearance/releases/tag/v1.5.2
[1.5.1]: https://github.com/xiting910/MineClearance/releases/tag/v1.5.1
[1.5.0]: https://github.com/xiting910/MineClearance/releases/tag/v1.5.0
[1.4.0]: https://github.com/xiting910/MineClearance/releases/tag/v1.4.0
[1.3.2]: https://github.com/xiting910/MineClearance/releases/tag/v1.3.2
[1.3.1]: https://github.com/xiting910/MineClearance/releases/tag/v1.3.1
[1.3.0]: https://github.com/xiting910/MineClearance/releases/tag/v1.3.0
[1.2.3]: https://github.com/xiting910/MineClearance/releases/tag/v1.2.3
[1.2.2]: https://github.com/xiting910/MineClearance/releases/tag/v1.2.2
[1.2.1]: https://github.com/xiting910/MineClearance/releases/tag/v1.2.1
[1.2.0]: https://github.com/xiting910/MineClearance/releases/tag/v1.2.0
[1.1.11]: https://github.com/xiting910/MineClearance/releases/tag/v1.1.11
[1.1.10]: https://github.com/xiting910/MineClearance/releases/tag/v1.1.10
[1.1.9]: https://github.com/xiting910/MineClearance/releases/tag/v1.1.9
[1.1.8]: https://github.com/xiting910/MineClearance/releases/tag/v1.1.8
[1.1.7]: https://github.com/xiting910/MineClearance/releases/tag/v1.1.7
[1.1.6]: https://github.com/xiting910/MineClearance/releases/tag/v1.1.6
[1.1.5]: https://github.com/xiting910/MineClearance/releases/tag/v1.1.5
[1.1.4]: https://github.com/xiting910/MineClearance/releases/tag/v1.1.4
[1.1.3]: https://github.com/xiting910/MineClearance/releases/tag/v1.1.3
[1.1.2]: https://github.com/xiting910/MineClearance/releases/tag/v1.1.2
[1.1.1]: https://github.com/xiting910/MineClearance/releases/tag/v1.1.1
[1.1.0]: https://github.com/xiting910/MineClearance/releases/tag/v1.1.0
[1.0.0]: https://github.com/xiting910/MineClearance/releases/tag/v1.0.0
