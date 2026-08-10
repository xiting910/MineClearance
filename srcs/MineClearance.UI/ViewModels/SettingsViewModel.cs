using Avalonia;
using Avalonia.Input;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using MineClearance.Infrastructure.Models;
using MineClearance.UI.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace MineClearance.UI.ViewModels;

/// <summary>
/// 设置视图模型, 负责主题/Toast 时长/日志级别配置与关于信息, 所有配置即时生效并自动保存
/// </summary>
public sealed partial class SettingsViewModel : ObservableObject
{
    /// <summary>
    /// 日志文件夹完整路径
    /// </summary>
    public static string LogsFolderPath => Path.Combine(
        Infrastructure.Constants.AppDataRootDirectory,
        Infrastructure.Constants.LogDirectoryName
    );

    /// <summary>
    /// UI 配置
    /// </summary>
    private readonly UIOptions _uiOptions;

    /// <summary>
    /// 文件日志记录器选项
    /// </summary>
    private readonly FileLoggerOptions _loggerOptions;

    /// <summary>
    /// 全局短暂提示视图模型, 用于操作失败时的反馈
    /// </summary>
    private readonly ToastViewModel _toast;

    /// <summary>
    /// 更新视图模型, 用于手动检查更新与悬浮球可见性联动
    /// </summary>
    private readonly UpdateViewModel _update;

    /// <summary>
    /// 主题模式
    /// </summary>
    [ObservableProperty]
    public partial ThemeMode Theme { get; set; }

    /// <summary>
    /// Toast 提示显示时间秒数
    /// </summary>
    [ObservableProperty]
    public partial double ToastDurationSeconds { get; set; }

    /// <summary>
    /// Toast 同时显示的最大条数
    /// </summary>
    [ObservableProperty]
    public partial int MaxToastCount { get; set; }

    /// <summary>
    /// 是否显示下载悬浮球
    /// </summary>
    [ObservableProperty]
    public partial bool ShowDownloadBall { get; set; }

    /// <summary>
    /// 日志级别
    /// </summary>
    [ObservableProperty]
    public partial LogLevel Level { get; set; }

    /// <summary>
    /// 首次点击格子时是否复制索引到剪贴板
    /// </summary>
    [ObservableProperty]
    public partial bool CopyIndexOnFirstClick { get; set; }

    /// <summary>
    /// 显示索引快捷键, Key.None 表示未设置
    /// </summary>
    [ObservableProperty]
    public partial Key ShowIndexHotKey { get; set; }

    /// <summary>
    /// 是否正在录制快捷键
    /// </summary>
    [ObservableProperty]
    public partial bool IsListeningHotkey { get; set; }

    /// <summary>
    /// 可选择的主题模式列表
    /// </summary>
    public IReadOnlyList<ThemeMode> Themes { get; } = Enum.GetValues<ThemeMode>();

    /// <summary>
    /// 可选择的日志级别列表
    /// </summary>
    public IReadOnlyList<LogLevel> Levels { get; } = Enum.GetValues<LogLevel>();

    /// <summary>
    /// 产品
    /// </summary>
    public string Product { get; }

    /// <summary>
    /// 版本号
    /// </summary>
    public string Version { get; }

    /// <summary>
    /// 作者
    /// </summary>
    public string Authors { get; }

    /// <summary>
    /// 许可证
    /// </summary>
    public string License { get; }

    /// <summary>
    /// GitHub 仓库地址
    /// </summary>
    public string GitHubUrl { get; }

    /// <summary>
    /// 热键按钮文本, 录制中为 &gt;键名&lt; 或 &gt;&lt;, 否则为键名或空
    /// </summary>
    public string HotkeyButtonText => IsListeningHotkey
        ? $">{ShowIndexHotKeyText}<"
        : ShowIndexHotKeyText;

    /// <summary>
    /// 快捷键显示文本, 未设置时为空字符串
    /// </summary>
    private string ShowIndexHotKeyText => ShowIndexHotKey is Key.None
        ? string.Empty
        : ShowIndexHotKey.ToString();

    /// <summary>
    /// 请求关闭设置抽屉的事件, 由壳视图模型处理
    /// </summary>
    public event Action? CloseRequested;

    /// <summary>
    /// 创建设置视图模型
    /// </summary>
    /// <param name="uiOptions">UI 配置</param>
    /// <param name="loggerOptions">文件日志记录器选项</param>
    /// <param name="toastViewModel">全局短暂提示视图模型</param>
    /// <param name="updateViewModel">更新视图模型</param>
    public SettingsViewModel(
        UIOptions uiOptions,
        FileLoggerOptions loggerOptions,
        ToastViewModel toastViewModel,
        UpdateViewModel updateViewModel)
    {
        _uiOptions = uiOptions;
        _loggerOptions = loggerOptions;
        _toast = toastViewModel;
        _update = updateViewModel;

        Theme = uiOptions.Theme;
        ToastDurationSeconds = uiOptions.ToastDurationSeconds;
        MaxToastCount = uiOptions.MaxToastCount;
        ShowDownloadBall = uiOptions.ShowDownloadBall;
        CopyIndexOnFirstClick = uiOptions.CopyIndexOnFirstClick;
        ShowIndexHotKey = uiOptions.ShowIndexHotKey;
        Level = loggerOptions.Level;

        Product = AppMetadata.Get(nameof(Product));
        Version = AppMetadata.Get(nameof(Version));
        Authors = AppMetadata.Get(nameof(Authors));
        License = AppMetadata.Get(nameof(License));
        GitHubUrl = AppMetadata.Get(nameof(GitHubUrl));
    }

