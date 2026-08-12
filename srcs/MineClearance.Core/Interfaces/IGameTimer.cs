using System;

namespace MineClearance.Core.Interfaces;

/// <summary>
/// 游戏计时器接口
/// </summary>
public interface IGameTimer
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
    /// 初始化计时器, 设置计时器的开始时间和已运行时间, 该方法会在游戏从存档中恢复时调用
    /// </summary>
    /// <param name="startTime">开始时间</param>
    /// <param name="elapsed">已用时间</param>
    void Initial(DateTime startTime, TimeSpan elapsed);

    /// <summary>
    /// 开始计时, 该方法不会重置计时器, 如果计时器已运行则不会有任何效果
    /// </summary>
    void Start();

    /// <summary>
    /// 暂停计时
    /// </summary>
    void Pause();
}
