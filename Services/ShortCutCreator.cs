#pragma warning disable SYSLIB1096
using System.Runtime.InteropServices;
using System.Text;

namespace MineClearance.Services;

/// <summary>
/// 快捷方式创建器类
/// </summary>
internal sealed partial class ShortcutCreator
{
    [ComImport]
    [Guid("00021401-0000-0000-C000-000000000046")]
    private class ShellLink { }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("0000010b-0000-0000-C000-000000000046")]
    private partial interface IPersistFile
    {
        void GetClassID(out Guid pClassID);
        void IsDirty();
        void Load([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, uint dwMode);
        void Save([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, [MarshalAs(UnmanagedType.VariantBool)] bool fRemember);
        void SaveCompleted([MarshalAs(UnmanagedType.LPWStr)] string pszFileName);
        void GetCurFile([MarshalAs(UnmanagedType.LPWStr)] out string ppszFileName);
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("000214F9-0000-0000-C000-000000000046")]
    private partial interface IShellLink
    {
        void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxPath, out IntPtr pfd, int fFlags);
        void GetIDList(out IntPtr ppidl);
        void SetIDList(IntPtr pidl);
        void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cchMaxName);
        void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
        void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cchMaxPath);
        void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
        void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cchMaxPath);
        void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
        void GetHotkey(out short pwHotkey);
        void SetHotkey(short wHotkey);
        void GetShowCmd(out int piShowCmd);
        void SetShowCmd(int iShowCmd);
        void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cchIconPath, out int piIcon);
        void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
        void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, int dwReserved);
        void Resolve(IntPtr hwnd, int fFlags);
        void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
    }

    /// <summary>
    /// 桌面路径
    /// </summary>
    private static readonly string DesktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

    /// <summary>
    /// 创建桌面快捷方式
    /// </summary>
    /// <param name="targetPath">目标程序路径</param>
    /// <param name="shortcutName">快捷方式名称（不带.lnk后缀）</param>
    /// <param name="arguments">启动参数</param>
    /// <param name="description">描述信息</param>
    /// <param name="iconPath">图标路径</param>
    /// <param name="iconIndex">图标索引</param>
    /// <exception cref="InvalidOperationException">如果创建快捷方式失败</exception>
    public static void CreateDesktopShortcut(string targetPath, string shortcutName, string arguments = "", string description = "", string? iconPath = null, int iconIndex = 0)
    {
        // 如果目标路径为空或无效, 则不创建快捷方式
        if (string.IsNullOrWhiteSpace(targetPath) || !File.Exists(targetPath))
        {
            return;
        }

        // 获取桌面快捷方式路径
        var shortcutPath = Path.Combine(DesktopPath, shortcutName + ".lnk");

        // 如果快捷方式已存在, 则删除
        if (File.Exists(shortcutPath))
        {
            File.Delete(shortcutPath);
            FileLogger.LogWarning($"删除旧快捷方式: {shortcutPath}");
        }

        // 创建ShellLink对象
        var shellLink = (IShellLink)new ShellLink();

        try
        {
            // 设置快捷方式属性
            shellLink.SetPath(targetPath);

            // 设置工作目录为目标程序所在目录
            var targetDirectory = Path.GetDirectoryName(targetPath) ?? throw new ArgumentException("目标路径无效", nameof(targetPath));
            shellLink.SetWorkingDirectory(targetDirectory);

            if (!string.IsNullOrEmpty(arguments))
            {
                shellLink.SetArguments(arguments);
            }

            if (!string.IsNullOrEmpty(description))
            {
                shellLink.SetDescription(description);
            }

            shellLink.SetIconLocation(string.IsNullOrEmpty(iconPath) ? targetPath : iconPath, iconIndex);

            // 保存快捷方式
            var persistFile = (IPersistFile)shellLink;
            persistFile.Save(shortcutPath, false);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("创建快捷方式失败: " + ex.Message);
        }
        finally
        {
            // 确保所有COM对象被释放
            _ = Marshal.ReleaseComObject(shellLink);
        }
    }
}