    /// <summary>
    /// 主题变化时同步配置并即时切换应用主题
    /// </summary>
    /// <param name="value">新主题模式</param>
    partial void OnThemeChanged(ThemeMode value)
    {
        // 同步到 UI 配置并自动保存
        _uiOptions.Theme = value;

        // 即时切换主题
        Application.Current?.RequestedThemeVariant = value switch
        {
            ThemeMode.Light => ThemeVariant.Light,
            ThemeMode.Dark => ThemeVariant.Dark,
            _ => ThemeVariant.Default
        };
    }

    /// <summary>
    /// Toast 显示时间变化时同步配置
    /// </summary>
    /// <param name="value">新显示时间</param>
    partial void OnToastDurationSecondsChanged(double value)
    {
        _uiOptions.ToastDurationSeconds = value;
    }

    /// <summary>
    /// Toast 数量变化时同步配置
    /// </summary>
    /// <param name="value">新最大条数</param>
    partial void OnMaxToastCountChanged(int value)
    {
        _uiOptions.MaxToastCount = value;
    }

    /// <summary>
    /// 日志级别变化时同步配置
    /// </summary>
    /// <param name="value">新日志级别</param>
    partial void OnLevelChanged(LogLevel value)
    {
        _loggerOptions.Level = value;
    }

    /// <summary>
    /// 悬浮球可见性变化时同步配置并立即刷新悬浮球
    /// </summary>
    /// <param name="value">新的可见性</param>
    partial void OnShowDownloadBallChanged(bool value)
    {
        _uiOptions.ShowDownloadBall = value;
        _update.RefreshBallVisibility();
    }

    /// <summary>
    /// 首点复制索引开关变化时同步配置
    /// </summary>
    /// <param name="value">新的开关状态</param>
    partial void OnCopyIndexOnFirstClickChanged(bool value)
    {
        _uiOptions.CopyIndexOnFirstClick = value;
    }

    /// <summary>
    /// 显示索引快捷键变化时同步配置并刷新按钮文本
    /// </summary>
    /// <param name="value">新的快捷键</param>
    partial void OnShowIndexHotKeyChanged(Key value)
    {
        _uiOptions.ShowIndexHotKey = value;
        OnPropertyChanged(nameof(HotkeyButtonText));
    }

    /// <summary>
    /// 录制状态变化时刷新按钮文本
    /// </summary>
    /// <param name="value">新的录制状态</param>
    partial void OnIsListeningHotkeyChanged(bool value)
    {
        OnPropertyChanged(nameof(HotkeyButtonText));
    }

    /// <summary>
    /// 打开日志文件夹, 失败时通过 Toast 提示
    /// </summary>
    [RelayCommand]
    private void OpenLogsFolder()
    {
        try
        {
            _ = Process.Start(new ProcessStartInfo
            {
                FileName = LogsFolderPath,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            _toast.Show($"打开日志文件夹失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 清除更新缓存, 由设置抽屉的清除缓存按钮触发
    /// </summary>
    [RelayCommand]
    private void ClearUpdateCache()
    {
        _update.ClearUpdateCache();
    }

    /// <summary>
    /// 进入快捷键录制状态, 由设置抽屉的热键按钮触发
    /// </summary>
    [RelayCommand]
    private void BeginListenHotkey()
    {
        IsListeningHotkey = true;
    }

    /// <summary>
    /// 手动检查更新, 由设置抽屉的检查更新按钮触发
    /// </summary>
    [RelayCommand]
    private Task CheckForUpdatesAsync()
    {
        return _update.CheckForUpdatesAsync(manual: true);
    }

    /// <summary>
    /// 打开 GitHub 仓库地址, 由视图在点击链接时调用, 失败时通过 Toast 提示
    /// </summary>
    public void OpenGitHub()
    {
        try
        {
            _ = Process.Start(new ProcessStartInfo
            {
                FileName = GitHubUrl,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            _toast.Show($"打开链接失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 请求关闭设置抽屉
    /// </summary>
    public void RequestClose()
    {
        CloseRequested?.Invoke();
    }

    /// <summary>
    /// 取消快捷键录制, 不改变设置, 由视图在 Esc/鼠标点击时调用
    /// </summary>
    public void CancelHotkeyListening()
    {
        IsListeningHotkey = false;
    }

    /// <summary>
    /// 设置新的快捷键并退出录制, 由视图在监听到有效按键时调用
    /// </summary>
    /// <param name="key">新快捷键</param>
    public void CompleteHotkeyCapture(Key key)
    {
        ShowIndexHotKey = key;
        IsListeningHotkey = false;
    }

    /// <summary>
    /// 清除快捷键并退出录制, 由视图在监听到 Back/Delete 时调用
    /// </summary>
    public void ClearHotkey()
    {
        ShowIndexHotKey = Key.None;
        IsListeningHotkey = false;
    }

    /// <summary>
    /// 提示按键不能用作快捷键, 由视图在监听到无效按键时调用
    /// </summary>
    /// <param name="key">无效按键</param>
    public void NotifyDisallowedHotKey(Key key)
    {
        _toast.Show($"该按键 ({key}) 不能用作快捷键");
    }
}
