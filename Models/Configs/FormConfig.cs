namespace MineClearance.Models.Configs;

/// <summary>
/// 窗口配置数据类
/// </summary>
internal sealed record FormConfig
{
    /// <summary>
    /// 主窗体左侧位置
    /// </summary>
    public int MainFormLeft { get; init; }

    /// <summary>
    /// 主窗体顶部位置
    /// </summary>
    public int MainFormTop { get; init; }

    /// <summary>
    /// 设置窗口左侧位置
    /// </summary>
    public int SettingFormLeft { get; init; }

    /// <summary>
    /// 设置窗口顶部位置
    /// </summary>
    public int SettingFormTop { get; init; }

    /// <summary>
    /// 设置窗口宽度
    /// </summary>
    public int SettingFormWidth { get; init; }

    /// <summary>
    /// 设置窗口高度
    /// </summary>
    public int SettingFormHeight { get; init; }

    /// <summary>
    /// 无效的配置
    /// </summary>
    public static FormConfig Invalid { get; } = new()
    {
        MainFormLeft = -1,
        MainFormTop = -1,
        SettingFormLeft = -1,
        SettingFormTop = -1,
        SettingFormWidth = -1,
        SettingFormHeight = -1
    };
}