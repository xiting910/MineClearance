using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using MineClearance.UI.Models;
using System;

namespace MineClearance.UI.ViewModels;

/// <summary>
/// 全局短暂提示视图模型, 供所有视图共用右下角 Toast 提示
/// </summary>
public sealed partial class ToastViewModel : ObservableObject
{
    /// <summary>
    /// 最大时间比例
    /// </summary>
    private const double MaxProgress = 1.0;

    /// <summary>
    /// 进度条刷新间隔, 用于平滑更新剩余时间进度条
    /// </summary>
    private static readonly TimeSpan RefreshInterval = TimeSpan.FromMilliseconds(50);

    /// <summary>
    /// UI 配置, 每次显示时读取提示时长
    /// </summary>
    private readonly UIOptions _uiOptions;

    /// <summary>
    /// 进度条刷新计时器, 驱动剩余时间扣减与进度条更新
    /// </summary>
    private readonly DispatcherTimer _refreshTimer;

    /// <summary>
    /// 上一次刷新计时的时间点, 用于计算实际经过的时间
    /// </summary>
    private DateTime _lastTickTime;

    /// <summary>
    /// 剩余显示时间
    /// </summary>
    private TimeSpan _remaining;

    /// <summary>
    /// 是否因鼠标悬停而暂停倒计时
    /// </summary>
    private bool _isPaused;

    /// <summary>
    /// 短暂提示文本
    /// </summary>
    [ObservableProperty]
    public partial string Feedback { get; set; } = string.Empty;

    /// <summary>
    /// 短暂提示是否可见
    /// </summary>
    [ObservableProperty]
    public partial bool FeedbackVisible { get; set; }

    /// <summary>
    /// 剩余显示时间比例 (0-1), 驱动底部进度条从满宽缩至零
    /// </summary>
    [ObservableProperty]
    public partial double Progress { get; set; } = 1.0;

    /// <summary>
    /// 创建短暂提示视图模型
    /// </summary>
    /// <param name="uiOptions">UI 配置</param>
    public ToastViewModel(UIOptions uiOptions)
    {
        _uiOptions = uiOptions;
        _refreshTimer = new(RefreshInterval, DispatcherPriority.Background, OnRefreshTimerTick);
    }

    /// <summary>
    /// 显示短暂提示后消失, 新提示会取代旧提示, 显示时长每次从配置读取
    /// </summary>
    /// <param name="message">提示文本</param>
    public void Show(string message)
    {
        Feedback = message;
        Progress = MaxProgress;

        // 显示时长
        var duration = TimeSpan.FromSeconds(_uiOptions.ToastDurationSeconds);
        if (duration <= TimeSpan.Zero)
        {
            FeedbackVisible = false;
            return;
        }

        // 显示提示
        FeedbackVisible = true;

        // 重置状态
        _lastTickTime = DateTime.Now;
        _remaining = duration;
        _isPaused = false;

        // 启动计时器, 以便在每次刷新时扣减剩余时间与更新进度条
        _refreshTimer.Start();
    }

    /// <summary>
    /// 鼠标悬停在提示上时暂停倒计时
    /// </summary>
    public void Pause()
    {
        _isPaused = true;
    }

    /// <summary>
    /// 鼠标移开提示后恢复倒计时
    /// </summary>
    public void Resume()
    {
        _isPaused = false;
    }

    /// <summary>
    /// 定时刷新剩余时间与进度条, 剩余时间为零时隐藏提示
    /// </summary>
    /// <param name="sender">计时器</param>
    /// <param name="e">计时器事件参数</param>
    private void OnRefreshTimerTick(object? sender, EventArgs e)
    {
        var now = DateTime.Now;
        var delta = now - _lastTickTime;
        _lastTickTime = now;

        // 悬停暂停期间不扣减剩余时间, 进度条保持不动
        if (!_isPaused)
        {
            _remaining -= delta;
        }

        // 计算剩余时间比例, 用于驱动进度条从满宽缩至零
        Progress = Math.Clamp(
            _remaining / TimeSpan.FromSeconds(_uiOptions.ToastDurationSeconds),
            0.0, MaxProgress
        );

        // 剩余时间为零时隐藏提示并停止计时
        if (_remaining <= TimeSpan.Zero)
        {
            FeedbackVisible = false;
            Feedback = string.Empty;
            _refreshTimer.Stop();
        }
    }
}
