using MineClearance.Infrastructure.Models;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace MineClearance.Infrastructure;

/// <summary>
/// 引导更新辅助类
/// </summary>
public static class BootstrapUpdateHelper
{
    /// <summary>
    /// 检查是否请求了引导更新, 并返回原始目录和版本号
    /// </summary>
    /// <param name="args">启动参数</param>
    /// <param name="originalDirectory">原始目录</param>
    /// <param name="originalVersion">原始版本号</param>
    /// <returns><see langword="true"/> 如果请求了引导更新, 否则 <see langword="false"/></returns>
    public static bool IsBootstrapUpdateRequested(
        string[] args,
        [MaybeNullWhen(false)] out string originalDirectory,
        [MaybeNullWhen(false)] out string originalVersion)
    {
        originalDirectory = default;
        originalVersion = default;

        var index = Array.IndexOf(args, Constants.UseBootstrapUpdateModeArgument);
        if (index >= 0)
        {
            var originalVersionIndex = index + 2;
            if (originalVersionIndex < args.Length)
            {
                originalDirectory = args[index + 1];
                if (Directory.Exists(originalDirectory))
                {
                    originalVersion = args[originalVersionIndex];
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// 执行引导更新
    /// </summary>
    /// <param name="originalDirectory">原始程序目录</param>
    /// <param name="originalVersion">原始程序版本</param>
    /// <returns>退出码</returns>
    public static int ExecuteBootstrapUpdate(string originalDirectory, string originalVersion)
    {
        // 如果当前程序目录不是引导副本目录, 不执行引导更新
        if (!Constants.PathComparer.Equals(
            Path.TrimEndingDirectorySeparator(AppContext.BaseDirectory),
            Constants.BootstrapCopyDirectory))
        {
            return 1;
        }

        // 打开更新日志文件的写入流
        using var logStream = new StreamWriter(Constants.UpdateLogFilePath, append: false, Encoding.UTF8)
        {
            AutoFlush = true
        };

        // 记录引导更新开始
        logStream.WriteLine(
            $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 引导更新开始: " +
            $"原始目录 {originalDirectory}, 原始版本 v{originalVersion}"
        );

        // 如果新版本信息文件或者更新包文件不存在, 记录日志并返回
        if (!File.Exists(Constants.NewVersionFilePath) || !File.Exists(Constants.UpdatePackageFilePath))
        {
            logStream.WriteLine(
                $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 新版本信息文件或更新包文件不存在, 引导更新失败"
            );
            return 2;
        }

        // 获取新版本号
        var newVersion = File.ReadAllText(Constants.NewVersionFilePath).Trim();

        // 获取当前进程
        var currentProcess = Process.GetCurrentProcess();

        // 获取当前进程的所有同名进程, 并排除当前进程
        var otherProcesses = Process.GetProcessesByName(currentProcess.ProcessName)
            .Where(p => p.Id != currentProcess.Id);

        // 等待所有同名进程退出
        foreach (var process in otherProcesses)
        {
            try
            {
                if (!process.WaitForExit(10000))
                {
                    // 记录等待超时的进程信息
                    logStream.WriteLine(
                        $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 等待进程 {process.ProcessName} 退出超时: " +
                        $"ID {process.Id}, 启动时间 {process.StartTime:yyyy-MM-dd HH:mm:ss}"
                    );

                    // 写入更新失败信息
                    WriteUpdateInfo(false, originalVersion, newVersion);

                    // 退出引导更新
                    return 3;
                }
            }
            catch (Exception ex)
            {
                // 记录等待进程退出时的异常信息
                logStream.WriteLine(
                    $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 等待进程 {process.ProcessName} 退出时发生异常: " +
                    $"ID {process.Id}, 启动时间 {process.StartTime:yyyy-MM-dd HH:mm:ss}, 异常 {ex}"
                );

                // 写入更新失败信息
                WriteUpdateInfo(false, originalVersion, newVersion);

                // 退出引导更新
                return 4;
            }
        }

        // 备份文件路径
        var backupFilePath = Path.Combine(Constants.BackupDirectory, $"{DateTime.Now:yyyy-MM-dd_HHmmss}.zip");

        try
        {
            // 确保备份目录存在
            _ = Directory.CreateDirectory(Constants.BackupDirectory);

            // 将原始目录备份
            ZipFile.CreateFromDirectory(
                originalDirectory,
                backupFilePath,
                CompressionLevel.Optimal,
                includeBaseDirectory: false
            );
        }
        catch (Exception ex)
        {
            // 记录备份目录时的异常信息
            logStream.WriteLine(
                $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 备份原始目录 {originalDirectory} 时发生异常: {ex}"
            );

            // 写入更新失败信息
            WriteUpdateInfo(false, originalVersion, newVersion);

            // 退出引导更新
            return 5;
        }

        try
        {
            // 执行更新操作
            ZipFile.ExtractToDirectory(Constants.UpdatePackageFilePath, originalDirectory, true);
        }
        catch (Exception ex)
        {
            // 记录更新操作时的异常信息
            logStream.WriteLine(
                $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 执行更新操作时发生异常: {ex}"
            );

            try
            {
                // 恢复备份
                ZipFile.ExtractToDirectory(backupFilePath, originalDirectory, true);

                // 记录恢复备份成功信息
                logStream.WriteLine(
                    $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 恢复备份: {backupFilePath} -> {originalDirectory}"
                );
            }
            catch (Exception restoreEx)
            {
                // 记录恢复备份时的异常信息
                logStream.WriteLine(
                    $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 恢复备份 {backupFilePath} 时发生异常: {restoreEx}"
                );
            }

            // 写入更新失败信息
            WriteUpdateInfo(false, originalVersion, newVersion);

            // 退出引导更新
            return 6;
        }

        // 记录更新成功信息
        logStream.WriteLine(
            $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 更新成功: {originalDirectory} -> v{newVersion}"
        );

        // 写入更新成功信息
        WriteUpdateInfo(true, originalVersion, newVersion);

        // 获取可执行文件名
        var executableName = Path.GetFileName(currentProcess.MainModule?.FileName);
        if (string.IsNullOrWhiteSpace(executableName))
        {
            // 记录获取可执行文件名失败信息
            logStream.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 获取可执行文件名失败");

            // 退出引导更新
            return 7;
        }

        try
        {
            // 启动更新后的程序
            _ = Process.Start(new ProcessStartInfo
            {
                WorkingDirectory = originalDirectory,
                FileName = Path.Combine(originalDirectory, executableName)
            });

            // 返回成功退出码
            return 0;
        }
        catch (Exception ex)
        {
            // 记录启动更新后的程序时的异常信息
            logStream.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 启动更新后的程序失败: {ex}");

            // 退出引导更新
            return 8;
        }
    }

    /// <summary>
    /// 写入更新信息到文件
    /// </summary>
    /// <param name="isSuccess">是否成功</param>
    /// <param name="originalVersion">原始版本号</param>
    /// <param name="newVersion">新版本号</param>
    private static void WriteUpdateInfo(bool isSuccess, string originalVersion, string newVersion)
    {
        try
        {
            File.WriteAllText(
                Constants.UpdateInfoFilePath,
                JsonSerializer.Serialize<UpdateInfo>(new(isSuccess, originalVersion, newVersion))
            );
        }
        catch { /* 忽略写入更新信息时的异常 */ }
    }
}
