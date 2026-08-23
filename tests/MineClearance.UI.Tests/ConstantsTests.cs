namespace MineClearance.UI.Tests;

/// <summary>
/// <see cref="Constants"/> 的单元测试, 验证测试环境下的数据目录重定向机制
/// </summary>
public sealed class ConstantsTests
{
    [Fact]
    public void UIOptionsSettingsFilePath_位于测试数据目录下()
    {
        Assert.StartsWith(
            Infrastructure.Constants.AppDataRootDirectory, Constants.UIOptionsSettingsFilePath
        );
    }

    [Fact]
    public void BackgroundImageDirectory_位于程序基目录的Pictures子目录()
    {
        Assert.Equal(
            Path.Combine(AppContext.BaseDirectory, "Pictures"), Constants.BackgroundImageDirectory
        );
    }
}
