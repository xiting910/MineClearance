using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using MineClearance.UI.Models;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;

namespace MineClearance.UI.ViewModels;

/// <summary>
/// 全局短暂提示视图模型, 供所有视图共用右下角 Toast 提示
/// </summary>
public sealed partial class ToastViewModel : ObservableObject
{
    /// <summary>
    /// 进度条刷新间隔, 用于平滑更新剩余时间进度条
    /// </summary>
    private static readonly TimeSpan RefreshInterval = TimeSpan.FromMilliseconds(
        Constants.ToastRefreshIntervalMilliseconds
    );

    /// <summary>
    /// 日志记录器
    /// </summary>
    private readonly ILogger<ToastViewModel> _logger;

    /// <summary>
    /// 进度条刷新计时器, 驱动所有条目的剩余时间扣减与进度条更新, 集合为空时停止
    /// </summary>
    private readonly DispatcherTimer _refreshTimer;

    /// <summary>
    /// 高精度计时器, 用于计算实际经过的时间
    /// </summary>
    private readonly Stopwatch _stopwatch;

    /// <summary>
    /// UI 配置, 每次显示时读取提示时长与最大条数
    /// </summary>
    private readonly UIOptions _uiOptions;

    /// <summary>
    /// 当前显示中的提示条目集合, 满员时新提示顶掉最早的一条
    /// </summary>
    public ObservableCollection<ToastItem> Items { get; } = [];

    /// <summary>
    /// 是否存在显示的提示条目, 供视图控制显隐
    /// </summary>
    public bool HasItems => Items.Count > 0;

    /// <summary>
    /// 构造函数, 注入日志记录器与 UI 配置
    /// </summary>
    /// <param name="logger">日志记录器</param>
    /// <param name="uiOptions">UI 配置</param>
    public ToastViewModel(ILogger<ToastViewModel> logger, UIOptions uiOptions)
    {
        _logger = logger;
        _stopwatch = new();
        _uiOptions = uiOptions;
        _refreshTimer = new(RefreshInterval, DispatcherPriority.Background, OnRefreshTimerTick);
        Items.CollectionChanged += OnItemsCollectionChanged;
    }

    /// <summary>
    /// 显示提示, 满员时顶掉最早的一条, 并启动计时器驱动进度条扣减
    /// </summary>
    /// <param name="message">提示文本</param>
    /// <param name="clickAction">点击回调</param>
    public void Show(string message, Action? clickAction = null)
    {
        // 获取显示时长
        var duration = TimeSpan.FromSeconds(_uiOptions.ToastDurationSeconds);

        // 显示时长小于等于零时不显示提示
        if (duration <= TimeSpan.Zero) { return; }

        // 获取最大条数
        var maxCount = _uiOptions.MaxToastCount;

        // 满员时顶掉最早的一条, 直到为新提示腾出位置
        while (Items.Count >= maxCount)
        {
            Items.RemoveAt(0);
        }

        // 创建并加入新条目
        var item = new ToastItem(message, duration, clickAction);
        Items.Add(item);

        // 下一帧触发入场动画, 确保起始状态先完成布局渲染再补间
        Dispatcher.UIThread.Post(() =>
        {
            item.EnterOffset = 0;
            item.EnterOpacity = Constants.MaxRatio;
        });

        // 启动计时器驱动进度条扣减
        _refreshTimer.Start();
        _stopwatch.Restart();

        // 记录日志
        LogToastShown(message);
    }

    /// <summary>
    /// 点击提示时由视图调用, 立即关闭该提示并执行其点击回调
    /// </summary>
    /// <param name="item">被点击的提示条目</param>
    public void InvokeClick(ToastItem item)
    {
        _ = Items.Remove(item);
        item.InvokeClick();
    }

    /// <summary>
    /// 定时刷新所有条目的剩余时间与进度条, 剩余时间耗尽的条目被移除
    /// </summary>
    /// <param name="sender">计时器</param>
    /// <param name="e">计时器事件参数</param>
    private void OnRefreshTimerTick(object? sender, EventArgs e)
    {
        // 计算实际经过的时间, 并重启计时器
        var delta = _stopwatch.Elapsed;
        _stopwatch.Restart();

        // 倒序驱动所有条目扣减剩余时间, 避免移除条目时索引错乱
        for (var i = Items.Count - 1; i >= 0; i--)
        {
            if (Items[i].Tick(delta))
            {
                Items.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// 提示条目集合变化时同步显隐状态, 集合为空时停止计时器
    /// </summary>
    /// <param name="sender">集合</param>
    /// <param name="e">集合变化事件参数</param>
    private void OnItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(HasItems));
        if (Items.Count == 0)
        {
            _refreshTimer.Stop();
            _stopwatch.Stop();
        }
    }

    /// <summary>
    /// 记录提示显示日志
    /// </summary>
    /// <param name="message">提示文本</param>
    [LoggerMessage(
        EventId = 1,
        EventName = "ToastShown",
        Level = LogLevel.Debug,
        Message = "Toast shown: {Message}"
    )]
    private partial void LogToastShown(string message);
}
