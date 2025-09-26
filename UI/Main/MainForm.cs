using MineClearance.Models.Enums;
using MineClearance.Services;
using System.Runtime.InteropServices;

namespace MineClearance.UI.Main;

/// <summary>
/// 表示游戏主界面的窗体
/// </summary>
internal sealed class MainForm : Form
{
    /// <summary>
    /// 主窗体的单例
    /// </summary>
    public static MainForm Instance { get; } = new();

    /// <summary>
    /// 切换到指定类型的面板
    /// </summary>
    /// <param name="panelType">面板类型</param>
    public void SwitchToPanel(PanelType panelType)
    {
        // 隐藏所有面板
        foreach (var panel in Enum.GetValues<PanelType>().Select(GetPanel))
        {
            panel.Visible = false;
        }

        // 获取目标面板
        var targetPanel = GetPanel(panelType);

        // 如果是历史记录面板, 则重启
        if (targetPanel is HistoryPanel historyPanel)
        {
            historyPanel.RestartHistoryPanel();
        }

        // 清除焦点
        ActiveControl = null;

        // 重新获得焦点
        _ = targetPanel.Focus();

        // 设置目标面板可见
        targetPanel.Visible = true;
    }

    /// <summary>
    /// 私有构造函数, 初始化主窗体
    /// </summary>
    private MainForm()
    {
        // 获取当前版本号
        var version = UIConstants.ProgramVersion?.ToString() ?? "未知版本";

        // 设置窗口属性
        Text = UIConstants.ProgramName + " - 版本" + version;
        Size = new(UIConstants.MainFormWidth, UIConstants.MainFormHeight);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        SizeGripStyle = SizeGripStyle.Hide;
        MaximizeBox = false;

        // 添加所有面板到窗体
        Controls.Add(MenuPanel.Instance);
        Controls.Add(GamePreparePanel.Instance);
        Controls.Add(HistoryPanel.Instance);
        Controls.Add(GamePanel.Instance);

        // 添加底部状态栏到窗体
        Controls.Add(BottomStatusBar.Instance);

        // 显示菜单面板
        SwitchToPanel(PanelType.Menu);
    }

    /// <summary>
    /// 根据面板类型获取面板实例
    /// </summary>
    /// <param name="panelType">面板类型</param>
    /// <returns>面板实例</returns>
    /// <exception cref="ArgumentOutOfRangeException">如果面板类型未知则抛出异常</exception>
    private static Panel GetPanel(PanelType panelType) => panelType switch
    {
        PanelType.Game => GamePanel.Instance,
        PanelType.Menu => MenuPanel.Instance,
        PanelType.History => HistoryPanel.Instance,
        PanelType.GamePrepare => GamePreparePanel.Instance,
        _ => throw new ArgumentOutOfRangeException(nameof(panelType), panelType, null)
    };

    /// <summary>
    /// 重写OnLoad方法, 恢复窗口位置和大小
    /// </summary>
    /// <param name="e">窗体加载事件参数</param>
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        // 获取窗口配置数据
        var config = FormConfigManager.Config;

        // 保存的位置
        var left = config.MainFormLeft;
        var top = config.MainFormTop;

        // 当前屏幕的工作区域
        var workingArea = Screen.GetWorkingArea(this);

        // 确保位置在工作区域内
        if (left >= 0 && top >= 0 && left + Width < workingArea.Width && top + Height < workingArea.Height)
        {
            Left = left;
            Top = top;
        }

        // 记录加载
        FileLogger.LogInfo($"成功加载主窗体位置: {Left}, {Top}");
    }

    /// <summary>
    /// 重写OnFormClosing方法
    /// </summary>
    /// <param name="e">窗体关闭事件参数</param>
    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);

        // 保存主窗体位置
        FormConfigManager.Config = FormConfigManager.Config with
        {
            MainFormLeft = Left,
            MainFormTop = Top
        };

        // 记录保存
        FileLogger.LogInfo($"成功保存主窗体位置: {Left}, {Top}");
    }

    /// <summary>
    /// 用于处理WM_MOVING消息的结构体
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        /// <summary>
        /// 左边界
        /// </summary>
        public int Left;

        /// <summary>
        /// 上边界
        /// </summary>
        public int Top;

        /// <summary>
        /// 右边界
        /// </summary>
        public int Right;

        /// <summary>
        /// 下边界
        /// </summary>
        public int Bottom;
    }

    /// <summary>
    /// 重写WndProc方法, 处理WM_MOVING消息, 用于使窗口保持在可见区域内
    /// </summary>
    /// <param name="m">Windows 消息</param>
    protected override void WndProc(ref Message m)
    {
        const int WM_MOVING = 0x0216;

        if (m.Msg == WM_MOVING)
        {
            // 获取当前屏幕的工作区域
            var workingArea = Screen.GetWorkingArea(this);

            // 获取窗口的当前位置
            var rectObj = Marshal.PtrToStructure<RECT>(m.LParam);

            if (rectObj is RECT rect)
            {
                // 调整位置
                if (rect.Left < workingArea.Left)
                {
                    rect.Right = workingArea.Left + (rect.Right - rect.Left);
                    rect.Left = workingArea.Left;
                }

                if (rect.Top < workingArea.Top)
                {
                    rect.Bottom = workingArea.Top + (rect.Bottom - rect.Top);
                    rect.Top = workingArea.Top;
                }

                if (rect.Right > workingArea.Right)
                {
                    rect.Left = workingArea.Right - (rect.Right - rect.Left);
                    rect.Right = workingArea.Right;
                }

                if (rect.Bottom > workingArea.Bottom)
                {
                    rect.Top = workingArea.Bottom - (rect.Bottom - rect.Top);
                    rect.Bottom = workingArea.Bottom;
                }

                // 将调整后的位置写回消息
                Marshal.StructureToPtr(rect, m.LParam, true);
            }
        }

        base.WndProc(ref m);
    }
}