using Avalonia;
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

namespace MineClearance.UI.ViewModels;

/// <summary>
/// 设置视图模型, 负责主题/Toast 时长/日志级别配置与关于信息, 所有配置即时生效并自动保存
/// </summary>
public sealed partial class SettingsViewModel : ObservableObject
{
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
    /// 可选择的主题模式列表
    /// </summary>
    public IReadOnlyList<ThemeMode> Themes { get; } = Enum.GetValues<ThemeMode>();

    /// <summary>
    /// 可选择的日志级别列表
    /// </summary>
    public IReadOnlyList<LogLevel> Levels { get; } = Enum.GetValues<LogLevel>();

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
    /// 日志级别
    /// </summary>
    [ObservableProperty]
    public partial LogLevel Level { get; set; }

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
    /// 请求关闭设置抽屉的事件, 由壳视图模型处理
    /// </summary>
    public event Action? CloseRequested;

    /// <summary>
    /// 创建设置视图模型
    /// </summary>
    /// <param name="uiOptions">UI 配置</param>
    /// <param name="loggerOptions">文件日志记录器选项</param>
    /// <param name="toastViewModel">全局短暂提示视图模型</param>
    public SettingsViewModel(
        UIOptions uiOptions,
        FileLoggerOptions loggerOptions,
        ToastViewModel toastViewModel)
    {
        _uiOptions = uiOptions;
        _loggerOptions = loggerOptions;
        _toast = toastViewModel;

        Theme = uiOptions.Theme;
        ToastDurationSeconds = uiOptions.ToastDurationSeconds;
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
    /// 日志级别变化时同步配置
    /// </summary>
    /// <param name="value">新日志级别</param>
    partial void OnLevelChanged(LogLevel value)
    {
        _loggerOptions.Level = value;
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
                FileName = Path.Combine(
                    Infrastructure.Constants.AppDataRootDirectory,
                    Infrastructure.Constants.LogDirectoryName
                ),
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            _toast.Show($"打开日志文件夹失败: {ex.Message}");
        }
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
}
