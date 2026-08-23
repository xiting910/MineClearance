using Avalonia.Input;
using Avalonia.Media;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MineClearance.Infrastructure;
using MineClearance.Infrastructure.Models;
using MineClearance.UI.Models;
using MineClearance.UI.ViewModels;
using Moq;

namespace MineClearance.UI.Tests;

/// <summary>
/// <see cref="SettingsViewModel"/> 的单元测试, 覆盖配置同步, 背景图片, 热键录制与关于信息
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
        _toast = new(NullLogger<ToastViewModel>.Instance, _uiOptions);
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
        Assert.Equal(_uiOptions.UseCustomBackgroundImage, _viewModel.UseCustomBackgroundImage);
        Assert.Equal(_uiOptions.BackgroundImageFileName, _viewModel.SelectedBackgroundImage.FileName);
        Assert.Equal(_uiOptions.BackgroundImageStretch, _viewModel.BackgroundImageStretch);
        Assert.Equal(_uiOptions.BackgroundImageOpacity, _viewModel.BackgroundImageOpacity);
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

    [Fact]
    public void 构造_默认使用内置背景_列表含不使用项和全部内置项()
    {
        Assert.False(_viewModel.UseCustomBackgroundImage);
        Assert.Equal(
            Constants.BuiltInBackgroundImageFileNames.Length + 1, _viewModel.BackgroundImages.Count
        );
        Assert.Equal("不使用背景图片", _viewModel.BackgroundImages[0].DisplayName);
        Assert.Equal(string.Empty, _viewModel.BackgroundImages[0].FileName);
        Assert.Equal(
            Constants.BuiltInBackgroundImageFileNames,
            _viewModel.BackgroundImages.Skip(1).Select(option => option.FileName)
        );
    }

    [Fact]
    public void 构造_使用自定义且目录不存在_仅含不使用项()
    {
        _uiOptions.UseCustomBackgroundImage = true;

        var viewModel = new SettingsViewModel(_uiOptions, _loggerOptions, _toast, _update);

        var option = Assert.Single(viewModel.BackgroundImages);
        Assert.Equal("不使用背景图片", option.DisplayName);
        Assert.Equal(string.Empty, option.FileName);
    }

    [Fact]
    public void 构造_自定义图片目录存在_识别图片并按名称排序()
    {
        var directory = Constants.CustomBackgroundImageDirectory;
        try
        {
            _ = Directory.CreateDirectory(directory);
            File.WriteAllText(Path.Combine(directory, "b.png"), "test");
            File.WriteAllText(Path.Combine(directory, "a.jpg"), "test");
            File.WriteAllText(Path.Combine(directory, "note.txt"), "test");
            _uiOptions.UseCustomBackgroundImage = true;

            var viewModel = new SettingsViewModel(_uiOptions, _loggerOptions, _toast, _update);

            Assert.Equal(
                ["a.jpg", "b.png"], viewModel.BackgroundImages.Skip(1).Select(option => option.FileName)
            );
        }
        finally
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, recursive: true);
            }
        }
    }

    [Fact]
    public void 构造_配置存在的自定义背景图片_选中对应选项()
    {
        var directory = Constants.CustomBackgroundImageDirectory;
        try
        {
            _ = Directory.CreateDirectory(directory);
            File.WriteAllText(Path.Combine(directory, "pic.png"), "test");
            _uiOptions.UseCustomBackgroundImage = true;
            _uiOptions.BackgroundImageFileName = "pic.png";

            var viewModel = new SettingsViewModel(_uiOptions, _loggerOptions, _toast, _update);

            Assert.Equal("pic.png", viewModel.SelectedBackgroundImage.FileName);
        }
        finally
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, recursive: true);
            }
        }
    }

    [Fact]
    public void 构造_配置的内置背景_选中对应选项()
    {
        _uiOptions.BackgroundImageFileName = "2.png";

        var viewModel = new SettingsViewModel(_uiOptions, _loggerOptions, _toast, _update);

        Assert.Equal("2.png", viewModel.SelectedBackgroundImage.FileName);
    }

    [Fact]
    public void 修改背景图片_同步配置并触发事件()
    {
        bool? changedUseCustom = null;
        string? changedFileName = null;
        _viewModel.BackgroundImageChanged += (useCustom, fileName) =>
        {
            changedUseCustom = useCustom;
            changedFileName = fileName;
        };

        _viewModel.SelectedBackgroundImage = new BackgroundImageOption("pic.png", "pic.png");

        Assert.False(changedUseCustom);
        Assert.Equal("pic.png", changedFileName);
        Assert.Equal("pic.png", _uiOptions.BackgroundImageFileName);
    }

    [Fact]
    public void 打开使用自定义开关_切换列表并同步配置()
    {
        bool? changedUseCustom = null;
        _viewModel.BackgroundImageChanged += (useCustom, _) => changedUseCustom = useCustom;

        _viewModel.UseCustomBackgroundImage = true;

        Assert.True(_uiOptions.UseCustomBackgroundImage);
        Assert.True(changedUseCustom);
        // 自定义目录不存在时仅含不使用项
        var option = Assert.Single(_viewModel.BackgroundImages);
        Assert.Equal("不使用背景图片", option.DisplayName);
    }

    [Fact]
    public void 关闭使用自定义开关_恢复内置列表()
    {
        _viewModel.UseCustomBackgroundImage = true;
        _viewModel.UseCustomBackgroundImage = false;

        Assert.False(_uiOptions.UseCustomBackgroundImage);
        Assert.Equal(
            Constants.BuiltInBackgroundImageFileNames.Length + 1, _viewModel.BackgroundImages.Count
        );
        Assert.Equal(
            Constants.BuiltInBackgroundImageFileNames,
            _viewModel.BackgroundImages.Skip(1).Select(option => option.FileName)
        );
    }

    [Fact]
    public void 修改背景拉伸_同步配置并触发事件()
    {
        var changedStretch = Stretch.None;
        _viewModel.BackgroundImageStretchChanged += value => changedStretch = value;

        _viewModel.BackgroundImageStretch = Stretch.Fill;

        Assert.Equal(Stretch.Fill, changedStretch);
        Assert.Equal(Stretch.Fill, _uiOptions.BackgroundImageStretch);
    }

    [Fact]
    public async Task 修改背景透明度_立即触发事件_延迟后保存配置()
    {
        var changedOpacity = -1.0;
        _viewModel.BackgroundImageOpacityChanged += value => changedOpacity = value;

        _viewModel.BackgroundImageOpacity = 0.5;

        // 事件立即触发以实时刷新显示, 配置在节流延迟后才保存
        Assert.Equal(0.5, changedOpacity);
        Assert.NotEqual(0.5, _uiOptions.BackgroundImageOpacity);

        await Task.Delay(Constants.OpacitySaveThrottleMilliseconds + 100, TestContext.Current.CancellationToken);

        Assert.Equal(0.5, _uiOptions.BackgroundImageOpacity);
    }

    [Fact]
    public async Task 修改背景透明度_节流期间再次修改_只保存最终值()
    {
        _viewModel.BackgroundImageOpacity = 0.2;
        _viewModel.BackgroundImageOpacity = 0.8;

        await Task.Delay(Constants.OpacitySaveThrottleMilliseconds + 100, TestContext.Current.CancellationToken);

        Assert.Equal(0.8, _uiOptions.BackgroundImageOpacity);
    }
}
