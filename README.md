# 💣 MineClearance

一个基于 **Avalonia UI** 的跨平台扫雷游戏，采用 **Clean Architecture** 架构，使用 **.NET 10.0** 构建。

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![CI](https://github.com/xiting910/MineClearance/actions/workflows/ci.yml/badge.svg)](https://github.com/xiting910/MineClearance/actions/workflows/ci.yml)
[![CodeQL](https://github.com/xiting910/MineClearance/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/xiting910/MineClearance/actions/workflows/codeql-analysis.yml)
[![Dependency Review](https://github.com/xiting910/MineClearance/actions/workflows/dependency-review.yml/badge.svg)](https://github.com/xiting910/MineClearance/actions/workflows/dependency-review.yml)

---

## ✨ 特性

- 🖥️ **跨平台支持** — Windows / Linux / macOS，单文件自包含发布
- 🧱 **Clean Architecture** — 清晰的 Core / Infrastructure / UI 分层，高内聚低耦合
- 🧩 **MVVM 模式** — 基于 CommunityToolkit.Mvvm 源代码生成器
- 🧪 **完善的测试** — xUnit + Moq 单元测试，coverlet 代码覆盖率
- 🔁 **CI/CD 自动化** — GitHub Actions 自动构建、测试、CodeQL 安全分析、Release 发布
- 📦 **依赖自动更新** — Dependabot 分组策略，保持依赖最新

---

## 🏗️ 项目结构

```
MineClearance/
├── src/
│   ├── MineClearance.Core/                         # 核心层 — 领域模型、接口、枚举、业务服务
│   │   ├── Constants.cs                            #   游戏常量定义
│   │   ├── Enums/                                  #   枚举定义 (4 个枚举)
│   │   │   ├── CellType.cs                         #     格子类型
│   │   │   ├── GameDifficulty.cs                   #     游戏难度
│   │   │   ├── GameStatus.cs                       #     游戏状态
│   │   │   └── HintStrategyType.cs                 #     提示策略
│   │   ├── Interfaces/                             #   接口定义 (8 个接口)
│   │   │   ├── IGame.cs                            #     游戏核心接口
│   │   │   ├── IGameBoard.cs                       #     游戏棋盘接口
│   │   │   ├── IGameManager.cs                     #     游戏管理器接口
│   │   │   ├── IGameResultRepository.cs            #     游戏结果仓储接口
│   │   │   ├── IGameTimer.cs                       #     游戏计时器接口
│   │   │   ├── IHintProvider.cs                    #     提示提供者接口
│   │   │   ├── IMineField.cs                       #     地雷场接口
│   │   │   └── IMineGenerator.cs                   #     地雷生成器接口 (internal)
│   │   ├── Models/                                 #   领域模型
│   │   │   ├── Cell.cs                             #     游戏格子 (含 INotifyPropertyChanged)
│   │   │   ├── Args/                               #     事件参数类 (4 个)
│   │   │   │   ├── GameChangedEventArgs.cs         #       游戏变更事件
│   │   │   │   ├── GameStatusChangedEventArgs.cs   #       状态变更事件
│   │   │   │   ├── GameTimerTickEventArgs.cs       #       计时器滴答事件
│   │   │   │   └── MineFieldGeneratedEventArgs.cs  #       地雷场生成事件
│   │   │   └── Records/                            #     记录类型 (Position, GameConfig 等)
│   │   ├── Services/                               #   领域服务
│   │   └── IServiceCollectionExtensions.cs         # DI 注册扩展
│   ├── MineClearance.Infrastructure/               # 基础设施层 — 数据访问、外部服务实现
│   └── MineClearance.UI/                           # 表示层 — Avalonia 桌面应用
│       └── Program.cs                              #   应用入口
└── tests/
    ├── MineClearance.Core.Tests/                   # Core 层单元测试
    └── MineClearance.Infrastructure.Tests/         # Infrastructure 层单元测试
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
dotnet run --project src/MineClearance.UI
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
# 发布为单文件自包含应用（Windows）
dotnet publish src/MineClearance.UI -c Release -r win-x64

# Linux
dotnet publish src/MineClearance.UI -c Release -r linux-x64

# macOS
dotnet publish src/MineClearance.UI -c Release -r osx-x64
```

---

## 📄 许可证

本项目采用 [MIT License](LICENSE)。
