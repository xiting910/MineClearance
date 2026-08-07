# Changelog

本文件记录了项目的所有重要变更。每个版本的变更都应在发布时记录在此文件中。

格式基于 [Keep a Changelog](https://keepachangelog.com/zh-CN/1.1.0/),
版本号遵循 [Semantic Versioning](https://semver.org/lang/zh-CN/).

---

## [Unreleased]

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

### Changed

- UI 层: 日志轮转改为仅最新日志文件存在且非空时执行
- Core 层: IGameDataRepository 异步方法改为同步属性 (SaveData, GameResults), 移除 HasGameSaveData 与 GetGameSaveDataAsync / GetGameResultsAsync
- Core 层: IGameManager.RestoreFromSaveDataAsync 改为同步方法 RestoreFromSaveData

[Unreleased]: https://github.com/xiting910/MineClearance/commits/main
