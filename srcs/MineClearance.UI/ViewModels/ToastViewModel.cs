using CommunityToolkit.Mvvm.ComponentModel;
using MineClearance.UI.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MineClearance.UI.ViewModels;

/// <summary>
/// 全局短暂提示视图模型, 供所有视图共用右下角 Toast 提示
/// </summary>
/// <param name="_uiOptions">UI 配置</param>
#pragma warning disable CA1707
public sealed partial class ToastViewModel(UIOptions _uiOptions) : ObservableObject, IDisposable
#pragma warning restore CA1707
{
    /// <summary>
    /// 用于延迟隐藏短暂提示的取消令牌源
    /// </summary>
    private CancellationTokenSource? _feedbackCts;

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

    /// <inheritdoc/>
    public void Dispose()
    {
        _feedbackCts?.Cancel();
        _feedbackCts?.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 显示短暂提示后消失, 新提示会取代旧提示, 显示时长每次从配置读取
    /// </summary>
    /// <param name="message">提示文本</param>
    public void Show(string message)
    {
        _feedbackCts?.Cancel();
        _feedbackCts?.Dispose();
        _feedbackCts = new();
        Feedback = message;
        FeedbackVisible = true;
        _ = ClearFeedbackAfterAsync(_feedbackCts, TimeSpan.FromSeconds(_uiOptions.ToastDurationSeconds));
    }

    /// <summary>
    /// 延迟隐藏短暂提示
    /// </summary>
    /// <param name="cts">本次提示对应的取消令牌源</param>
    /// <param name="delay">提示显示持续时间</param>
    private async Task ClearFeedbackAfterAsync(CancellationTokenSource cts, TimeSpan delay)
    {
        try
        {
            await Task.Delay(delay, cts.Token);
            if (ReferenceEquals(_feedbackCts, cts))
            {
                FeedbackVisible = false;
                Feedback = string.Empty;
            }
        }
        catch (OperationCanceledException) { }
    }
}
