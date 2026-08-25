using System;
using System.Collections.Generic;
using System.Threading;

namespace MineClearance.Infrastructure.Models;

/// <summary>
/// 对枚举类型的原子操作封装结构体
/// </summary>
/// <typeparam name="TEnum">枚举类型</typeparam>
/// <param name="initialValue">初始值</param>
#pragma warning disable CA1711 // 标识符应采用正确的后缀
public struct AtomicEnum<TEnum>(TEnum initialValue) where TEnum : struct, Enum
#pragma warning restore CA1711 // 标识符应采用正确的后缀
{
    /// <summary>
    /// 枚举的值
    /// </summary>
    private TEnum _value = initialValue;

    /// <summary>
    /// 获取当前枚举值
    /// </summary>
    /// <remarks>
    /// 如果要进行 读取-修改-写入 的原子操作, 请改为使用以下方法:
    /// <list type="bullet">
    /// <item><see cref="SpinPredicateAndSet(Func{TEnum, bool}, TEnum, out TEnum)"/></item>
    /// <item><see cref="SpinPredicateAndSet(Func{TEnum, bool}, Func{TEnum}, out TEnum)"/></item>
    /// </list>
    /// </remarks>
    public TEnum Value => Interlocked.CompareExchange(ref _value, default, default);

    /// <summary>
    /// 将当前枚举值设置为指定的值
    /// </summary>
    /// <remarks>
    /// 如果要进行 读取-修改-写入 的原子操作, 请改为使用以下方法:
    /// <list type="bullet">
    /// <item><see cref="SpinPredicateAndSet(Func{TEnum, bool}, TEnum, out TEnum)"/></item>
    /// <item><see cref="SpinPredicateAndSet(Func{TEnum, bool}, Func{TEnum}, out TEnum)"/></item>
    /// </list>
    /// </remarks>
    /// <param name="newValue">要设置的新值</param>
    /// <returns><see langword="true"/> 如果当前值发生改变, 否则为 <see langword="false"/></returns>
    public bool Set(TEnum newValue)
    {
        var originalValue = Interlocked.Exchange(ref _value, newValue);
        return !EqualityComparer<TEnum>.Default.Equals(originalValue, newValue);
    }

    /// <summary>
    /// 使用自旋模式, 如果当前枚举值满足指定的谓词条件, 则将其设置为新值
    /// </summary>
    /// <param name="predicate">要检查的谓词条件</param>
    /// <param name="newValue">要设置的新值</param>
    /// <param name="previousValue">调用此方法时的旧值</param>
    /// <returns><see langword="true"/> 如果当前值发生改变, 否则为 <see langword="false"/></returns>
    public bool SpinPredicateAndSet(Func<TEnum, bool> predicate, TEnum newValue, out TEnum previousValue)
    {
        TEnum? previous = null;
        var spinWait = new SpinWait();
        while (true)
        {
            var currentValue = Value;
            previous ??= currentValue;
            if (EqualityComparer<TEnum>.Default.Equals(currentValue, newValue) || !predicate(currentValue))
            {
                previousValue = previous.Value;
                return false;
            }
            var originalValue = Interlocked.CompareExchange(ref _value, newValue, currentValue);
            if (EqualityComparer<TEnum>.Default.Equals(originalValue, currentValue))
            {
                previousValue = previous.Value;
                return true;
            }
            spinWait.SpinOnce();
        }
    }

    /// <summary>
    /// 使用自旋模式, 如果当前枚举值满足指定的谓词条件, 则将其设置为由指定函数生成的新值
    /// </summary>
    /// <param name="predicate">要检查的谓词条件</param>
    /// <param name="factory">用于生成新值的函数</param>
    /// <param name="newValue">调用此方法后的最终值</param>
    /// <returns><see langword="true"/> 如果当前值发生改变, 否则为 <see langword="false"/></returns>
    public bool SpinPredicateAndSet(Func<TEnum, bool> predicate, Func<TEnum> factory, out TEnum newValue)
    {
        var spinWait = new SpinWait();
        while (true)
        {
            var currentValue = Value;
            if (!predicate(currentValue))
            {
                newValue = currentValue;
                return false;
            }
            newValue = factory();
            if (EqualityComparer<TEnum>.Default.Equals(currentValue, newValue))
            {
                return false;
            }
            var originalValue = Interlocked.CompareExchange(ref _value, newValue, currentValue);
            if (EqualityComparer<TEnum>.Default.Equals(originalValue, currentValue))
            {
                return true;
            }
            spinWait.SpinOnce();
        }
    }
}
