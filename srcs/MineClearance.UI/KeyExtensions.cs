using Avalonia.Input;
using System.Collections.Generic;

namespace MineClearance.UI;

/// <summary>
/// <see cref="Key"/> 枚举的扩展方法
/// </summary>
public static class KeyExtensions
{
    /// <summary>
    /// 不允许的按键哈希表
    /// </summary>
    private static readonly HashSet<Key> InvalidKeys = [
        // Avalonia 内部保留键: 用于内部处理, 应用收不到按键事件
        Key.Enter, Key.Return, Key.Space, Key.Tab,
        // 功能键: 录制模式拦截, 系统保留, 应用收不到按键事件
        Key.Back, Key.Delete, Key.Escape,
        // Windows 键: 切换系统状态, 应用收不到按键事件
        Key.LWin, Key.RWin,
        // 状态锁定键: 切换系统状态, NumLock 在 Windows 上不产生按键事件
        Key.CapsLock, Key.NumLock, Key.Scroll,
        // 系统拦截键: 系统保留/电源/截图, 应用收不到按键事件
        Key.Pause, Key.Sleep, Key.PrintScreen,
        // 内部虚拟键: IME/系统键掩码, 运行时不会以普通按键事件出现
        Key.System, Key.ImeProcessed, Key.DeadCharProcessed
    ];

    /// <summary>
    /// <see cref="Key"/> 枚举的扩展
    /// </summary>
    /// <param name="key">按键</param>
    extension(Key key)
    {
        /// <summary>
        /// 判断按键是否可作为快捷键
        /// </summary>
        /// <returns><see langword="true"/> 如果按键可作为快捷键, 否则 <see langword="false"/></returns>
        public bool IsValidHotKey()
        {
            return !InvalidKeys.Contains(key);
        }
    }
}
