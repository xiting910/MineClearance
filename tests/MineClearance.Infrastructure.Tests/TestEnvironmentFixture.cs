namespace MineClearance.Infrastructure.Tests;

/// <summary>
/// 程序集级测试夹具, 在测试运行前设置数据目录环境变量指向临时目录, 结束后恢复并清理
/// </summary>
public sealed class TestEnvironmentFixture : IDisposable
{
    /// <summary>
    /// 测试使用的临时数据根目录
    /// </summary>
    public string DataRootDirectory { get; }

    /// <summary>
    /// 初始化夹具, 创建临时目录并设置环境变量
    /// </summary>
    public TestEnvironmentFixture()
    {
        DataRootDirectory = Path.Combine(
            Path.GetTempPath(), nameof(MineClearance), $"{Guid.NewGuid():N}"
        );
        _ = Directory.CreateDirectory(DataRootDirectory);
        Environment.SetEnvironmentVariable(
            Constants.AppDataRootDirectoryEnvironmentVariableName, DataRootDirectory
        );
    }

    /// <summary>
    /// 恢复环境变量并删除临时目录
    /// </summary>
    public void Dispose()
    {
        Environment.SetEnvironmentVariable(Constants.AppDataRootDirectoryEnvironmentVariableName, null);
        try
        {
            Directory.Delete(DataRootDirectory, recursive: true);
        }
        catch { /* 忽略删除临时目录时的异常 */ }
    }
}
