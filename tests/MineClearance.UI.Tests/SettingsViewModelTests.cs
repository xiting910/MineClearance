using Avalonia.Input;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MineClearance.Infrastructure;
using MineClearance.Infrastructure.Models;
using MineClearance.UI.Models;
using MineClearance.UI.ViewModels;
using Moq;

namespace MineClearance.UI.Tests;

/// <summary>
/// <see cref="SettingsViewModel"/> 的单元测试, 覆盖配置同步, 热键录制与关于信息
/// </summary>
public sealed class SettingsViewModelTests
{
    /// <summary>
    /// UI 配置实例
    /// </summary>
    private readonly UIOptions _uiOptions;

    /// <summary>
    /// 文件日志记录器选项实例
    /// </summary>
    private readonly FileLoggerOptions _loggerOptions;

    /// <summary>
    /// 全局短暂提示视图模型
    /// </summary>
    private readonly ToastViewModel _toast;

    /// <summary>
    /// 更新视图模型
    /// </summary>
    private readonly UpdateViewModel _update;

    /// <summary>
    /// 设置视图模型实例
    /// </summary>
    private readonly SettingsViewModel _viewModel;

    /// <summary>
    /// 初始化配置实例与设置视图模型
    /// </summary>
    public SettingsViewModelTests()
    {
        _uiOptions = new(new ConfigurationBuilder().Build());
        _loggerOptions = new(new ConfigurationBuilder().Build());
        _toast = new(_uiOptions);
        _update = new(new Mock<IUpdateService>().Object, _toast, _uiOptions);
        _viewModel = new(_uiOptions, _loggerOptions, _toast, _update);
    }

    [Fact]
    public void 构造_属性从配置复制()
    {
        Assert.Equal(_uiOptions.Theme, _viewModel.Theme);
        Assert.Equal(_uiOptions.ToastDurationSeconds, _viewModel.ToastDurationSeconds);
        Assert.Equal(_uiOptions.MaxToastCount, _viewModel.MaxToastCount);
        Assert.Equal(_uiOptions.ShowDownloadBall, _viewModel.ShowDownloadBall);
        Assert.Equal(_uiOptions.CopyIndexOnFirstClick, _viewModel.CopyIndexOnFirstClick);
        Assert.Equal(_uiOptions.ShowIndexHotKey, _viewModel.ShowIndexHotKey);
        Assert.Equal(_loggerOptions.Level, _viewModel.Level);
    }

    [Fact]
    public void 构造_关于信息非空()
    {
        Assert.NotEmpty(_viewModel.Product);
        Assert.NotEmpty(_viewModel.Version);
        Assert.NotEmpty(_viewModel.Authors);
        Assert.NotEmpty(_viewModel.License);
        Assert.NotEmpty(_viewModel.GitHubUrl);
    }

    [Fact]
    public void 构造_未设置热键_按钮文本为空()
    {
        Assert.Equal(string.Empty, _viewModel.HotkeyButtonText);
    }

    [Fact]
    public void 修改主题_同步到UI配置()
    {
        _viewModel.Theme = ThemeMode.Dark;

        Assert.Equal(ThemeMode.Dark, _uiOptions.Theme);
    }

    [Fact]
    public void 修改Toast时长_同步到UI配置()
    {
        _viewModel.ToastDurationSeconds = 3;

        Assert.Equal(3, _uiOptions.ToastDurationSeconds);
    }

    [Fact]
    public void 修改Toast条数_同步到UI配置()
    {
        _viewModel.MaxToastCount = 4;

        Assert.Equal(4, _uiOptions.MaxToastCount);
    }

    [Fact]
    public void 修改日志级别_同步到日志选项()
    {
        _viewModel.Level = LogLevel.Debug;

        Assert.Equal(LogLevel.Debug, _loggerOptions.Level);
    }

    [Fact]
    public void 修改首点复制开关_同步到UI配置()
    {
        _viewModel.CopyIndexOnFirstClick = true;

        Assert.True(_uiOptions.CopyIndexOnFirstClick);
    }

    [Fact]
    public void 修改快捷键_同步到UI配置并刷新按钮文本()
    {
        _viewModel.CompleteHotkeyCapture(Key.F8);

        Assert.Equal(Key.F8, _uiOptions.ShowIndexHotKey);
        Assert.Equal("F8", _viewModel.HotkeyButtonText);
        Assert.False(_viewModel.IsListeningHotkey);
    }

    [Fact]
    public void 开始录制_按钮文本切换为录制样式()
    {
        _viewModel.BeginListenHotkeyCommand.Execute(null);

        Assert.True(_viewModel.IsListeningHotkey);
        Assert.Equal("><", _viewModel.HotkeyButtonText);
    }

    [Fact]
    public void 取消录制_不改变设置()
    {
        _viewModel.BeginListenHotkeyCommand.Execute(null);

        _viewModel.CancelHotkeyListening();

        Assert.False(_viewModel.IsListeningHotkey);
        Assert.Equal(string.Empty, _viewModel.HotkeyButtonText);
    }

    [Fact]
    public void 清除快捷键_置为None并退出录制()
    {
        _viewModel.CompleteHotkeyCapture(Key.F8);

        _viewModel.ClearHotkey();

        Assert.Equal(Key.None, _viewModel.ShowIndexHotKey);
        Assert.Equal(string.Empty, _viewModel.HotkeyButtonText);
        Assert.False(_viewModel.IsListeningHotkey);
    }

    [Fact]
    public void 提示无效按键_弹出Toast提示()
    {
        _viewModel.NotifyDisallowedHotKey(Key.Escape);

        Assert.NotEmpty(_toast.Items);
    }

    [Fact]
    public void 请求关闭_触发关闭事件()
    {
        var closed = false;
        _viewModel.CloseRequested += () => closed = true;

        _viewModel.RequestClose();

        Assert.True(closed);
    }

    [Fact]
    public void 修改悬浮球可见性_同步到UI配置()
    {
        _viewModel.ShowDownloadBall = false;

        Assert.False(_uiOptions.ShowDownloadBall);
    }
}
