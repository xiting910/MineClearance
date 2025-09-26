using System.Reflection;
using System.Runtime.InteropServices;

namespace MineClearance.UI;

/// <summary>
/// UI 常量类, 提供一些 UI 相关的常量
/// </summary>
internal static partial class UIConstants
{
    /// <summary>
    /// 获取 ToolTip 实例
    /// </summary>
    public static ToolTip ToolTip => new()
    {
        UseFading = true,
        UseAnimation = true,
        IsBalloon = true,
        ShowAlways = true,
        InitialDelay = 300,
        ReshowDelay = 100
    };

    /// <summary>
    /// 程序名字
    /// </summary>
    public const string ProgramName = "扫雷游戏";

    /// <summary>
    /// 程序版本(可能为 null)
    /// </summary>
    public static readonly Version? ProgramVersion = Assembly.GetExecutingAssembly().GetName().Version;

    /// <summary>
    /// 主窗体的宽度
    /// </summary>
    public static int MainFormWidth => (int)(1264 * DpiScale);

    /// <summary>
    /// 主窗体的高度
    /// </summary>
    public static int MainFormHeight => (int)(854 * DpiScale);

    /// <summary>
    /// 网格大小
    /// </summary>
    public static int GridSize => (int)(25 * DpiScale);

    /// <summary>
    /// 设置窗体的最小宽度
    /// </summary>
    public static int SettingFormMinWidth => (int)(350 * DpiScale);

    /// <summary>
    /// 设置窗体的最小高度
    /// </summary>
    public static int SettingFormMinHeight => (int)(300 * DpiScale);

    /// <summary>
    /// DPI 缩放比例的字段
    /// </summary>
    private static float? _dpiScale;

    /// <summary>
    /// DPI 缩放比例, 如果未初始化则抛出异常
    /// </summary>
    public static float DpiScale => _dpiScale ?? throw new InvalidOperationException("DPI 未初始化");

    /// <summary>
    /// 初始化 DPI 缩放比例
    /// </summary>
    public static void InitDpiScale()
    {
        // Windows 10 Creators Update (1703) 及以上支持 GetDpiForSystem
        if (OperatingSystem.IsWindowsVersionAtLeast(10))
        {
            _dpiScale = GetDpiForSystem() / 96f;
        }
        else
        {
            // 兼容旧系统
            using var g = Graphics.FromHwnd(IntPtr.Zero);
            _dpiScale = g.DpiX / 96f;
        }
    }

    /// <summary>
    /// 获取系统 DPI
    /// </summary>
    /// <returns>系统 DPI</returns>
    [LibraryImport("user32.dll")]
    private static partial uint GetDpiForSystem();
}