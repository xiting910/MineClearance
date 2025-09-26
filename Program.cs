using MineClearance.Services;
using MineClearance.UI;
using MineClearance.UI.Main;

namespace MineClearance;

/// <summary>
/// 主程序类
/// </summary>
file static class Program
{
    /// <summary>
    /// 程序唯一标识符
    /// </summary>
    private const string AppId = "Local\\MineClearance_xiting910";

    /// <summary>
    /// 全局互斥体，保证单实例
    /// </summary>
    private static Mutex? _mutex;

    /// <summary>
    /// 未知异常类
    /// </summary>
    /// <param name="message">异常消息</param>
    private sealed class UnknownException(string message) : Exception(message);

    /// <summary>
    /// 程序入口点
    /// </summary>
    [STAThread]
    private static void Main()
    {
        // 检测是否为Windows操作系统
        if (!OperatingSystem.IsWindows())
        {
            // 如果不是Windows操作系统, 则显示错误信息并退出程序
            _ = MessageBox.Show("本程序仅支持在Windows操作系统上运行", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        // 设置未处理异常模式为捕获异常
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

        // 绑定未捕获异常事件
        Application.ThreadException += OnThreadException;
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

        // 设置当前线程的UI文化为简体中文
        Thread.CurrentThread.CurrentUICulture = new("zh-CN");

        // 初始化应用程序配置
        ApplicationConfiguration.Initialize();

        // 初始化 DPI 缩放
        UIConstants.InitDpiScale();

        try
        {
            // 保证只运行一个实例
            _mutex = new(true, AppId, out var isNewInstance);
            if (!isNewInstance)
            {
                _ = MessageBox.Show("程序已在运行中！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 初始化日志记录器
            FileLogger.Initialize();

            // 记录程序启动信息
            FileLogger.LogInfo("程序正在启动...");

            // 确保注册表中不存在无效项
            AutoStartHelper.EnsureValidRegistryEntries();

            // 初始化数据
            Datas.Initialize().GetAwaiter().GetResult();

            // 显示主窗口
            Application.Run(MainForm.Instance);

            // 记录程序关闭信息
            FileLogger.LogInfo("程序正在关闭...");

            // 关闭日志记录器
            FileLogger.ShutdownAsync().GetAwaiter().GetResult();
        }
        finally
        {
            // 只有获得互斥体时才释放
            if (_mutex is not null)
            {
                try
                {
                    _mutex.ReleaseMutex();
                }
                catch (ApplicationException) { }
                _mutex.Dispose();
                _mutex = null;
            }
        }
    }

    /// <summary>
    /// 处理未处理的线程异常
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="e">线程异常事件参数</param>
    private static void OnThreadException(object sender, ThreadExceptionEventArgs e)
    {
        // 记录异常到日志文件并弹窗提示错误信息
        FileLogger.LogException(e.Exception);
        _ = MessageBox.Show($"发生未处理的线程异常: {e.Exception.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);

        // 退出应用程序
        Application.Exit();
    }

    /// <summary>
    /// 处理未处理的应用程序异常
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="e">未处理异常事件参数</param>
    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        // 记录异常到日志文件并弹窗提示错误信息
        if (e.ExceptionObject is Exception ex)
        {
            FileLogger.LogException(ex);
            _ = MessageBox.Show($"发生未处理的应用程序异常: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        else
        {
            FileLogger.LogException(new UnknownException("发生未知的未处理异常"));
            _ = MessageBox.Show($"发生未知的未处理异常", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        // 退出应用程序
        Application.Exit();
    }
}