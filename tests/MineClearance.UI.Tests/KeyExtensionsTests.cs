using Avalonia.Input;

namespace MineClearance.UI.Tests;

/// <summary>
/// <see cref="KeyExtensions"/> 的单元测试, 覆盖快捷键可用性判定
/// </summary>
public sealed class KeyExtensionsTests
{
    [Theory]
    [InlineData(Key.A)]
    [InlineData(Key.D1)]
    [InlineData(Key.F5)]
    [InlineData(Key.LeftShift)]
    [InlineData(Key.OemQuestion)]
    public void IsValidHotKey_普通按键_为true(Key key)
    {
        Assert.True(key.IsValidHotKey());
    }

    [Theory]
    [InlineData(Key.Enter)]
    [InlineData(Key.Space)]
    [InlineData(Key.Tab)]
    [InlineData(Key.Back)]
    [InlineData(Key.Delete)]
    [InlineData(Key.Escape)]
    [InlineData(Key.LWin)]
    [InlineData(Key.RWin)]
    [InlineData(Key.CapsLock)]
    [InlineData(Key.NumLock)]
    [InlineData(Key.Scroll)]
    [InlineData(Key.Pause)]
    [InlineData(Key.Sleep)]
    [InlineData(Key.PrintScreen)]
    [InlineData(Key.System)]
    [InlineData(Key.ImeProcessed)]
    [InlineData(Key.DeadCharProcessed)]
    public void IsValidHotKey_系统保留键_为false(Key key)
    {
        Assert.False(key.IsValidHotKey());
    }
}
