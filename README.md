<div align="center">

# 💣 MineClearance

🎮 基于 **Avalonia UI** 的跨平台扫雷游戏 — 免安装 · 存档恢复 · 无猜体验 · 种子复现

采用 **Clean Architecture** 架构，使用 **.NET 10.0** 构建，开箱即用、自动更新。

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![.NET 10.0](https://img.shields.io/badge/.NET-10.0-512BD4.svg)](https://dotnet.microsoft.com/)
[![Avalonia 12](https://img.shields.io/badge/Avalonia-12-8B45C6.svg)](https://avaloniaui.net/)
[![Release](https://img.shields.io/github/v/release/xiting910/MineClearance)](https://github.com/xiting910/MineClearance/releases)
[![Downloads](https://img.shields.io/github/downloads/xiting910/MineClearance/total)](https://github.com/xiting910/MineClearance/releases)

[![Windows x64](https://img.shields.io/badge/Windows-x64-0078D6?logo=windows&logoColor=white)](https://github.com/xiting910/MineClearance/releases)
[![Linux x64](https://img.shields.io/badge/Linux-x64-FCC624?logo=linux&logoColor=black)](https://github.com/xiting910/MineClearance/releases)
[![macOS x64](https://img.shields.io/badge/macOS-x64-000000?logo=apple&logoColor=white)](https://github.com/xiting910/MineClearance/releases)

[![CI](https://github.com/xiting910/MineClearance/actions/workflows/ci.yml/badge.svg)](https://github.com/xiting910/MineClearance/actions/workflows/ci.yml)
[![CodeQL](https://github.com/xiting910/MineClearance/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/xiting910/MineClearance/actions/workflows/codeql-analysis.yml)
[![Dependency Review](https://github.com/xiting910/MineClearance/actions/workflows/dependency-review.yml/badge.svg)](https://github.com/xiting910/MineClearance/actions/workflows/dependency-review.yml)

</div>

---

## ✨ 特性

### 🎮 游戏体验

- 🎮 **经典扫雷玩法** — 左键翻开, 数字格自动展开, 右键标旗/问号, 数字格一键插旗, 按住滑动连续操作
- ⚠️ **警告数字检测** — 周围旗数超过实际雷数时数字格实时变黄提示
- 🎯 **无猜体验** — 有条件挽救: 仅被迫猜测时约束重排雷位 (已揭示数字不变), 有必安全格或必死格时拒救判负
- 💡 **失败复盘** — 输局揭示地雷/插错旗/必安全格 (绿色 ✓), 让玩家清楚知道输在哪里
- 🎚️ **多级难度** — 初级/中级/高级/大师, 自定义行列/雷数/种子 (固定雷区可复现)
- 🔢 **格子索引** — 按住热键显示全部索引, 首次点击自动复制, 热键可录制
- ⏸️ **自动暂停** — 打开抽屉或窗口最小化/失焦时自动暂停游戏

### ⚙️ 便捷功能

- 👋 **首次启动提示** — 欢迎信息与操作指引, 展示后自动关闭
- 💾 **自动保存** — 关闭窗口或从游戏视图退出时自动保存进行中的游戏
- 📂 **存档恢复** — 主视图一键恢复上次进度 (雷位/已开格/计时续走), 单文件存档可备份分享
- 📊 **历史记录** — 按难度统计 (胜率/用时/完成度), 日期/难度/结果筛选, 列头排序, 删除选中/二次确认清空
- 🎨 **主题切换** — 跟随系统/浅色/深色, 即时生效并自动保存
- 🖼️ **背景图片** — 内置 3 张, 支持 Pictures 目录自定义图片 (拉伸/透明度可调, 新图手动刷新)
- ⚙️ **设置抽屉** — 主题/背景/Toast/日志级别/悬浮球即时配置, 手动检查更新, 清除更新缓存, 关于信息
- 🔄 **自动更新** — 启动后台检查 GitHub, 断点续传下载并校验, 进度悬浮球/详情抽屉, 失败自动回滚
- 🔁 **单实例运行** — 重复启动激活已有实例, Windows 被遮挡/最小化强制置前
- 🛡️ **异常处理** — 未处理异常写独立日志, UI 线程异常 Toast 提示日志位置

### 🔧 工程与质量

- 🧱 **Clean Architecture** — Core / Infrastructure / UI 分层, 高内聚低耦合
- 🧩 **MVVM 模式** — CommunityToolkit.Mvvm 源代码生成器
- 📝 **结构化日志** — ILogger + LoggerMessage 源生成器
- 🧪 **单元测试** — Core / Infrastructure / UI 三层 (xUnit v3 + Moq)
- 🔁 **CI/CD 自动化** — 自动构建、测试、CodeQL 分析、Release 发布
- 📦 **依赖自动更新** — Dependabot 分组策略

---

## 🛠️ 技术栈

| 类别    | 技术                                                  |
| ------- | ----------------------------------------------------- |
| 运行时  | .NET 10 · C# 14                                       |
| UI 框架 | Avalonia 12 · CommunityToolkit.Mvvm                   |
| 架构    | Clean Architecture · MVVM                             |
| 测试    | xUnit v3 · Moq · 318 个单元测试                       |
| 工程化  | GitHub Actions · CodeQL · Dependabot · CPM 集中包管理 |

---

## 🏗️ 项目结构

```
MineClearance/
├── .github/                                          # GitHub 配置
│   ├── ISSUE_TEMPLATE/                               #  Issue 模板
│   │   ├── bug_report.md                             #   Bug 报告模板
│   │   ├── config.yml                                #   Issue 模板选择配置
│   │   └── feature_request.md                        #   功能建议模板
│   ├── workflows/                                    #  GitHub Actions 工作流
│   │   ├── ci.yml                                    #   CI (构建/测试/上传测试结果)
│   │   ├── codeql-analysis.yml                       #   CodeQL 安全分析
│   │   ├── dependabot-auto-merge.yml                 #   Dependabot PR 自动 approve + squash 合并
│   │   ├── dependency-review.yml                     #   依赖漏洞审查
│   │   ├── release-delete.yml                        #   Release 清理
│   │   └── release-publish.yml                       #   Release 发布
│   ├── dependabot.yml                                #  Dependabot 依赖更新
│   └── PULL_REQUEST_TEMPLATE.md                      #  PR 描述模板
├── .vscode/                                          # VSCode 配置
│   └── settings.json                                 #  VSCode 设置
├── srcs/                                             # 源码目录
│   ├── MineClearance.Core/                           #  核心层
│   │   ├── Enums/                                    #   枚举定义
│   │   │   ├── CellType.cs                           #    格子类型
│   │   │   ├── GameDifficulty.cs                     #    游戏难度
│   │   │   └── GameStatus.cs                         #    游戏状态
│   │   ├── Interfaces/                               #   接口定义
│   │   │   ├── IGame.cs                              #    游戏核心接口
│   │   │   ├── IGameBoardDictionary.cs               #    棋盘格子字典接口
│   │   │   ├── IGameBoardDictionaryFactory.cs        #    棋盘字典工厂接口 (internal)
│   │   │   ├── IGameDataRepository.cs                #    游戏数据仓储接口
│   │   │   ├── IGameFactory.cs                       #    游戏工厂接口 (internal)
│   │   │   ├── IGameManager.cs                       #    游戏管理器接口
│   │   │   ├── IGameTimer.cs                         #    游戏计时器接口
│   │   │   ├── IMineField.cs                         #    地雷场接口 (internal)
│   │   │   ├── IMineGenerator.cs                     #    地雷生成器接口 (internal)
│   │   │   └── IMineSolver.cs                        #    地雷求解器接口 (internal)
│   │   ├── Models/                                   #   领域模型
│   │   │   ├── Records/                              #    记录类型
│   │   │   │   ├── GameConfig.cs                     #     游戏配置
│   │   │   │   ├── GameResult.cs                     #     游戏结果
│   │   │   │   ├── GameSaveData.cs                   #     游戏存档
│   │   │   │   └── Position.cs                       #     位置
│   │   │   └── Cell.cs                               #    游戏格子
│   │   ├── Services/                                 #   领域服务实现 (internal)
│   │   │   ├── Game.cs                               #    游戏核心实现
│   │   │   ├── Game.Logging.cs                       #    游戏日志实现
│   │   │   ├── Game.Private.cs                       #    游戏私有实现
│   │   │   ├── GameBoardDictionary.cs                #    棋盘字典实现
│   │   │   ├── GameBoardDictionaryFactory.cs         #    棋盘字典工厂实现
│   │   │   ├── GameFactory.cs                        #    游戏工厂实现
│   │   │   ├── GameManager.cs                        #    游戏管理器实现
│   │   │   ├── GameTimer.cs                          #    游戏计时器实现
│   │   │   ├── MineField.cs                          #    地雷场实现
│   │   │   ├── MineGenerator.cs                      #    地雷生成器实现
│   │   │   └── MineSolver.cs                         #    地雷求解器实现
│   │   ├── Constants.cs                              #   游戏常量
│   │   ├── EnumExtensions.cs                         #   枚举扩展
│   │   ├── IServiceCollectionExtensions.cs           #   DI 注册扩展
│   │   └── MineClearance.Core.csproj                 #   项目文件
│   ├── MineClearance.Infrastructure/                 #  基础设施层
│   │   ├── Models/                                   #   基础设施模型
│   │   │   ├── AtomicEnum.cs                         #    原子枚举操作封装
│   │   │   ├── FileLoggerOptions.cs                  #    文件日志选项
│   │   │   ├── UpdateInfo.cs                         #    更新信息记录
│   │   │   └── UpdateState.cs                        #    更新状态枚举
│   │   ├── Services/                                 #   服务实现
│   │   │   ├── FileLoggerProvider.cs                 #    文件日志提供程序实现
│   │   │   ├── GameDataRepository.Converter.cs       #    游戏数据仓储 Json 转换器
│   │   │   ├── GameDataRepository.cs                 #    游戏数据仓储实现
│   │   │   ├── GameDataRepository.Logging.cs         #    游戏数据仓储日志
│   │   │   ├── UpdateService.cs                      #    更新服务实现
│   │   │   ├── UpdateService.Logging.cs              #    更新服务日志
│   │   │   └── UpdateService.Private.cs              #    更新服务私有实现
│   │   ├── BootstrapUpdateHelper.cs                  #   引导更新辅助
│   │   ├── Constants.cs                              #   常量
│   │   ├── ILoggingBuilderExtensions.cs              #   日志构建器扩展
│   │   ├── IServiceCollectionExtensions.cs           #   DI 注册扩展
│   │   ├── IUpdateService.cs                         #   更新服务接口
│   │   ├── MineClearance.Infrastructure.csproj       #   项目文件
│   │   ├── SingleInstanceServer.cs                   #   单实例服务器
│   │   └── UnhandledExceptionHelper.cs               #   未处理异常辅助
│   └── MineClearance.UI/                             #  表示层
│       ├── Assets/                                   #   资源目录
│       │   ├── Backgrounds/                          #    内置背景图片
│       │   │   ├── 1.png                             #     图片 1
│       │   │   ├── 2.png                             #     图片 2
│       │   │   └── 3.png                             #     图片 3
│       │   └── logo.ico                              #    应用图标
│       ├── Models/                                   #   UI 模型
│       │   ├── BackgroundImageOption.cs              #    背景图片选项
│       │   ├── DifficultyFilterOption.cs             #    难度筛选选项
│       │   ├── GameResultRow.cs                      #    游戏结果行
│       │   ├── NavigationTarget.cs                   #    导航目标枚举
│       │   ├── ResultFilterOption.cs                 #    结果筛选选项
│       │   ├── SortKeys.cs                           #    统计表格排序键常量
│       │   ├── StatsRow.cs                           #    统计行
│       │   ├── StatsRowBuilder.cs                    #    统计行构建器
│       │   ├── ThemeMode.cs                          #    主题模式枚举
│       │   ├── ToastItem.cs                          #    Toast 提示条目
│       │   └── UIOptions.cs                          #    UI 配置
│       ├── ViewModels/                               #   视图模型
│       │   ├── CellViewModel.cs                      #    格子视图模型
│       │   ├── GameViewModel.cs                      #    游戏视图模型
│       │   ├── HistoryViewModel.cs                   #    历史记录视图模型
│       │   ├── MainViewModel.cs                      #    主视图模型
│       │   ├── SettingsViewModel.cs                  #    设置视图模型
│       │   ├── ShellViewModel.cs                     #    壳视图模型
│       │   ├── ToastViewModel.cs                     #    Toast 提示视图模型
│       │   └── UpdateViewModel.cs                    #    更新视图模型
│       ├── Views/                                    #   视图
│       │   ├── DownloadBallView.axaml                #    下载进度悬浮球视图
│       │   ├── DownloadBallView.axaml.cs             #    下载进度悬浮球视图代码后置
│       │   ├── DownloadDrawerView.axaml              #    下载详情抽屉视图
│       │   ├── DownloadDrawerView.axaml.cs           #    下载详情抽屉视图代码后置
│       │   ├── GameView.axaml                        #    游戏视图
│       │   ├── GameView.axaml.cs                     #    游戏视图代码后置
│       │   ├── HistoryView.axaml                     #    历史记录视图
│       │   ├── HistoryView.axaml.cs                  #    历史记录视图代码后置
│       │   ├── MainView.axaml                        #    主视图
│       │   ├── MainView.axaml.cs                     #    主视图代码后置
│       │   ├── SettingsView.axaml                    #    设置抽屉内容视图
│       │   ├── SettingsView.axaml.cs                 #    设置抽屉内容视图代码后置
│       │   ├── ShellView.axaml                       #    壳视图
│       │   ├── ShellView.axaml.cs                    #    壳视图代码后置
│       │   ├── ToastView.axaml                       #    Toast 视图
│       │   └── ToastView.axaml.cs                    #    Toast 视图代码后置
│       ├── App.axaml                                 #   应用定义
│       ├── App.axaml.cs                              #   应用类
│       ├── App.Manifest.xml                          #   Windows 应用清单
│       ├── AppMetadata.cs                            #   应用元数据
│       ├── Constants.cs                              #   UI 常量
│       ├── EnumDescriptionConverter.cs               #   枚举描述转换器
│       ├── KeyExtensions.cs                          #   Key 按键扩展
│       ├── MineClearance.UI.csproj                   #   项目文件
│       ├── Program.cs                                #   应用入口
│       ├── ShellWindow.axaml                         #   主窗口
│       ├── ShellWindow.axaml.cs                      #   主窗口代码
│       ├── ViewLocator.cs                            #   ViewModel → View 定位器
│       └── WindowsHelper.cs                          #   Windows 窗口操作辅助
├── tests/                                            # 测试项目目录
│   ├── MineClearance.Core.Tests/                     #  Core 层单元测试
│   │   ├── GameBoardDictionaryTests.cs               #   棋盘字典测试
│   │   ├── GameConfigTests.cs                        #   游戏配置测试
│   │   ├── GameManagerTests.cs                       #   游戏管理器测试
│   │   ├── GameResultTests.cs                        #   游戏结果测试
│   │   ├── GameSaveDataTests.cs                      #   游戏存档测试
│   │   ├── GameTests.cs                              #   游戏核心测试
│   │   ├── GameTimerTests.cs                         #   游戏计时器测试
│   │   ├── MineClearance.Core.Tests.csproj           #   测试项目文件
│   │   ├── MineFieldTests.cs                         #   地雷场测试
│   │   ├── MineSolverTests.cs                        #   地雷求解器测试
│   │   └── PositionTests.cs                          #   位置测试
│   ├── MineClearance.Infrastructure.Tests/           #  Infrastructure 层单元测试
│   │   ├── AssemblyInfo.cs                           #   程序集级配置
│   │   ├── BootstrapUpdateHelperTests.cs             #   引导更新辅助测试
│   │   ├── ConstantsTests.cs                         #   常量测试
│   │   ├── FileLoggerOptionsTests.cs                 #   文件日志选项测试
│   │   ├── FileLoggerProviderTests.cs                #   文件日志提供程序测试
│   │   ├── GameDataRepositoryTests.cs                #   游戏数据仓储测试
│   │   ├── MineClearance.Infrastructure.Tests.csproj #   测试项目文件
│   │   ├── ServiceRegistrationTests.cs               #   服务注册测试
│   │   ├── SingleInstanceServerTests.cs              #   单实例服务器测试
│   │   ├── TestEnvironmentFixture.cs                 #   测试环境夹具
│   │   └── UpdateServiceTests.cs                     #   更新服务测试
│   └── MineClearance.UI.Tests/                       #  UI 层单元测试
│       ├── AppMetadataTests.cs                       #   应用元数据测试
│       ├── AssemblyInfo.cs                           #   程序集级配置
│       ├── ConstantsTests.cs                         #   常量测试
│       ├── GameResultRowTests.cs                     #   游戏结果行测试
│       ├── HistoryViewModelTests.cs                  #   历史记录视图模型测试
│       ├── KeyExtensionsTests.cs                     #   按键扩展测试
│       ├── MainViewModelTests.cs                     #   主视图模型测试
│       ├── MineClearance.UI.Tests.csproj             #   测试项目文件
│       ├── SettingsViewModelTests.cs                 #   设置视图模型测试
│       ├── StatsRowBuilderTests.cs                   #   统计行构建器测试
│       ├── TestEnvironmentFixture.cs                 #   测试环境夹具
│       ├── ToastItemTests.cs                         #   Toast 条目测试
│       ├── UIOptionsTests.cs                         #   UI 配置测试
│       └── UpdateViewModelTests.cs                   #   更新视图模型测试
├── .editorconfig                                     # 代码风格统一配置
├── .gitattributes                                    # Git 行尾归一化, diff 策略与二进制标记
├── .gitignore                                        # 忽略规则
├── CHANGELOG.md                                      # 变更日志
├── Directory.Build.props                             # 全局构建属性
├── Directory.Packages.props                          # 集中包版本管理
├── global.json                                       # .NET 测试选项
├── LICENSE                                           # MIT 许可证
├── MineClearance.slnx                                # 解决方案文件
├── README.md                                         # 本文档
├── RePublish.bat                                     # Windows 清理构建脚本
└── TagPush.bat                                       # 打 tag 并推送发布脚本
```

依赖方向：`UI → Infrastructure → Core`（符合整洁架构依赖规则）

---

## 🚀 快速开始

> 💡 不想编译？直接前往 [Releases](https://github.com/xiting910/MineClearance/releases) 下载对应平台的免安装压缩包，解压即玩。

### 环境要求

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

### 克隆

```bash
git clone https://github.com/xiting910/MineClearance.git
```

### 运行

```bash
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

## 🤝 问题反馈与贡献

- 🐛 **反馈问题** — 遇到 Bug 或想提建议？前往 [Issues](https://github.com/xiting910/MineClearance/issues/new/choose) 提交（内置 Bug 报告 / 功能建议模板）
- 🚀 **贡献代码** — Fork 后提交 [PR](https://github.com/xiting910/MineClearance/pulls)，CI + CodeQL 自动守护
- 📖 **更新日志** — 版本演进记录见 [CHANGELOG.md](CHANGELOG.md)
- ⭐ **支持项目** — 觉得好用？点个 Star 支持一下～

---

## 📄 许可证

本项目采用 [MIT License](LICENSE)。
