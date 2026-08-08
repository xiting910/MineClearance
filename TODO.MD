# MineClearance UI 层开发计划

> 本文档是 MineClearance 从 WinForms 迁移到 Avalonia 后, UI 层的最终设计方案与开发任务清单.
> UI 架构与代码风格参照已完成的 Avalonia 项目 [CalculateFolderSize](https://github.com/xiting910/CalculateFolderSize).

---

## 1. 设计总览

### 1.1 技术栈

| 项目 | 选型                                                                                                   |
| ---- | ------------------------------------------------------------------------------------------------------ |
| 框架 | Avalonia 12.1.1 (FluentTheme, 编译绑定默认开启)                                                        |
| MVVM | CommunityToolkit.Mvvm 8.4.2 (ObservableObject, [ObservableProperty], [RelayCommand])                   |
| 表格 | Avalonia.Controls.DataGrid 12.1.2 (已引入)                                                             |
| DI   | Microsoft.Extensions.DependencyInjection (AddCore / AddInfrastructure + UI 服务直接在 Program.cs 注册) |
| 配置 | Microsoft.Extensions.Configuration (Settings 目录下所有 JSON 已加载进 IConfiguration)                  |
| 日志 | 现有 Infrastructure FileLogger (ILogger + LoggerMessage)                                               |

### 1.2 架构模式 (参照 CalculateFolderSize)

```
ShellWindow (主窗口)
  └── ShellView (根视图)
        ├── 主视图 MainView            ← 3 视图互斥切换, 由 ShellViewModel.CurrentView 控制
        ├── 游戏视图 GameView
        ├── 历史记录视图 HistoryView
        └── ToastView (全局右下角通知, 常驻覆盖)

SettingsWindow (附加窗口, 独立打开, 非抽屉)
```

- **视图切换**: ShellViewModel 常驻持有 `Main` / `Game` / `History` 三个子 ViewModel 实例 (均注册为单例), 通过 `[ObservableProperty] CurrentView` 切换并同步各视图可见性. 三视图常驻 ShellView 的 Panel (宿主层控制可见性而非 ContentControl 重建: 其余视图 IsVisible 切换, 游戏视图常驻布局以预热棋盘控件池), 游戏视图与棋盘控件池常驻, 切换视图零重建 (游戏实例本身存于 Core 的 IGameManager).
- **设置窗口**: 独立 Window, **窗口单例** (由 ShellView code-behind 创建, 已打开则 Activate), SettingsViewModel 每次打开新建 (Transient). VM 不持有 View, 弹窗职责在 View 层
- **所有通知统一为右下角 Toast**: 游戏结果提示、删除/清空历史反馈、保存反馈、错误提示等一律走 ToastViewModel.Show(). 不使用系统 MessageBox.

### 1.3 已确认的设计决策

- [x] 去除底部状态栏
- [x] 放弃准备游戏面板 (难度选择整合进主视图: 下拉框选难度 + 开始新游戏按钮)
- [x] 主窗口 3 视图切换: 主视图 / 游戏视图 / 历史记录视图
- [x] 1 个附加窗口: 设置窗口 (关于信息放设置窗口内)
- [x] 所有弹窗通知改为右下角 Toast
- [x] 放弃作弊模式 / 开机自启 / 桌面快捷方式
- [x] 历史面板与游戏结果均走 Core 的 IGameDataRepository (存档与历史已由 Infrastructure 持久化)

---

## 2. 界面设计

### 2.1 ShellWindow + ShellView

| 文件                    | 职责                                                                                      |
| ----------------------- | ----------------------------------------------------------------------------------------- |
| Views/ShellWindow.axaml | 桌面主窗口: Title "MineClearance", Icon logo.ico, 居中启动, 承载 ShellView                |
| Views/ShellView.axaml   | 根视图: 3 视图常驻 Panel (宿主层控制可见性, 游戏视图常驻布局预热) + 全局 ToastView 覆盖层 |

ShellWindow 挂接 `Closing` 事件: 若游戏进行中 (状态为 InProgress / Paused), 取消关闭并调用 `IGameManager.SaveAndExitAsync()` 保存进度后真正关闭 (无弹窗, 主视图显示"继续游戏").

### 2.2 主视图 MainView (替代旧 MenuPanel + GamePreparePanel)

- 标题 "扫雷游戏" (TitleForegroundBrush), 青色提示文字 (EmphasisBrush) 位于参数框顶部
- **左侧参数输入框** (卡片大框, CardBackgroundBrush):
  - 顶部提示: "选择难度, 自定义难度可编辑棋盘参数" (青色强调)
  - 参数 Grid 五行, 标签彩色: 难度: (黄, YellowLabelBrush) / 高度·宽度: (紫, VioletLabelBrush) / 雷数: (红, RedLabelBrush) / 随机种子: (灰, SecondaryTextBrush)
  - 难度下拉框 (初级 / 中级 / 高级 / 大师 / 自定义, 无 ToolTip, 与参数行等宽对齐)
  - 高度 (1-30) / 宽度 (1-50) / 雷数 (1-宽×高-1, 上限随宽高联动) / 随机种子 (留空随机生成)
  - 预设难度: 参数实时显示对应预设值且不可编辑, 种子清空; 自定义难度: 允许编辑
  - 非自定义难度时卡片悬浮提示 "非自定义难度不允许输入" (ParameterInputTip)
- **右侧按钮列**: 等宽按钮 (150px, 文本居中, 四色): 开始新游戏 (蓝 Primary) / 继续游戏 (绿 Success, 仅当 `SaveData` 存在时可见) / 历史记录 (紫 Neutral) / 设置 (橙 Accent)
- **"开始新游戏"**: 预设难度走 `StartNewGame(difficulty)`, 自定义难度以输入框参数构建 `GameConfig` + 种子走 `StartNewGame(config, seed)`, 切换至游戏视图; 清空旧存档由 GameManager.StartNewGame 内部调用 `DeleteGameSaveDataAsync()` 完成
- 无自定义难度弹窗

### 2.3 游戏视图 GameView (核心)

#### 2.3.1 顶部信息栏

| 元素         | 绑定/说明                                                                                                              |
| ------------ | ---------------------------------------------------------------------------------------------------------------------- |
| 状态         | `IGame.Status` 描述文本 (等待开始/进行中/已暂停/胜利/失败, 黄色)                                                       |
| 难度         | `IGame.Difficulty` 描述文本 + 棋盘尺寸与雷数                                                                           |
| 剩余地雷数   | 总雷数 - `Board.FlagCount`                                                                                             |
| 已打开格子数 | `Board.OpenedCount`                                                                                                    |
| 完成度       | `IGame.Completion` 内部 0-1, 显示为百分比                                                                              |
| 游戏时间     | MM:SS, DispatcherTimer (250ms, 计入常量) 调 `IGame.Timer.Refresh()` 后读 `Elapsed`                                     |
| 种子         | `IGame.Seed` (灰色, 自定义难度可复现)                                                                                  |
| 按钮         | 操作提示 / 重新开始 / 暂停·继续 / 保存并返回主视图 / 不保存并返回主视图 (均带 ToolTip, 右侧常驻, 宽度不足时信息区裁剪) |

- 暂停: `IGame.Pause()/CancelPause()`, 暂停时棋盘上方显示"已暂停"覆盖层 (非弹窗)
- 保存并返回主视图: `IGameManager.SaveAndExitAsync()` 保存进度 + Toast "游戏进度已保存, 下次可继续游戏"; 不保存并返回主视图: `IGameManager.ExitWithoutSaving()` 丢弃进度

#### 2.3.2 棋盘渲染 (固定格子池 + Canvas 绝对定位)

- **格子池**: GameViewModel 构造时按最大棋盘 30×50 = 1500 格一次性构建固定 CellViewModel 池, 每格 X/Y 像素坐标按最大棋盘位置 × CellSize 计算且固定不变, 启动构建毫秒级无感
- **渲染**: `ItemsControl` + `ItemsPanel` = `Canvas`; 每个格子容器通过 ItemContainerTheme 绑定 `Canvas.Left/Top` (X/Y) 与 `IsVisible` (Canvas 只读取直接子元素的布局参数, 故不能写在 DataTemplate 上); ItemsControl 宽高绑定当前棋盘像素尺寸 (`Columns×CellSize` / `Rows×CellSize`), 低难度时超出棋盘的行列格子 `IsVisible=false` 隐藏, 棋盘整体顶部居中显示 (与旧项目一致)
- **数据更新**: UpdateBoard 全量遍历格子池只做两件事——按 `row < Rows && col < Columns` 切换 IsVisible, `UpdateCell(cell)` 替换内部 Cell 引用 (首击前 Board 为 null 或超出范围时引用共享占位 Cell); 不重建任何控件
- **控件实例化只发生一次** (启动预热: 构造时初始化最大棋盘尺寸并预置占位格子可见, 常驻布局首次布局即完成全部实例化), 此后开始/重开/切换难度/首击/视图往返全部零控件创建, 仅更新数据与可见性
- 格子模板: 3 层 (背景 Border + 悬停高亮 Border + TextBlock), 按 `Cell.Type` 显示:
  | CellType      | 显示                                                           |
  | ------------- | -------------------------------------------------------------- |
  | Unopened      | 灰色块                                                         |
  | Empty         | 白色块                                                         |
  | Number        | 白色块 + 数字 (1-8 经典配色: 1蓝 2绿 3红 4紫 5栗 6青 7黑 8灰)  |
  | WarningNumber | 黄色块 (周围旗数超过实际雷数)                                  |
  | Flagged       | 绿色块 + 旗符号 ⚑                                              |
  | Question      | 灰块 + 问号 ?                                                  |
  | Mine          | 红色块 + 地雷符号 💣 (失败后揭示全部地雷, 踩中的格子深红色突出) |
- 全部格子有悬停效果 (pointerover 半透明橙色高亮层)
- 格子大小: 固定 `Constants.CellSize` = 25px 正方形
- 性能: 数据直接由 `IGame.Board` (IGameBoardDictionary) 读取, `GetCellStates()` 仅供存档序列化使用 UI 不调用; CellViewModel 订阅 Cell.PropertyChanged (Type 变化) 局部更新自身, Board 的 INotifyPropertyChanged (计数变化) 驱动信息栏刷新

#### 2.3.3 交互 (GameView code-behind 处理指针事件)

| 操作         | 行为                                                                                                                                                     |
| ------------ | -------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 左键点击     | 统一分发 `GameViewModel.LeftClickAt(pos)`: 未打开格 `OpenCell` (首次点击由 Core 生成地雷, 踩雷记录位置突出显示), 数字格 `OpenAdjacentCells` (chord 展开) |
| 右键点击     | 数字格 `FlagAdjacentCells` (周围未开格全插旗) + 其他格三态循环: Flagged → Question → Unopened (FlagCell / QuestionCell / UnmarkCell)                     |
| 按住左键滑动 | 持续 LeftClickAt 分发 (扫一片)                                                                                                                           |
| 按住右键滑动 | 持续三态标记                                                                                                                                             |
| 状态提示     | 等待开始/已结束状态禁止交互 (Core AssertGamePerformable), 结束由右下角 Toast 通知结果                                                                    |

### 2.4 历史记录视图 HistoryView

#### 2.4.1 统计区

- 6 组统计 (全部 / 初级 / 中级 / 高级 / 大师 / 自定义): 游戏次数 / 胜利次数 / 胜率 / 平均胜利用时 / 最短胜利用时 / 平均完成度 (内部 0-1, 显示为百分比)
- 用 DataGrid 排布

#### 2.4.2 详细区

- `DataGrid` 绑定 `IGameDataRepository.GameResults` (Core 已按时间倒序)
- 列: 开始时间 (yyyy-MM-dd HH:mm:ss) / 难度 / 结果 (胜利/失败) / 完成度 (内部 0-1, 显示为百分比) / 用时 (mm:ss.xx) / 宽度 / 高度 / 地雷数 (自定义难度显示实际值, 内置难度显示对应配置)
- **排序**: 列头点击排序, 参照 CalculateFolderSize ResultView 的 Tag + PointerPressed 模式, 主键/次键各一列或单列升降切换
- **筛选**: 工具栏 ComboBox: 难度 (全部/5 档), 结果 (全部/胜利/失败), "清除筛选" 按钮
- **删除选中**: 选中行 + "删除选中" 按钮 → 直接删除 + Toast "已删除 N 条记录" (不做确认弹窗)
- **清空历史**: 危险操作, 按钮二次点击确认模式 (首次点击按钮变为 "再次点击确认清空", 3 秒内再点才执行) + Toast 反馈

### 2.5 设置窗口 SettingsWindow

独立 Window (420×520), 承载 SettingsView, **窗口单例** (已打开则 Activate), 无窗口内标题行与关闭按钮 (窗口标题栏自带):

| 区域    | 标题色                    | 内容                                                                            |
| ------- | ------------------------- | ------------------------------------------------------------------------------- |
| UI 配置 | 蓝 (TitleForegroundBrush) | 主题 ComboBox (跟随系统/浅色/深色, 即时切换) + Toast 显示时长 (0-20 秒, 默认 5) |
| 日志    | 紫 (VioletLabelBrush)     | 日志级别 ComboBox + 打开日志文件夹按钮 (最左, 蓝 Primary)                       |
| 关于    | 红 (RedLabelBrush)        | 产品/版本/作者/许可证/GitHub 链接 (AssemblyMetadata, 点击打开仓库)              |

- 所有配置控件直接双向绑定配置属性 (ComboBox SelectedItem / NumericUpDown Value), **setter 变化即自动保存文件并即时生效, 无需保存按钮**:
  - 主题: 绑定 UIOptions.Theme, setter 写文件, SettingsViewModel 的 OnThemeChanged 回调中设置 Application.Current.RequestedThemeVariant
  - Toast 时长: 绑定 UIOptions.ToastDurationSeconds, setter 写文件, ToastViewModel 每次 Show 时读取新值
  - 日志级别: 绑定 Infrastructure 的 IFileLoggerOptions.Level, setter 写文件, FileLogger 每次写日志实时读取
- 设置持久化: UIOptions / IFileLoggerOptions 属性 setter 变化即写 `Settings/` 下的 JSON (同 FileLoggerOptions 模式, Program.cs 已将该目录 JSON 加载进 IConfiguration, 重启自动生效)

### 2.6 Toast 通知

完全照搬 CalculateFolderSize 模式:

- `ToastViewModel` (单例): `Feedback` / `FeedbackVisible` / `Progress` (剩余时间比例) 属性, `Show(message)` 显示后由 DispatcherTimer 按实际经过时间扣减倒计时, 鼠标悬停暂停, 新提示取代旧提示, 时长每次 Show 时从 UIOptions.ToastDurationSeconds 读取 (变化即时生效)
- `ToastView`: 右下角, 半透明深色圆角卡片, 白字, 最大宽度 400, 底部剩余时间进度条 (ScaleTransform 补间平滑缩短)

---

## 3. 文件结构

### 3.1 新增文件

```
srcs/MineClearance.UI/
├── Models/
│   ├── ThemeMode.cs                 # 主题模式枚举 (跟随系统/浅色/深色, [Description])
│   ├── UIOptions.cs                 # UI 配置类 (构造从 IConfiguration 读初始值, setter 变化自动保存文件)
│   └── NavigationTarget.cs          # 导航目标枚举 (GameView / HistoryView / SettingsWindow)
├── EnumDescriptionConverter.cs      # 枚举 → [Description] 文本
├── ViewModels/
│   ├── ShellViewModel.cs            # 3 视图切换 + SettingsWindowRequested 事件
│   ├── MainViewModel.cs             # 难度下拉框 + 棋盘参数输入 (预设只读/自定义可编辑) + 导航命令
│   ├── GameViewModel.cs             # 游戏状态绑定, 信息栏, 计时刷新, 暂停/重开/返回
│   ├── CellViewModel.cs             # 格子包装 (Position + Cell), 数字颜色/符号计算属性
│   ├── HistoryViewModel.cs          # 统计 + 详细 + 排序筛选删除清空
│   ├── SettingsViewModel.cs         # 设置窗口逻辑 + 关于信息 (AssemblyMetadata)
│   └── ToastViewModel.cs            # 右下角通知
└── Views/
    ├── ShellWindow.axaml / .cs      # 主窗口 (Closing 自动保存)
    ├── ShellView.axaml / .cs        # 根视图 + 设置窗口单例创建
    ├── MainView.axaml / .cs
    ├── GameView.axaml / .cs         # 棋盘 + 指针交互 code-behind
    ├── HistoryView.axaml / .cs
    ├── SettingsView.axaml / .cs     # 设置窗口内容
    ├── SettingsWindow.axaml / .cs   # 设置窗口
    └── ToastView.axaml / .cs
```

### 3.2 修改文件

| 文件         | 修改内容                                                                                                                                                                                 |
| ------------ | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| App.axaml    | 添加 DataGrid Fluent 主题 StyleInclude; ThemeDictionaries 浅色/深色资源 (窗口背景/标题/分隔线/按钮四色/卡片背景/标签色/次级文本/强调色等) + 全局 EnumDescriptionConverter                |
| App.axaml.cs | OnFrameworkInitializationCompleted: 按 UIOptions.Theme 设置 RequestedThemeVariant; MainWindow = ShellWindow (DataContext = ShellViewModel)                                               |
| Program.cs   | DI 注册: AddCore / AddInfrastructure + UIOptions 与全部 ViewModel (Shell/Main/Game/History/Toast 为 Singleton, Settings 为 Transient); 其余保持不变 (目录初始化/日志轮转/配置加载已就绪) |
| Constants.cs | 追加 UI 常量: Toast 时长默认值/范围, UI 配置设置文件路径 (UISettings.json)                                                                                                               |

---

## 4. 分阶段任务清单

### 阶段 0: 基础设施

- [x] App.axaml: 添加 DataGrid 主题 Include + 浅色/深色 ThemeDictionaries 资源
- [x] Models/ThemeMode.cs, Models/UIOptions.cs (IConfiguration 读初始值 + 范围钳制, setter 变化自动保存), Models/NavigationTarget.cs
- [x] EnumDescriptionConverter.cs (注册到 App.axaml 全局资源)
- [x] ViewModels/ToastViewModel.cs, Views/ToastView.axaml + .cs
- [x] ViewModels/ShellViewModel.cs, Views/ShellView.axaml + .cs, Views/ShellWindow.axaml + .cs
- [x] App.axaml.cs: 主题应用 + MainWindow 注入
- [x] Program.cs: UI 服务直接注册 (无 AddUI 扩展)
- [x] 验证: 空壳窗口启动, 3 个空视图可切换, Toast 可显示

### 阶段 1: 主视图

- [x] ViewModels/MainViewModel.cs: 难度下拉框 + 棋盘参数属性 (预设只读/自定义可编辑, 雷数上限联动) + 开始新游戏命令 (预设走难度分支, 自定义以输入框参数构建 config+seed)
- [x] Views/MainView.axaml + .cs: 左侧参数输入框 (卡片) + 右侧等宽四色按钮列, 参数 Grid 五行对齐, 非自定义悬浮提示
- [x] 验证: 切换难度实时刷新参数 (非自定义只读); 自定义可编辑且雷数上限联动; 开始新游戏清空存档并进入游戏视图; 有存档时显示"继续游戏"

### 阶段 2: 游戏视图 (核心)

- [x] ViewModels/GameViewModel.cs: 订阅 IGameManager 的 INotifyPropertyChanging/Changed (Game 切换) 与 IGame.PropertyChanged (Board 出现/状态/结果/完成度), 绑定 IGame / Board / Timer; DispatcherTimer 刷新计时; 固定 1500 格 CellViewModel 池 (启动构建, UpdateBoard 全量更新引用与可见性)
- [x] ViewModels/CellViewModel.cs: 格子包装 (Position + Cell, UpdateCell 替换引用) + X/Y 像素坐标 + IsVisible + 数字颜色/符号
- [x] Views/GameView.axaml: 信息栏 (按钮右侧常驻/信息区裁剪) + 棋盘 ItemsControl (Canvas + ItemContainerTheme 定位) + 暂停覆盖层 (只覆盖棋盘)
- [x] Views/GameView.axaml.cs: 指针交互 (左键统一分发/右键三态循环+数字格插旗/滑动持续操作/位置换算用 Position.IsInBounds)
- [x] 游戏结束: 不弹窗, 右下角 Toast 提示胜负/用时/完成度 (百分比), 失败揭示全部地雷并深红标出踩中的格子
- [x] 暂停覆盖层 + 保存并返回 (SaveAndExitAsync, 等待开始/已结束不保存不提示) / 强制返回 (ExitWithoutSaving) + Toast
- [x] 性能: 控件池复用, 首次进入游戏视图一次性实例化, 大师难度开始/重开/切换零卡顿
- [x] 验证: 完整玩一局 (胜负都验证), 首击保护, chord, 警告数字, 暂停, 重开, 返回自动保存

### 阶段 3: 历史记录视图

- [ ] ViewModels/HistoryViewModel.cs: 统计聚合 (6 组), 排序, 筛选, 删除, 清空 (二次确认)
- [ ] Views/HistoryView.axaml + .cs: DataGrid 列头排序 (Tag + PointerPressed), 工具栏
- [ ] 验证: 玩几局后统计正确, 排序筛选删除清空生效, Toast 反馈

### 阶段 4: 设置窗口

- [x] ViewModels/SettingsViewModel.cs: 主题/Toast 时长 (UIOptions), 日志级别 (IFileLoggerOptions), 关于 (AssemblyMetadata), 主题变化回调即时切换 RequestedThemeVariant, 打开日志文件夹
- [x] Views/SettingsView.axaml + .cs + SettingsWindow.axaml + .cs (独立窗口, 窗口单例由 ShellView 创建, 无保存按钮/无窗口内标题, 配置控件直接双向绑定)
- [x] 验证: 主题/Toast 时长/日志级别即时生效并自动保存, 重启后仍生效, 关于信息正确, GitHub 链接可打开, 多次点击只弹一个窗口
