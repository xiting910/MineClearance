using MineClearance.Models.Enums;

namespace MineClearance.UI.Main;

/// <summary>
/// 菜单面板类
/// </summary>
internal sealed class MenuPanel : Panel
{
    /// <summary>
    /// 菜单面板的单例
    /// </summary>
    public static MenuPanel Instance { get; } = new();

    /// <summary>
    /// 标题标签
    /// </summary>
    private readonly Label _titleLabel;

    /// <summary>
    /// 新游戏按钮
    /// </summary>
    private readonly Button _btnNewGame;

    /// <summary>
    /// 游戏历史记录按钮
    /// </summary>
    private readonly Button _btnShowHistory;

    /// <summary>
    /// 设置按钮
    /// </summary>
    private readonly Button _btnSettings;

    /// <summary>
    /// 退出按钮
    /// </summary>
    private readonly Button _btnExit;

    /// <summary>
    /// 提示气泡
    /// </summary>
    private readonly ToolTip _toolTip;

    /// <summary>
    /// 私有构造函数, 初始化菜单面板
    /// </summary>
    private MenuPanel()
    {
        // 设置菜单面板属性
        Dock = DockStyle.Fill;
        BackColor = Color.LightBlue;

        // 标题标签宽度和高度
        var titleLabelWidth = (int)(150 * UIConstants.DpiScale);
        var titleLabelHeight = (int)(50 * UIConstants.DpiScale);

        // 标题标签左侧位置和顶部位置
        var titleLabelLeft = (UIConstants.MainFormWidth - titleLabelWidth) / 2;
        var titleLabelTop = (int)(25 * UIConstants.DpiScale);

        // 添加标题标签
        _titleLabel = new()
        {
            Text = UIConstants.ProgramName,
            Font = new("Microsoft YaHei", 24, FontStyle.Bold),
            ForeColor = Color.DarkRed,
            BackColor = Color.Transparent,
            Size = new(titleLabelWidth, titleLabelHeight),
            Location = new(titleLabelLeft, titleLabelTop),
            TextAlign = ContentAlignment.MiddleCenter
        };

        // 按钮宽度和高度
        var buttonWidth = (int)(125 * UIConstants.DpiScale);
        var buttonHeight = (int)(40 * UIConstants.DpiScale);

        // 按钮间距
        var buttonMargin = (int)(15 * UIConstants.DpiScale);

        // 按钮左侧位置和顶部位置
        var buttonLeft = (UIConstants.MainFormWidth - buttonWidth) / 2;
        var buttonTop = titleLabelTop + titleLabelHeight + buttonMargin;

        // 添加新游戏按钮
        _btnNewGame = new()
        {
            Text = "新游戏",
            Size = new(buttonWidth, buttonHeight),
            Location = new(buttonLeft, buttonTop),
            BackColor = Color.LightGreen,
            ForeColor = Color.DarkBlue,
            FlatStyle = FlatStyle.Flat,
            TabStop = false
        };
        _btnNewGame.Click += (sender, e) =>
        {
            MainForm.Instance.SwitchToPanel(PanelType.GamePrepare);
            BottomStatusBar.Instance.SetStatus(StatusBarState.Preparing);
        };
        buttonTop += buttonHeight + buttonMargin;

        // 添加显示历史记录按钮
        _btnShowHistory = new()
        {
            Text = "游戏历史记录",
            Size = new(buttonWidth, buttonHeight),
            Location = new(buttonLeft, buttonTop),
            BackColor = Color.LightYellow,
            ForeColor = Color.DarkBlue,
            FlatStyle = FlatStyle.Flat,
            TabStop = false
        };
        _btnShowHistory.Click += (sender, e) =>
        {
            MainForm.Instance.SwitchToPanel(PanelType.History);
            BottomStatusBar.Instance.SetStatus(StatusBarState.History);
        };
        buttonTop += buttonHeight + buttonMargin;

        // 创建设置按钮
        _btnSettings = new()
        {
            Text = "设置",
            Size = new(buttonWidth, buttonHeight),
            Location = new(buttonLeft, buttonTop),
            BackColor = Color.LightGray,
            ForeColor = Color.DarkBlue,
            FlatStyle = FlatStyle.Flat,
            TabStop = false
        };
        _btnSettings.Click += (sender, e) => SettingForm.ShowForm();
        buttonTop += buttonHeight + buttonMargin;

        // 添加退出按钮
        _btnExit = new()
        {
            Text = "退出",
            Size = new(buttonWidth, buttonHeight),
            Location = new(buttonLeft, buttonTop),
            BackColor = Color.LightCoral,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            TabStop = false
        };
        _btnExit.Click += (sender, e) => Application.Exit();

        // 添加控件到菜单面板
        Controls.Add(_titleLabel);
        Controls.Add(_btnNewGame);
        Controls.Add(_btnShowHistory);
        Controls.Add(_btnSettings);
        Controls.Add(_btnExit);

        // 初始化提示气泡
        _toolTip = UIConstants.ToolTip;

        // 设置控件的悬浮提示
        _toolTip.SetToolTip(_btnNewGame, "选择游戏难度并开始新游戏");
        _toolTip.SetToolTip(_btnShowHistory, "查看和管理保存在本地的所有游戏历史记录");
        _toolTip.SetToolTip(_btnSettings, "打开设置窗口, 包含一些程序的配置和选项");
        _toolTip.SetToolTip(_btnExit, "关闭主窗口并在后台处理完一些清理和保存工作后退出程序");
    }
}