<div align="center">

# 💣 MineClearance

[English](README.en.md) | **简体中文**

基于 **Avalonia UI** 的跨平台扫雷游戏 — 免安装 · 存档恢复 · 无猜体验 · 种子复现

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

## 📸 界面预览

<div align="center">

<img src="docs/images/Main.webp" width="49%" alt="主界面与设置抽屉" />
<img src="docs/images/InGame.webp" width="49%" alt="对局中" />

<img src="docs/images/History.webp" width="49%" alt="浅色主题 · 历史记录" />
<img src="docs/images/DarkTheme.webp" width="49%" alt="深色主题 · 历史记录" />

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

- 📐 **交互式架构图** — [Architecture.html](docs/Architecture.html)
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
| 测试    | xUnit v3 · Moq · 388 个单元测试                       |
| 工程化  | GitHub Actions · CodeQL · Dependabot · CPM 集中包管理 |

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

---

## 🤝 问题反馈与贡献

- 🐛 **反馈问题** — 遇到 Bug 或想提建议？前往 [Issues](https://github.com/xiting910/MineClearance/issues/new/choose) 提交（内置 Bug 报告 / 功能建议模板）
- 🚀 **贡献代码** — Fork 后提交 [PR](https://github.com/xiting910/MineClearance/pulls)，CI + CodeQL 自动守护
- 📖 **更新日志** — 版本演进记录见 [CHANGELOG.md](CHANGELOG.md)
- ⭐ **支持项目** — 觉得好用？点个 Star 支持一下～

---

## 📄 许可证

本项目采用 [MIT License](LICENSE)。
