namespace MineClearance.UI.Tests;

/// <summary>
/// <see cref="AppMetadata"/> 的单元测试, 覆盖程序集元数据读取
/// </summary>
public sealed class AppMetadataTests
{
    [Fact]
    public void Get_作者键_返回非空值()
    {
        Assert.False(string.IsNullOrWhiteSpace(AppMetadata.Get(AppMetadata.AuthorKey)));
    }

    [Fact]
    public void Get_产品键_返回MineClearance()
    {
        Assert.Equal(nameof(MineClearance), AppMetadata.Get(AppMetadata.ProductKey));
    }

    [Fact]
    public void Get_版本键_返回非空值()
    {
        Assert.False(string.IsNullOrWhiteSpace(AppMetadata.Get(AppMetadata.VersionKey)));
    }

    [Fact]
    public void Get_许可键_返回MIT()
    {
        Assert.Equal("MIT", AppMetadata.Get("License"));
    }

    [Fact]
    public void Get_仓库地址键_返回GitHub地址()
    {
        Assert.StartsWith("https://github.com/", AppMetadata.Get("GitHubUrl"));
    }
}
