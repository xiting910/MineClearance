using MineClearance.Models.Enums;
using MineClearance.Utilities;
using System.Collections.Concurrent;
using System.Globalization;

namespace MineClearance.Services;

/// <summary>
/// 文件日志记录器
/// </summary>
internal static class FileLogger
{
    /// <summary>
    /// 当前日志文件路径
    /// </summary>
    private static readonly string LogFilePath = Path.Combine(Constants.LogFolderPath, "Latest.log");

    /// <summary>
    /// 日志队列
    /// </summary>
    private static readonly ConcurrentQueue<string> _logQueue = new();

    /// <summary>
    /// 取消令牌源, 用于取消日志写入任务
    /// </summary>
    private static readonly CancellationTokenSource _cts = new();

    /// <summary>
    /// 后台任务是否正在运行
    /// </summary>
    private static volatile bool _isProcessing;

    /// <summary>
    /// 是否已经停止写入日志
    /// </summary>
    private static volatile bool _isStopped;

    /// <summary>
    /// 初始化日志记录器
    /// </summary>
    public static void Initialize()
    {
        // 如果日志文件夹不存在, 则创建
        if (!Directory.Exists(Constants.LogFolderPath))
        {
            _ = Directory.CreateDirectory(Constants.LogFolderPath);
        }

        // 删除过期的日志文件
        DeleteExpiredLogFiles();

        // 重命名旧的最新日志文件
        RenameLatestLogFile();
    }

    /// <summary>
    /// 记录信息
    /// </summary>
    /// <param name="message">日志消息</param>
    public static void LogInfo(string? message) => Log(LogLevel.Info, message);

    /// <summary>
    /// 记录警告
    /// </summary>
    /// <param name="message">日志消息</param>
    public static void LogWarning(string? message) => Log(LogLevel.Warning, message);

    /// <summary>
    /// 记录异常
    /// </summary>
    /// <param name="exception">异常对象</param>
    public static void LogException(Exception exception) => Log(LogLevel.Error, exception.ToString());

    /// <summary>
    /// 结束所有日志写入任务并重命名日志文件
    /// </summary>
    public static async Task ShutdownAsync()
    {
        // 设置停止标志, 防止新日志写入
        _isStopped = true;

        // 等待后台任务完成的任务
        var task = Task.Run(async () =>
        {
            while (_isProcessing && !_logQueue.IsEmpty)
            {
                await Task.Delay(100);
            }
        });

        try
        {
            // 等待后台任务完成
            await task.WaitAsync(TimeSpan.FromMilliseconds(LogConfigManager.Config.MaxLogWaitTime));
        }
        catch (TimeoutException)
        {
            // 超时, 取消后台任务
            _cts.Cancel();
        }

        // 重命名日志文件
        RenameLatestLogFile();
    }

    /// <summary>
    /// 删除过期的日志文件
    /// </summary>
    private static void DeleteExpiredLogFiles()
    {
        var retentionDays = LogConfigManager.Config.MaxLogFileRetentionDays;
        var expirationUtc = DateTime.UtcNow.AddDays(-retentionDays);

        foreach (var path in Directory.EnumerateFiles(Constants.LogFolderPath, "*.log"))
        {
            try
            {
                var lastWriteUtc = File.GetLastWriteTimeUtc(path);
                if (lastWriteUtc < expirationUtc)
                {
                    File.Delete(path);
                }
            }
            catch { /* 忽略 */ }
        }
    }

    /// <summary>
    /// 记录日志
    /// </summary>
    /// <param name="level">日志级别</param>
    /// <param name="message">日志消息</param>
    private static void Log(LogLevel level, string? message)
    {
        // 如果消息为空或者已经停止写入, 则跳过
        if (string.IsNullOrWhiteSpace(message) || _isStopped)
        {
            return;
        }

        // 如果日志级别低于最低级别, 则跳过
        if (level < LogConfigManager.Config.MinLogLevel)
        {
            return;
        }

        // 要记录的日志
        var log = $"[{DateTime.Now}] [{level}] {message}{Environment.NewLine}";

        // 添加到队列
        _logQueue.Enqueue(log);

        // 如果后台任务未启动且未停止, 则启动
        if (!_isProcessing && !_isStopped)
        {
            _isProcessing = true;
            _ = ProcessLogs(_cts.Token);
        }
    }

    /// <summary>
    /// 后台处理日志队列
    /// </summary>
    /// <param name="cts">取消令牌</param>
    private static async Task ProcessLogs(CancellationToken cts)
    {
        while (_logQueue.TryDequeue(out var log))
        {
            if (!cts.IsCancellationRequested)
            {
                await File.AppendAllTextAsync(LogFilePath, log, cts);
            }
        }

        // 处理完后重置标志
        _isProcessing = false;
    }

    /// <summary>
    /// 重命名最新日志文件
    /// </summary>
    private static void RenameLatestLogFile()
    {
        if (!File.Exists(LogFilePath))
        {
            // 如果文件不存在, 跳过
            return;
        }

        // 获取今天的日期
        var today = DateTime.Now.ToString("yyyy_MM_dd", CultureInfo.InvariantCulture);

        // 获取当天已有的日志文件中的最大序号
        var max = Directory.EnumerateFiles(Constants.LogFolderPath, $"{today}-*.log")
            .Select(path => Path.GetFileNameWithoutExtension(path).Split('-', 2))
            .Where(parts => parts.Length > 1 && int.TryParse(parts[1], out _))
            .Select(parts => int.Parse(parts[1], CultureInfo.InvariantCulture))
            .Where(num => num >= 0)
            .DefaultIfEmpty()
            .Max();

        // 获取当前日志文件的新文件名
        var newFileName = $"{today}-{max + 1}.log";
        var newFilePath = Path.Combine(Constants.LogFolderPath, newFileName);

        // 重命名日志文件
        File.Move(LogFilePath, newFilePath);
    }
}