namespace MineClearance.UI.Assist;

/// <summary>
/// 自定义双缓冲面板
/// </summary>
internal sealed class DoubleBufferedPanel : Panel
{
    /// <summary>
    /// 构造函数, 初始化双缓冲面板
    /// </summary>
    public DoubleBufferedPanel()
    {
        DoubleBuffered = true;
        SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        UpdateStyles();
    }
}

/// <summary>
/// 自定义双缓冲DataGridView
/// </summary>
internal sealed class DoubleBufferedDataGridView : DataGridView
{
    /// <summary>
    /// 构造函数, 初始化双缓冲DataGridView
    /// </summary>
    public DoubleBufferedDataGridView()
    {
        DoubleBuffered = true;
        SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        UpdateStyles();
    }
}