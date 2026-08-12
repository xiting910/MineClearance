using MineClearance.Infrastructure.Models;
using System.Text.Json;

namespace MineClearance.Infrastructure.Tests;

/// <summary>
/// <see cref="BootstrapUpdateHelper"/> 的单元测试, 覆盖引导更新参数解析、更新信息读取与残留清理
/// </summary>
public sealed class BootstrapUpdateHelperTests
{
    /// <summary>
    /// 每个测试开始前重置更新数据目录, 避免测试间互相干扰
    /// </summary>
    public BootstrapUpdateHelperTests()
    {
        var updateDataDir = new DirectoryInfo(Constants.UpdateDataDirectory);
        if (updateDataDir.Exists)
        {
            updateDataDir.Delete(recursive: true);
        }
        updateDataDir.Create();
    }

    [Fact]
    public void IsBootstrapUpdateRequested_无参数_返回false()
    {
        Assert.False(BootstrapUpdateHelper.IsBootstrapUpdateRequested([], out _, out _));
    }

    [Fact]
    public void IsBootstrapUpdateRequested_仅标志参数_返回false()
    {
        Assert.False(BootstrapUpdateHelper.IsBootstrapUpdateRequested(
            [Constants.UseBootstrapUpdateModeArgument], out _, out _
        ));
    }

    [Fact]
    public void IsBootstrapUpdateRequested_目录不存在_返回false()
    {
        var directory = Path.Combine(Constants.UpdateDataDirectory, "NotExists");

        Assert.False(BootstrapUpdateHelper.IsBootstrapUpdateRequested(
            [Constants.UseBootstrapUpdateModeArgument, directory, "1.0.0"], out _, out _
        ));
    }

    [Fact]
    public void IsBootstrapUpdateRequested_参数完整且目录存在_返回true并输出参数()
    {
        var directory = Directory.CreateTempSubdirectory();
        try
        {
            Assert.True(BootstrapUpdateHelper.IsBootstrapUpdateRequested(
                [Constants.UseBootstrapUpdateModeArgument, directory.FullName, "1.0.0"],
                out var originalDirectory,
                out var originalVersion
            ));
            Assert.Equal(directory.FullName, originalDirectory);
            Assert.Equal("1.0.0", originalVersion);
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }

    [Fact]
    public void IsBootstrapUpdateRequested_标志参数在中间且带后续参数_返回true()
    {
        var directory = Directory.CreateTempSubdirectory();
        try
        {
            Assert.True(BootstrapUpdateHelper.IsBootstrapUpdateRequested(
                ["arg0", Constants.UseBootstrapUpdateModeArgument, directory.FullName, "1.0.0", "arg4"],
                out var originalDirectory,
                out var originalVersion
            ));
            Assert.Equal(directory.FullName, originalDirectory);
            Assert.Equal("1.0.0", originalVersion);
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }

    [Fact]
    public void ExecuteBootstrapUpdate_非引导副本目录_返回1()
    {
        Assert.Equal(1, BootstrapUpdateHelper.ExecuteBootstrapUpdate("任意目录", "1.0.0"));
    }

    [Fact]
    public void GetLastUpdateInfoAndCleanUp_无更新信息文件_返回null()
    {
        Assert.Null(BootstrapUpdateHelper.GetLastUpdateInfoAndCleanUp());
    }

    [Fact]
    public void GetLastUpdateInfoAndCleanUp_更新成功_返回信息并清理残留()
    {
        // 构造更新成功后的残留文件
        File.WriteAllText(
            Constants.UpdateInfoFilePath, JsonSerializer.Serialize(new UpdateInfo(true, "1.0.0", "1.1.0"))
        );
        File.WriteAllText(Constants.NewVersionFilePath, "1.1.0");
        File.WriteAllText(Constants.UpdatePackageFilePath, "package");
        File.WriteAllText(Constants.UpdateLogFilePath, "log");
        _ = Directory.CreateDirectory(Constants.BackupDirectory);
        _ = Directory.CreateDirectory(Constants.BootstrapCopyDirectory);

        var info = BootstrapUpdateHelper.GetLastUpdateInfoAndCleanUp();

        Assert.Equal(new(true, "1.0.0", "1.1.0"), info);
        Assert.False(File.Exists(Constants.UpdateInfoFilePath));
        Assert.False(File.Exists(Constants.NewVersionFilePath));
        Assert.False(File.Exists(Constants.UpdatePackageFilePath));
        Assert.False(File.Exists(Constants.UpdateLogFilePath));
        Assert.False(Directory.Exists(Constants.BackupDirectory));
        Assert.False(Directory.Exists(Constants.BootstrapCopyDirectory));
    }

    [Fact]
    public void GetLastUpdateInfoAndCleanUp_更新失败_返回信息但保留残留()
    {
        // 构造更新失败后的残留文件
        File.WriteAllText(
            Constants.UpdateInfoFilePath, JsonSerializer.Serialize(new UpdateInfo(false, "1.0.0", "1.1.0"))
        );
        File.WriteAllText(Constants.NewVersionFilePath, "1.1.0");

        var info = BootstrapUpdateHelper.GetLastUpdateInfoAndCleanUp();

        Assert.Equal(new(false, "1.0.0", "1.1.0"), info);
        Assert.False(File.Exists(Constants.UpdateInfoFilePath));
        Assert.True(File.Exists(Constants.NewVersionFilePath));
    }

    [Fact]
    public void GetLastUpdateInfoAndCleanUp_更新信息文件损坏_返回null并删除文件()
    {
        File.WriteAllText(Constants.UpdateInfoFilePath, "not-json");

        Assert.Null(BootstrapUpdateHelper.GetLastUpdateInfoAndCleanUp());
        Assert.False(File.Exists(Constants.UpdateInfoFilePath));
    }
}
