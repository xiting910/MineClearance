# 💣 MineClearance

一个基于 **Avalonia UI** 的跨平台扫雷游戏，采用 **Clean Architecture** 架构，使用 **.NET 10.0** 构建。

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![.NET 10.0](https://img.shields.io/badge/.NET-10.0-512BD4.svg)](https://dotnet.microsoft.com/)
[![CI](https://github.com/xiting910/MineClearance/actions/workflows/ci.yml/badge.svg)](https://github.com/xiting910/MineClearance/actions/workflows/ci.yml)
[![CodeQL](https://github.com/xiting910/MineClearance/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/xiting910/MineClearance/actions/workflows/codeql-analysis.yml)
[![Dependency Review](https://github.com/xiting910/MineClearance/actions/workflows/dependency-review.yml/badge.svg)](https://github.com/xiting910/MineClearance/actions/workflows/dependency-review.yml)

---

## ✨ 特性

- 🖥️ **跨平台支持** — Windows / Linux / macOS
- 🎮 **经典扫雷玩法** — 左键翻开, 点击数字格自动展开周围, 右键标旗/问号 (数字格一键插旗周围), 按住滑动连续操作, 警告数字检测
- 💾 **自动保存** — 关闭窗口时自动保存进行中的游戏, 下次可从主视图继续游戏
- 📊 **历史记录** — 按难度分组的统计汇总 (胜率/用时/完成度), 日期/难度/结果多条件筛选与列头排序, 支持删除选中与二次确认清空
- 🎨 **主题切换** — 跟随系统 / 浅色 / 深色, 即时生效并自动保存
- 🖼️ **背景图片** — 内置 3 张背景图片可直接选择, 勾选"使用自定义背景图片"后可从程序目录 Pictures 文件夹中的图片选择 (支持拉伸方式与透明度调节, 放入新图片可手动刷新识别)
- ⚙️ **设置抽屉** — 主题 / 背景图片 / Toast 时长与数量 / 日志级别 / 下载悬浮球即时配置, 手动检查更新与清除更新缓存, 关于信息 (游戏视图内呼出自动暂停)
- 👋 **首次启动提示** — 首次启动展示欢迎信息与操作指引 (作者 / Esc 打开设置抽屉 / 种子雷区 / 自动更新说明), 展示后自动关闭
- 🔢 **格子索引显示与复制** — 等待开始时按住热键显示全部格子索引 (松开隐藏), 首次点击格子可自动复制索引到剪贴板, 热键可在设置中录制
- 🎯 **无猜体验 (No-guess)** — 被迫猜测选到雷格时自动重排雷位使该格安全翻开并继续游戏 (约束求解保证已揭示数字不变); 存在必安全格 (玩家本有确定动作) 或打开必死格 (无合法重排) 时按原逻辑判负
- 🔄 **自动更新** — 启动时后台检查 GitHub 新版本, 断点续传下载并校验更新包完整性, 下载进度悬浮球与详情抽屉, 退出后引导更新失败自动回滚
- 🔁 **单实例运行** — 重复启动时自动激活已有实例主窗口 (Windows 上绕过系统前台激活限制, 被遮挡/最小化均强制置前), 避免多开
- 🛡️ **未处理异常处理** — 未处理异常写入独立日志文件, UI 线程异常通过 Toast 提示日志位置
- 🧱 **Clean Architecture** — 清晰的 Core / Infrastructure / UI 分层，高内聚低耦合
- 🧩 **MVVM 模式** — 基于 CommunityToolkit.Mvvm 源代码生成器
- 📝 **结构化日志** — ILogger + LoggerMessage 源代码生成器，记录游戏关键事件
- 🧪 **完善的单元测试** — Core / Infrastructure / UI 三层单元测试 (xUnit v3 + Moq + coverlet): Core 覆盖状态机流转、格子操作、胜负判定、地雷布局生成、存档与结果校验; Infrastructure 覆盖数据持久化、文件日志、服务注册、更新服务、引导更新与单实例服务器; UI 覆盖视图模型状态与交互、统计筛选排序、配置解析持久化、Toast 计时与更新流程
- 🔁 **CI/CD 自动化** — GitHub Actions 自动构建、测试、CodeQL 安全分析、Release 发布
- 📦 **依赖自动更新** — Dependabot 分组策略，保持依赖最新

---

## 🏗️ 项目结构

```
MineClearance/
├── .editorconfig                                     #   代码风格统一配置
├── .github/                                          #   GitHub 配置
│   ├── dependabot.yml                                #   Dependabot 依赖更新 (NuGet + Actions 分组策略)
│   ├── ISSUE_TEMPLATE/                               #   Issue 模板
│   │   ├── bug_report.md                             #     Bug 报告模板
│   │   ├── config.yml                                #     Issue 模板选择配置 (文档链接)
│   │   └── feature_request.md                        #     功能建议模板
│   ├── PULL_REQUEST_TEMPLATE.md                      #   PR 描述模板
│   └── workflows/                                    #   GitHub Actions 工作流
│       ├── ci.yml                                    #     CI (构建/测试/上传测试结果)
│       ├── codeql-analysis.yml                       #     CodeQL 安全分析 (push/PR/每周定时)
│       ├── dependabot-auto-merge.yml                 #     Dependabot PR 自动 approve + squash 合并
│       ├── dependency-review.yml                     #     依赖漏洞审查 (PR 评论区报告)
│       ├── release-delete.yml                        #     Release 清理 (删除 tag 时同步删除)
│       └── release-publish.yml                       #     Release 发布 (构建/测试/发布产物)
├── .gitattributes                                    #   Git 行尾归一化 (默认 LF), diff 策略与二进制标记
├── .gitignore                                        #   忽略规则 (构建产物 / IDE / .NET / OS)
├── Directory.Build.props                             #   全局构建属性 (TargetFramework / Nullable / CPM)
├── Directory.Packages.props                          #   集中包版本管理 (NuGet CPM)
├── CHANGELOG.md                                      #   变更日志
├── LICENSE                                           #   MIT 许可证
├── MineClearance.slnx                                #   解决方案文件 (.NET XML 格式)
├── README.md                                         #   本文档
├── RePublish.bat                                     #   Windows 清理构建脚本
├── TagPush.bat                                       #   打 tag 并推送发布脚本 (校验版本/工作区/CHANGELOG)
├── srcs/
│   ├── MineClearance.Core/                           # 核心层 — 领域模型、接口、枚举、领域服务
│   │   ├── MineClearance.Core.csproj                 #   项目文件 (DI/Logging Abstractions 引用)
│   │   ├── Constants.cs                              #   游戏常量
│   │   ├── EnumExtensions.cs                         #   枚举扩展 (Description 描述)
│   │   ├── Enums/                                    #   枚举定义
│   │   │   ├── CellType.cs                           #     格子类型
│   │   │   ├── GameDifficulty.cs                     #     游戏难度
│   │   │   └── GameStatus.cs                         #     游戏状态
│   │   ├── Interfaces/                               #   接口定义
│   │   │   ├── IGame.cs                              #     游戏核心接口 (INotifyPropertyChanged + IDisposable)
│   │   │   ├── IGameBoardDictionary.cs               #     棋盘格子字典接口 (IReadOnlyDictionary + INotifyPropertyChanged)
│   │   │   ├── IGameBoardDictionaryFactory.cs        #     棋盘字典工厂接口 (internal)
│   │   │   ├── IGameDataRepository.cs                #     游戏数据仓储接口
│   │   │   ├── IGameFactory.cs                       #     游戏工厂接口 (internal)
│   │   │   ├── IGameManager.cs                       #     游戏管理器接口
│   │   │   ├── IGameTimer.cs                         #     游戏计时器接口
│   │   │   ├── IMineField.cs                         #     地雷场接口 (internal)
│   │   │   ├── IMineSolver.cs                        #     地雷求解器接口 (internal)
│   │   │   └── IMineGenerator.cs                     #     地雷生成器接口 (internal)
│   │   ├── Models/                                   #   领域模型
│   │   │   ├── Cell.cs                               #     游戏格子 (INotifyPropertyChanged)
│   │   │   └── Records/                              #     记录类型
│   │   │       ├── GameConfig.cs                     #       游戏配置
│   │   │       ├── GameResult.cs                     #       游戏结果
│   │   │       ├── GameSaveData.cs                   #       游戏存档
│   │   │       └── Position.cs                       #       位置
│   │   ├── Services/                                 #   领域服务实现 (internal)
│   │   │   ├── Game.cs                               #     游戏核心实现
│   │   │   ├── Game.Logging.cs                       #     游戏日志 (LoggerMessage)
│   │   │   ├── Game.Private.cs                       #     游戏私有实现
│   │   │   ├── GameBoardDictionary.cs                #     棋盘字典实现
│   │   │   ├── GameBoardDictionaryFactory.cs         #     棋盘字典工厂实现
│   │   │   ├── GameFactory.cs                        #     游戏工厂实现
│   │   │   ├── GameManager.cs                        #     游戏管理器实现
│   │   │   ├── GameTimer.cs                          #     游戏计时器实现
│   │   │   ├── MineField.cs                          #     地雷场实现
│   │   │   ├── MineGenerator.cs                      #     地雷生成器实现 (纯随机, 首点及邻域排除)
│   │   │   └── MineSolver.cs                         #     地雷求解器实现 (无猜雷位重排)
│   │   └── IServiceCollectionExtensions.cs           # DI 注册扩展
│   ├── MineClearance.Infrastructure/                 # 基础设施层 — 数据访问、外部服务实现
│   │   ├── MineClearance.Infrastructure.csproj       #   项目文件 (引用 Core + Downloader)
│   │   ├── BootstrapUpdateHelper.cs                  #   引导更新辅助 (准备副本/备份/解压/回滚/重启/清理)
│   │   ├── Constants.cs                              #   常量 (数据目录, 文件路径, Json 选项)
│   │   ├── ILoggingBuilderExtensions.cs              #   日志构建器扩展 (AddFileLogger)
│   │   ├── IServiceCollectionExtensions.cs           #   DI 注册扩展 (AddInfrastructure)
│   │   ├── IUpdateService.cs                         #   更新服务接口
│   │   ├── Models/                                   #   基础设施模型
│   │   │   ├── FileLoggerOptions.cs                  #     文件日志选项
│   │   │   ├── UpdateInfo.cs                         #     更新信息记录
│   │   │   └── UpdateState.cs                        #     更新状态枚举
│   │   ├── Services/                                 #   服务实现
│   │   │   ├── FileLoggerProvider.cs                 #     文件日志提供程序实现
│   │   │   ├── GameDataRepository.cs                 #     游戏数据仓储实现
│   │   │   ├── GameDataRepository.Converter.cs       #     游戏数据仓储 Json 转换器
│   │   │   ├── GameDataRepository.Logging.cs         #     游戏数据仓储日志 (LoggerMessage)
│   │   │   ├── UpdateService.cs                      #     更新服务实现
│   │   │   ├── UpdateService.Logging.cs              #     更新服务日志 (LoggerMessage)
│   │   │   └── UpdateService.Private.cs              #     更新服务私有实现
│   │   ├── SingleInstanceServer.cs                   #   单实例服务器 (命名管道/激活请求)
│   │   └── UnhandledExceptionHelper.cs               #   未处理异常辅助 (未处理异常日志)
│   └── MineClearance.UI/                             # 表示层 — Avalonia 桌面应用
│       ├── MineClearance.UI.csproj                   #   项目文件 (Avalonia 系列/图标/单文件发布)
│       ├── App.Manifest.xml                          #   Windows 应用清单
│       ├── App.axaml                                 #   应用定义 (主题/DataTemplate/颜色资源)
│       ├── App.axaml.cs                              #   应用类 (服务容器/异常处理/主题应用/主窗口)
│       ├── Assets/                                   #   资源目录
│       │   ├── Backgrounds/                          #     内置背景图片 (1.png / 2.png / 3.png)
│       │   └── logo.ico                              #     应用图标
│       ├── AppMetadata.cs                            #   应用元数据 (AssemblyMetadata 读取)
│       ├── Constants.cs                              #   UI 常量
│       ├── EnumDescriptionConverter.cs               #   枚举描述转换器 ([Description] → 文本)
│       ├── KeyExtensions.cs                          #   Key 按键扩展 (快捷键有效性校验)
│       ├── Models/                                   #   UI 模型
│       │   ├── BackgroundImageOption.cs              #     背景图片选项 (显示文本/文件名)
│       │   ├── DifficultyFilterOption.cs             #     难度筛选选项 (多选)
│       │   ├── GameResultRow.cs                      #     游戏结果行 (显示文本/棋盘尺寸)
│       │   ├── NavigationTarget.cs                   #     导航目标枚举
│       │   ├── ResultFilterOption.cs                 #     结果筛选选项 (全部/胜利/失败)
│       │   ├── SortKeys.cs                           #     统计表格排序键常量
│       │   ├── StatsRow.cs                           #     统计行 (难度范围汇总统计)
│       │   ├── StatsRowBuilder.cs                    #     统计行构建器 (单次遍历累积统计)
│       │   ├── ThemeMode.cs                          #     主题模式枚举 (跟随系统/浅色/深色)
│       │   ├── ToastItem.cs                          #     Toast 提示条目 (剩余进度/悬停暂停/点击回调)
│       │   └── UIOptions.cs                          #     UI 配置 (setter 变化自动保存)
│       ├── Program.cs                                #   应用入口 (单实例检查 + DI + Avalonia 启动)
│       ├── ShellWindow.axaml                         #   主窗口 (关闭自动保存/退出触发引导更新/启动更新流程)
│       ├── ShellWindow.axaml.cs                      #   主窗口代码 (自动保存/引导更新/Esc 处理抽屉/启动更新流程)
│       ├── ViewLocator.cs                            #   ViewModel → View 定位器
│       ├── WindowsHelper.cs                          #   Windows 窗口操作辅助 (绕过前台激活限制置前)
│       ├── ViewModels/                               #   视图模型
│       │   ├── CellViewModel.cs                      #     格子视图模型 (固定格子池)
│       │   ├── GameViewModel.cs                      #     游戏视图模型 (固定格子池/交互分发)
│       │   ├── HistoryViewModel.cs                   #     历史记录视图模型 (统计/筛选/排序/删除)
│       │   ├── MainViewModel.cs                      #     主视图模型 (难度选择/参数输入/导航)
│       │   ├── SettingsViewModel.cs                  #     设置视图模型 (背景图片/悬浮球开关/手动检查更新/关闭请求事件)
│       │   ├── ShellViewModel.cs                     #     壳视图模型 (视图切换/导航/抽屉动画/共用遮布)
│       │   ├── ToastViewModel.cs                     #     Toast 提示视图模型 (多条目集合/满员顶掉最早)
│       │   └── UpdateViewModel.cs                    #     更新视图模型 (启动更新流程/检查更新/下载悬浮球与抽屉)
│       └── Views/                                    #   视图
│           ├── DownloadBallView.axaml                #     下载进度悬浮球视图 (点击呼出下载抽屉)
│           ├── DownloadBallView.axaml.cs             #     下载进度悬浮球视图代码后置
│           ├── DownloadDrawerView.axaml              #     下载详情抽屉视图 (进度/详情/异常/取消)
│           ├── DownloadDrawerView.axaml.cs           #     下载详情抽屉视图代码后置
│           ├── GameView.axaml                        #     游戏视图
│           ├── GameView.axaml.cs                     #     游戏视图代码后置
│           ├── HistoryView.axaml                     #     历史记录视图
│           ├── HistoryView.axaml.cs                  #     历史记录视图代码后置
│           ├── MainView.axaml                        #     主视图
│           ├── MainView.axaml.cs                     #     主视图代码后置
│           ├── SettingsView.axaml                    #     设置抽屉内容视图
│           ├── SettingsView.axaml.cs                 #     设置抽屉内容视图代码后置
│           ├── ShellView.axaml                       #     壳视图 (背景图片层 + 视图切换 + 抽屉 + 悬浮球 + Toast 覆盖层)
│           ├── ShellView.axaml.cs                    #     壳视图代码后置
│           ├── ToastView.axaml                       #     Toast 视图 (多条目堆叠/入场动画)
│           └── ToastView.axaml.cs                    #     Toast 视图代码后置
└── tests/
    ├── MineClearance.Core.Tests/                     # Core 层单元测试
    │   ├── MineClearance.Core.Tests.csproj           #   测试项目文件
    │   ├── GameBoardDictionaryTests.cs               #     棋盘字典测试 (格子访问/计数/导出)
    │   ├── GameConfigTests.cs                        #     游戏配置测试 (校验/难度映射)
    │   ├── GameManagerTests.cs                       #     游戏管理器测试 (创建/恢复/存档/结果)
    │   ├── GameResultTests.cs                        #     游戏结果测试 (工厂校验/IsValid)
    │   ├── GameSaveDataTests.cs                      #     游戏存档测试 (工厂校验/IsValid)
    │   ├── GameTests.cs                              #     游戏核心测试 (状态机/操作/胜负/存档)
    │   ├── GameTimerTests.cs                         #     游戏计时器测试 (启停/累计)
    │   ├── MineFieldTests.cs                         #     地雷场测试 (布局/相邻雷数)
    │   ├── MineSolverTests.cs                        #     地雷求解器测试 (无猜重排/必死格判定)
    │   └── PositionTests.cs                          #     位置测试 (索引转换/相邻/边界)
    ├── MineClearance.Infrastructure.Tests/           # Infrastructure 层单元测试
    │   ├── MineClearance.Infrastructure.Tests.csproj #   测试项目文件
    │   ├── AssemblyInfo.cs                           #   程序集级配置 (测试夹具注册/禁用并行)
    │   ├── TestEnvironmentFixture.cs                 #   测试环境夹具 (临时数据目录重定向)
    │   ├── BootstrapUpdateHelperTests.cs             #     引导更新辅助测试 (参数解析/残留清理)
    │   ├── ConstantsTests.cs                         #     常量测试 (数据目录重定向)
    │   ├── FileLoggerOptionsTests.cs                 #     文件日志选项测试 (级别配置解析/持久化)
    │   ├── FileLoggerProviderTests.cs                #     文件日志提供程序测试 (级别过滤/内容写入)
    │   ├── GameDataRepositoryTests.cs                #     游戏数据仓储测试 (存档/结果记录)
    │   ├── ServiceRegistrationTests.cs               #     服务注册测试 (DI 注册行为)
    │   ├── SingleInstanceServerTests.cs              #     单实例服务器测试 (创建/激活请求/断开恢复)
    │   └── UpdateServiceTests.cs                     #     更新服务测试 (状态守卫/状态机流转)
    └── MineClearance.UI.Tests/                       # UI 层单元测试
        ├── MineClearance.UI.Tests.csproj             #   测试项目文件
        ├── AssemblyInfo.cs                           #   程序集级配置 (测试夹具注册/禁用并行)
        ├── TestEnvironmentFixture.cs                 #   测试环境夹具 (临时数据目录重定向)
        ├── AppMetadataTests.cs                       #     应用元数据测试 (AssemblyMetadata 读取)
        ├── ConstantsTests.cs                         #     常量测试 (设置文件路径重定向)
        ├── GameResultRowTests.cs                     #     游戏结果行测试 (显示文本格式化)
        ├── HistoryViewModelTests.cs                  #     历史记录视图模型测试 (统计/筛选/排序/删除)
        ├── KeyExtensionsTests.cs                     #     按键扩展测试 (热键有效性校验)
        ├── MainViewModelTests.cs                     #     主视图模型测试 (难度参数联动/开始/继续/导航)
        ├── SettingsViewModelTests.cs                 #     设置视图模型测试 (配置同步/热键录制)
        ├── StatsRowBuilderTests.cs                   #     统计行构建器测试 (胜率/用时/完成度)
        ├── ToastItemTests.cs                         #     Toast 条目测试 (倒计时/悬停暂停/点击回调)
        ├── UIOptionsTests.cs                         #     UI 配置测试 (解析/钳制/持久化)
        └── UpdateViewModelTests.cs                   #     更新视图模型测试 (状态反馈/悬浮球/抽屉)
```

依赖方向：`UI → Infrastructure → Core`（符合整洁架构依赖规则）

---

## 🚀 快速开始

### 环境要求

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

### 克隆 & 运行

```bash
git clone https://github.com/xiting910/MineClearance.git
cd MineClearance
dotnet run --project srcs/MineClearance.UI
```

### 构建

```bash
dotnet build
```

### 运行测试

```bash
dotnet test
```

### 发布

```bash
# Windows
dotnet publish srcs/MineClearance.UI -c Release -r win-x64

# Windows 还可以使用脚本
.\RePublish.bat

# Linux
dotnet publish srcs/MineClearance.UI -c Release -r linux-x64

# macOS
dotnet publish srcs/MineClearance.UI -c Release -r osx-x64
```

---

## 📄 许可证

本项目采用 [MIT License](LICENSE)。
