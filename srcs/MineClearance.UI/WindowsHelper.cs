using Avalonia.Controls;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace MineClearance.UI;

/// <summary>
/// Windows 平台相关的窗口操作辅助类
/// </summary>
[SupportedOSPlatform("windows")]
public static partial class WindowsHelper
{
    /// <summary>
    /// 将窗口带到前台并获取键盘焦点, Windows 上绕过前台激活限制
    /// </summary>
    /// <param name="window">目标窗口</param>
    public static void BringToFront(Window window)
    {
        if (window.TryGetPlatformHandle() is not { } handle)
        {
            return;
        }

        var hwnd = handle.Handle;
        var foregroundHwnd = GetForegroundWindow();
        if (foregroundHwnd == hwnd)
        {
            _ = SetFocus(hwnd);
            return;
        }

        var foregroundThread = GetWindowThreadProcessId(foregroundHwnd, out _);
        var currentThread = GetCurrentThreadId();
        var attached = false;
        if (foregroundThread != 0 && foregroundThread != currentThread)
        {
            attached = AttachThreadInput(currentThread, foregroundThread, true);
        }

        var activated = SetForegroundWindow(hwnd);
        if (attached)
        {
            _ = AttachThreadInput(currentThread, foregroundThread, false);
        }

        if (activated)
        {
            _ = SetFocus(hwnd);
            return;
        }

        _ = SetWindowPos(
            hwnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE | SWP_SHOWWINDOW
        );
        _ = SetWindowPos(
            hwnd, HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE
        );
    }

    private const nint HWND_TOPMOST = -1;
    private const nint HWND_NOTOPMOST = -2;
    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOACTIVATE = 0x0010;
    private const uint SWP_SHOWWINDOW = 0x0040;
    private const string User32Dll = "user32.dll";
    private const string Kernel32Dll = "kernel32.dll";

    [LibraryImport(User32Dll)]
    private static partial nint GetForegroundWindow();

    [LibraryImport(User32Dll)]
    private static partial uint GetWindowThreadProcessId(nint hWnd, out uint processId);

    [LibraryImport(Kernel32Dll)]
    private static partial uint GetCurrentThreadId();

    [LibraryImport(User32Dll)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool AttachThreadInput(
        uint idAttach, uint idAttachTo, [MarshalAs(UnmanagedType.Bool)] bool fAttach
    );

    [LibraryImport(User32Dll)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetForegroundWindow(nint hWnd);

    [LibraryImport(User32Dll)]
    private static partial nint SetFocus(nint hWnd);

    [LibraryImport(User32Dll, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetWindowPos(
        nint hWnd, nint hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags
    );
}
