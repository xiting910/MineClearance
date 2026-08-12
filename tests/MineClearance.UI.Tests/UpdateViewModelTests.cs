using Microsoft.Extensions.Configuration;
using MineClearance.Infrastructure;
using MineClearance.Infrastructure.Models;
using MineClearance.UI.Models;
using MineClearance.UI.ViewModels;
using Moq;

namespace MineClearance.UI.Tests;

/// <summary>
/// <see cref="UpdateViewModel"/> 的单元测试, 覆盖检查更新反馈, 下载抽屉与悬浮球可见性
/// </summary>
public sealed class UpdateViewModelTests
{
    /// <summary>
    /// 更新服务模拟
    /// </summary>
    private readonly Mock<IUpdateService> _updateService = new();

    /// <summary>
    /// UI 配置实例
    /// </summary>
    private readonly UIOptions _uiOptions;

    /// <summary>
    /// 全局短暂提示视图模型
    /// </summary>
    private readonly ToastViewModel _toast;

    /// <summary>
    /// 更新视图模型实例
    /// </summary>
    private readonly UpdateViewModel _viewModel;

    /// <summary>
    /// 初始化模拟与更新视图模型
    /// </summary>
    public UpdateViewModelTests()
    {
        _uiOptions = new(new ConfigurationBuilder().Build());
        _toast = new(_uiOptions);
        // 检查请求默认立即完成, 各测试按需覆盖 State 等属性
        _ = _updateService
            .Setup(s =>
                s.CheckNewestAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()
                )
            ).Returns(Task.CompletedTask);
        _viewModel = new(_updateService.Object, _toast, _uiOptions);
    }

    [Fact]
    public void 构造_初始状态_抽屉关闭且悬浮球不可见()
    {
        Assert.False(_viewModel.IsBallVisible);
        Assert.False(_viewModel.IsDrawerOpen);
        Assert.False(_viewModel.IsDrawerVisible);
        Assert.False(_viewModel.IsFailed);
        Assert.False(_viewModel.IsCancelVisible);
    }

    [Fact]
    public async Task 检查更新_手动且已是最新_提示已是最新()
    {
        _ = _updateService.SetupGet(s => s.State).Returns(UpdateState.UpToDate);

        await _viewModel.CheckForUpdatesAsync(manual: true);

        Assert.Contains(_toast.Items, static item => item.Feedback.Contains("已是最新"));
    }

    [Fact]
    public async Task 检查更新_自动且已是最新_不提示()
    {
        _ = _updateService.SetupGet(s => s.State).Returns(UpdateState.UpToDate);

        await _viewModel.CheckForUpdatesAsync(manual: false);

        Assert.Empty(_toast.Items);
    }

    [Fact]
    public async Task 检查更新_检查进行中_提示后台检查且不发起新请求()
    {
        _ = _updateService.SetupGet(s => s.State).Returns(UpdateState.Checking);

        await _viewModel.CheckForUpdatesAsync(manual: true);

        _updateService.Verify(
            s => s.CheckNewestAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        Assert.Contains(_toast.Items, static item => item.Feedback.Contains("已有更新检查"));
    }

    [Fact]
    public async Task 检查更新_下载进行中_提示稍后再检查()
    {
        _ = _updateService.SetupGet(s => s.State).Returns(UpdateState.Downloading);

        await _viewModel.CheckForUpdatesAsync(manual: true);

        Assert.Contains(_toast.Items, static item => item.Feedback.Contains("请稍候再检查"));
    }

    [Fact]
    public async Task 检查更新_手动且检查失败_提示失败原因()
    {
        _ = _updateService.SetupGet(s => s.State).Returns(UpdateState.CheckFailed);
        _ = _updateService.SetupGet(s => s.Exception).Returns(new InvalidOperationException("网络错误"));

        await _viewModel.CheckForUpdatesAsync(manual: true);

        Assert.Contains(_toast.Items, static item => item.Feedback.Contains("检查更新失败"));
    }

    [Fact]
    public async Task 检查更新_需要更新且前一状态非检查中_不提示()
    {
        _ = _updateService.SetupGet(s => s.State).Returns(UpdateState.NeedUpdate);

        await _viewModel.CheckForUpdatesAsync(manual: false);

        Assert.Empty(_toast.Items);
    }

    [Fact]
    public void 刷新悬浮球_未下载_始终不可见()
    {
        _ = _updateService.SetupGet(s => s.State).Returns(UpdateState.Idle);
        _uiOptions.ShowDownloadBall = true;

        _viewModel.RefreshBallVisibility();

        Assert.False(_viewModel.IsBallVisible);
    }

    [Fact]
    public void 刷新悬浮球_下载中且允许显示_悬浮球可见()
    {
        _ = _updateService.SetupGet(s => s.State).Returns(UpdateState.Downloading);
        _uiOptions.ShowDownloadBall = true;

        _viewModel.RefreshBallVisibility();

        Assert.True(_viewModel.IsBallVisible);
    }

    [Fact]
    public void 刷新悬浮球_下载中但禁止显示_弹出下载抽屉()
    {
        _ = _updateService.SetupGet(s => s.State).Returns(UpdateState.Downloading);
        _uiOptions.ShowDownloadBall = false;

        _viewModel.RefreshBallVisibility();

        Assert.False(_viewModel.IsBallVisible);
        Assert.True(_viewModel.IsDrawerOpen);
        Assert.True(_viewModel.IsDrawerVisible);
    }

    [Fact]
    public void 切换抽屉_关闭状态时打开()
    {
        _viewModel.ToggleDrawer();

        Assert.True(_viewModel.IsDrawerOpen);
        Assert.True(_viewModel.IsDrawerVisible);
        Assert.Equal(0, _viewModel.DrawerSlideOffset);
    }

    [Fact]
    public void 关闭抽屉_动画结束后隐藏()
    {
        _viewModel.ToggleDrawer();

        _viewModel.CloseDrawer();

        Assert.False(_viewModel.IsDrawerOpen);
        Assert.Equal(-Constants.DrawerWidth, _viewModel.DrawerSlideOffset);
    }

    [Fact]
    public async Task 关闭抽屉_等待动画时长_抽屉隐藏()
    {
        _viewModel.ToggleDrawer();

        _viewModel.CloseDrawer();
        await Task.Delay(
            Constants.DrawerAnimationDurationMilliseconds + 100, TestContext.Current.CancellationToken
        );

        Assert.False(_viewModel.IsDrawerVisible);
    }

    [Fact]
    public async Task 关闭抽屉_动画期间重新打开_不隐藏()
    {
        _viewModel.ToggleDrawer();

        _viewModel.CloseDrawer();
        _viewModel.ToggleDrawer();
        await Task.Delay(
            Constants.DrawerAnimationDurationMilliseconds + 100, TestContext.Current.CancellationToken
        );

        Assert.True(_viewModel.IsDrawerVisible);
    }

    [Fact]
    public void 取消下载_调用更新服务()
    {
        _viewModel.CancelDownloadCommand.Execute(null);

        _updateService.Verify(s => s.CancelDownload(), Times.Once);
    }

    [Fact]
    public void 清除更新缓存_缓存目录存在_删除并提示()
    {
        _ = Directory.CreateDirectory(Infrastructure.Constants.UpdateDataDirectory);
        File.WriteAllText(Infrastructure.Constants.UpdatePackageFilePath, "data");

        _viewModel.ClearUpdateCache();

        Assert.False(Directory.Exists(Infrastructure.Constants.UpdateDataDirectory));
        Assert.Contains(_toast.Items, static item => item.Feedback.Contains("更新缓存已清除"));
    }

    [Fact]
    public void 清除更新缓存_缓存目录不存在_仅提示()
    {
        _viewModel.ClearUpdateCache();

        Assert.Contains(_toast.Items, static item => item.Feedback.Contains("更新缓存已清除"));
    }

    [Fact]
    public void 启动更新流程_首次启动_显示欢迎提示并关闭首次启动开关()
    {
        _uiOptions.ShowFirstLaunchTip = true;
        _ = _updateService.Setup(s => s.GetLastUpdateInfoAndCleanUp()).Returns((UpdateInfo?)null);

        _viewModel.StartUpdateRoutine();

        Assert.Contains(_toast.Items, static item => item.Feedback.Contains("欢迎使用"));
        Assert.False(_uiOptions.ShowFirstLaunchTip);
    }
}
