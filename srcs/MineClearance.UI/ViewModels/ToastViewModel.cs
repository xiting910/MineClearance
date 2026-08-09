using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using MineClearance.UI.Models;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

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
    /// 进度条刷新计时器, 驱动所有条目的剩余时间扣减与进度条更新, 集合为空时停止
    /// </summary>
    private readonly DispatcherTimer _refreshTimer;

    /// <summary>
    /// UI 配置, 每次显示时读取提示时长与最大条数
    /// </summary>
    private readonly UIOptions _uiOptions;

    /// <summary>
    /// 上一次刷新计时的时间点, 用于计算实际经过的时间
    /// </summary>
    private DateTime _lastTickTime;

    /// <summary>
    /// 当前显示中的提示条目集合, 满员时新提示顶掉最早的一条
    /// </summary>
    public ObservableCollection<ToastItem> Items { get; } = [];

    /// <summary>
    /// 是否存在显示的提示条目, 供视图控制显隐
    /// </summary>
    public bool HasItems => Items.Count > 0;

    /// <summary>
    /// 创建短暂提示视图模型
    /// </summary>
    /// <param name="uiOptions">UI 配置</param>
    public ToastViewModel(UIOptions uiOptions)
    {
        _uiOptions = uiOptions;
        _refreshTimer = new(RefreshInterval, DispatcherPriority.Background, OnRefreshTimerTick);
        Items.CollectionChanged += OnItemsCollectionChanged;
    }

    /// <summary>
    /// 显示短暂提示后消失, 满员时顶掉最早的一条, 显示时长与最大条数每次从配置读取
    /// </summary>
    /// <param name="message">提示文本</param>
    /// <param name="clickAction">点击回调</param>
    public void Show(string message, Action? clickAction = null)
    {
        // 显示时长为零时忽略提示
        var duration = TimeSpan.FromSeconds(_uiOptions.ToastDurationSeconds);
        if (duration <= TimeSpan.Zero) { return; }

        // 满员时顶掉最早的一条, 为新提示腾出位置
        while (Items.Count >= _uiOptions.MaxToastCount)
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
            item.EnterOpacity = ToastItem.MaxOpacity;
        });

        // 启动计时器驱动进度条扣减
        _lastTickTime = DateTime.Now;
        _refreshTimer.Start();
    }

    /// <summary>
    /// 点击提示时由视图调用, 立即关闭该提示并执行其点击回调
    /// </summary>
    /// <param name="item">被点击的提示条目</param>
    public void InvokeClick(ToastItem item)
    {
        item.InvokeClick();
        _ = Items.Remove(item);
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
        }
    }

    /// <summary>
    /// 定时刷新所有条目的剩余时间与进度条, 剩余时间耗尽的条目被移除
    /// </summary>
    /// <param name="sender">计时器</param>
    /// <param name="e">计时器事件参数</param>
    private void OnRefreshTimerTick(object? sender, EventArgs e)
    {
        var now = DateTime.Now;
        var delta = now - _lastTickTime;
        _lastTickTime = now;

        // 倒序驱动所有条目扣减剩余时间, 耗尽的条目直接移除避免索引错位
        for (var i = Items.Count - 1; i >= 0; i--)
        {
            if (Items[i].Tick(delta))
            {
                Items.RemoveAt(i);
            }
        }
    }
}
