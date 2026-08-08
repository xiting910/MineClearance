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
- ⚙️ **设置中心** — 主题 / Toast 时长 / 日志级别即时配置, 关于信息
- 🧱 **Clean Architecture** — 清晰的 Core / Infrastructure / UI 分层，高内聚低耦合
- 🧩 **MVVM 模式** — 基于 CommunityToolkit.Mvvm 源代码生成器
- 📝 **结构化日志** — ILogger + LoggerMessage 源代码生成器，记录游戏关键事件
- 🧪 **完善的测试** — xUnit + Moq 单元测试，coverlet 代码覆盖率
- 🔁 **CI/CD 自动化** — GitHub Actions 自动构建、测试、CodeQL 安全分析、Release 发布
- 📦 **依赖自动更新** — Dependabot 分组策略，保持依赖最新

---

## 🏗️ 项目结构

```
MineClearance/
├── .editorconfig                                   #   代码风格统一配置
├── .gitattributes                                  #   Git 行尾归一化 (默认 LF), diff 策略与二进制标记
├── .gitignore                                      #   忽略规则 (构建产物 / IDE / .NET / OS)
├── Directory.Build.props                           #   全局构建属性 (TargetFramework / Nullable / CPM)
├── Directory.Packages.props                        #   集中包版本管理 (NuGet CPM)
├── CHANGELOG.md                                    #   变更日志
├── LICENSE                                         #   MIT 许可证
├── MineClearance.slnx                              #   解决方案文件 (.NET XML 格式)
├── README.md                                       #   本文档
├── ReBuild.bat                                     #   Windows 清理构建脚本
├── srcs/
│   ├── MineClearance.Core/                         # 核心层 — 领域模型、接口、枚举、领域服务
│   │   ├── Constants.cs                            #   游戏常量
│   │   ├── EnumExtensions.cs                       #   枚举扩展 (Description 描述)
│   │   ├── Enums/                                  #   枚举定义
│   │   │   ├── CellType.cs                         #     格子类型
│   │   │   ├── GameDifficulty.cs                   #     游戏难度
│   │   │   └── GameStatus.cs                       #     游戏状态
│   │   ├── Interfaces/                             #   接口定义
│   │   │   ├── IGame.cs                            #     游戏核心接口 (INotifyPropertyChanged + IDisposable)
│   │   │   ├── IGameBoardDictionary.cs             #     棋盘格子字典接口 (IReadOnlyDictionary + INotifyPropertyChanged)
│   │   │   ├── IGameBoardDictionaryFactory.cs      #     棋盘字典工厂接口 (internal)
│   │   │   ├── IGameDataRepository.cs              #     游戏数据仓储接口
│   │   │   ├── IGameFactory.cs                     #     游戏工厂接口 (internal)
│   │   │   ├── IGameManager.cs                     #     游戏管理器接口
│   │   │   ├── IGameTimer.cs                       #     游戏计时器接口 (INotifyPropertyChanged)
│   │   │   ├── IMineField.cs                       #     地雷场接口 (internal)
│   │   │   ├── IMineGenerator.cs                   #     地雷生成器接口 (internal)
│   │   │   └── ISolvabilityChecker.cs              #     可解性检查器接口 (internal)
│   │   ├── Models/                                 #   领域模型
│   │   │   ├── Cell.cs                             #     游戏格子 (INotifyPropertyChanged)
│   │   │   └── Records/                            #     记录类型
│   │   │       ├── GameConfig.cs                   #       游戏配置
│   │   │       ├── GameResult.cs                   #       游戏结果
│   │   │       ├── GameSaveData.cs                 #       游戏存档
│   │   │       └── Position.cs                     #       位置
│   │   ├── Services/                               #   领域服务实现 (internal)
│   │   │   ├── Game.cs                             #     游戏核心实现
│   │   │   ├── Game.Logging.cs                     #     游戏日志 (LoggerMessage)
│   │   │   ├── Game.Private.cs                     #     游戏私有实现
│   │   │   ├── GameBoardDictionary.cs              #     棋盘字典实现
│   │   │   ├── GameBoardDictionaryFactory.cs       #     棋盘字典工厂实现
│   │   │   ├── GameFactory.cs                      #     游戏工厂实现
│   │   │   ├── GameManager.cs                      #     游戏管理器实现
│   │   │   ├── GameTimer.cs                        #     游戏计时器实现
│   │   │   ├── MineField.cs                        #     地雷场实现
│   │   │   ├── MineGenerator.cs                    #     地雷生成器实现 (含可解性检查)
│   │   │   └── SolvabilityChecker.cs               #     可解性检查器实现
│   │   └── IServiceCollectionExtensions.cs         # DI 注册扩展
│   ├── MineClearance.Infrastructure/               # 基础设施层 — 数据访问、外部服务实现
│   │   ├── Constants.cs                            #   常量 (数据目录, 文件路径, Json 选项)
│   │   ├── FileLoggerOptions.cs                    #   文件日志选项
│   │   ├── ILoggingBuilderExtensions.cs            #   日志构建器扩展 (AddFileLogger)
│   │   ├── IServiceCollectionExtensions.cs         #   DI 注册扩展 (AddInfrastructure)
│   │   └── Services/                               #   服务实现
│   │       ├── FileLoggerProvider.cs               #     文件日志提供程序实现
│   │       ├── GameDataRepository.cs               #     游戏数据仓储实现
│   │       ├── GameDataRepository.Converter.cs     #     游戏数据仓储 Json 转换器
│   │       └── GameDataRepository.Logging.cs       #     游戏数据仓储日志 (LoggerMessage)
│   └── MineClearance.UI/                           # 表示层 — Avalonia 桌面应用
│       ├── App.axaml                               #   应用定义 (主题/DataTemplate/颜色资源)
│       ├── App.axaml.cs                            #   应用类 (服务容器/主题应用/主窗口)
│       ├── Constants.cs                            #   UI 常量
│       ├── EnumDescriptionConverter.cs             #   枚举描述转换器 ([Description] → 文本)
│       ├── Models/                                 #   UI 模型
│       │   ├── DifficultyFilterOption.cs           #     难度筛选选项 (多选)
│       │   ├── GameResultRow.cs                    #     游戏结果行 (显示文本/棋盘尺寸)
│       │   ├── NavigationTarget.cs                 #     导航目标枚举
│       │   ├── ResultFilterOption.cs               #     结果筛选选项 (全部/胜利/失败)
│       │   ├── SortKeys.cs                         #     统计表格排序键常量
│       │   ├── StatsRow.cs                         #     统计行 (难度范围汇总统计)
│       │   ├── ThemeMode.cs                        #     主题模式枚举 (跟随系统/浅色/深色)
│       │   └── UIOptions.cs                        #     UI 配置 (setter 变化自动保存)
│       ├── Program.cs                              #   应用入口 (DI + Avalonia 启动)
│       ├── ViewLocator.cs                          #   ViewModel → View 定位器
│       ├── ViewModels/                             #   视图模型
│       │   ├── CellViewModel.cs                    #     格子视图模型 (固定格子池)
│       │   ├── GameViewModel.cs                    #     游戏视图模型 (固定格子池/交互分发)
│       │   ├── HistoryViewModel.cs                 #     历史记录视图模型 (统计/筛选/排序/删除)
│       │   ├── MainViewModel.cs                    #     主视图模型 (难度选择/参数输入/导航)
│       │   ├── SettingsViewModel.cs                #     设置视图模型
│       │   ├── ShellViewModel.cs                   #     壳视图模型 (视图切换/导航)
│       │   └── ToastViewModel.cs                   #     Toast 提示视图模型
│       └── Views/                                  #   视图
│           ├── GameView.axaml                      #     游戏视图
│           ├── HistoryView.axaml                   #     历史记录视图
│           ├── MainView.axaml                      #     主视图
│           ├── SettingsView.axaml                  #     设置视图
│           ├── SettingsWindow.axaml                #     设置窗口
│           ├── ShellView.axaml                     #     壳视图 (视图切换 + Toast 覆盖层)
│           ├── ShellWindow.axaml                   #     主窗口
│           └── ToastView.axaml                     #     Toast 视图
└── tests/
    ├── MineClearance.Core.Tests/                   # Core 层单元测试
    ├── MineClearance.Infrastructure.Tests/         # Infrastructure 层单元测试
    └── MineClearance.UI.Tests/                     # UI 层单元测试
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
# 完整清理构建
dotnet build

# 或使用清理脚本（仅 Windows）
.\ReBuild.bat
```

### 运行测试

```bash
dotnet test
```

### 发布

```bash
# Windows
dotnet publish srcs/MineClearance.UI -c Release -r win-x64

# Linux
dotnet publish srcs/MineClearance.UI -c Release -r linux-x64

# macOS
dotnet publish srcs/MineClearance.UI -c Release -r osx-x64
```

---

## 📄 许可证

本项目采用 [MIT License](LICENSE)。
