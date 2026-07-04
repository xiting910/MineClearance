using System;
using System.ComponentModel;

namespace MineClearance.Core.Interfaces;

/// <summary>
/// 游戏计时器接口
/// </summary>
public interface IGameTimer : INotifyPropertyChanged
{
    /// <summary>
    /// 获取计时器第一次开始计时的时间, 如果计时器从未开始过, 则返回 <see langword="null"/>
    /// </summary>
    DateTime? FirstStartTime { get; }

    /// <summary>
    /// 获取计时器已运行的时间
    /// </summary>
    TimeSpan Elapsed { get; }

    /// <summary>
    /// 获取计时器是否正在运行
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// 刷新计时器, 该方法会触发 <see cref="Elapsed"/> 属性的 <see cref="INotifyPropertyChanged.PropertyChanged"/>
    /// 事件, 以便通知 UI 更新显示的时间
    /// </summary>
    void Refresh();

    /// <summary>
    /// 暂停计时
    /// </summary>
    void Pause();

    /// <summary>
    /// 开始计时, 该方法不会重置计时器, 如果计时器已运行则不会有任何效果
    /// </summary>
    void Start();

    /// <summary>
    /// 重新开始计时, 如果计时器已运行则会重置计时器并重新开始计时
    /// </summary>
    void ReStart();

    /// <summary>
    /// 设置计时器初始时间
    /// </summary>
    /// <param name="initialTime">初始时间</param>
    void SetInitialTime(TimeSpan initialTime);
}
