namespace MineClearance.Infrastructure.Tests;

/// <summary>
/// Constants 的单元测试, 验证测试环境下的数据根目录重定向机制
/// </summary>
public sealed class ConstantsTests
{
    [Fact]
    public void AppDataRootDirectory_测试环境变量已设置_返回环境变量值()
    {
        var expected = Environment.GetEnvironmentVariable(
            Constants.AppDataRootDirectoryEnvironmentVariableName
        );
        Assert.Equal(expected, Constants.AppDataRootDirectory);
    }
}
