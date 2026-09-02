using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace MineClearance.UI.Models;

/// <summary>
/// 单条 Toast 提示项, 含提示文本/剩余时间进度/悬停暂停状态与点击回调
/// </summary>
/// <param name="feedback">提示文本</param>
/// <param name="duration">显示时长</param>
/// <param name="clickAction">点击回调</param>
public sealed partial class ToastItem(
    string feedback,
    TimeSpan duration,
    Action? clickAction = null
) : ObservableObject
{
    /// <summary>
    /// 提示文本
    /// </summary>
    [ObservableProperty]
    public partial string Feedback { get; set; } = feedback;

    /// <summary>
    /// 入场位移偏移 (像素), 新提示从下方滑入时由初始值过渡到 0
    /// </summary>
    [ObservableProperty]
    public partial double EnterOffset { get; set; } = Constants.ToastEnterOffset;

    /// <summary>
    /// 入场透明度, 新提示淡入时由 0 过渡到 1
    /// </summary>
    [ObservableProperty]
    public partial double EnterOpacity { get; set; }

    /// <summary>
    /// 剩余显示时间比例 (0-1), 驱动底部进度条从满宽缩至零
    /// </summary>
    [ObservableProperty]
    public partial double Progress { get; set; } = Constants.MaxRatio;

    /// <summary>
    /// 是否因鼠标悬停而暂停倒计时
    /// </summary>
    [ObservableProperty]
    public partial bool IsPaused { get; set; }

    /// <summary>
    /// 点击回调, 点击提示时执行
    /// </summary>
    private readonly Action? _clickAction = clickAction;

    /// <summary>
    /// 总显示时长, 用于计算剩余时间比例
    /// </summary>
    private readonly TimeSpan _totalDuration = duration;

    /// <summary>
    /// 剩余显示时间
    /// </summary>
    private TimeSpan _remaining = duration;

    /// <summary>
    /// 鼠标悬停在提示上时暂停倒计时
    /// </summary>
    public void Pause()
    {
        IsPaused = true;
    }

    /// <summary>
    /// 鼠标移开提示后恢复倒计时
    /// </summary>
    public void Resume()
    {
        IsPaused = false;
    }

    /// <summary>
    /// 按经过时间扣减剩余时间并更新进度条
    /// </summary>
    /// <param name="delta">距上次刷新的时间间隔</param>
    /// <returns><see langword="true"/> 如果剩余时间耗尽, 否则 <see langword="false"/></returns>
    public bool Tick(TimeSpan delta)
    {
        if (!IsPaused)
        {
            _remaining -= delta;
            Progress = Math.Clamp(_remaining / _totalDuration, 0, Constants.MaxRatio);
        }
        return _remaining <= TimeSpan.Zero;
    }

    /// <summary>
    /// 执行点击回调
    /// </summary>
    public void InvokeClick()
    {
        try
        {
            _clickAction?.Invoke();
        }
        catch { /* 忽略回调异常 */ }
    }
}
