using MineClearance.Core;
using MineClearance.Services;
using MineClearance.Utilities;
using MineClearance.UI.Assist;
using MineClearance.Models.Enums;

namespace MineClearance.UI.Main;

/// <summary>
/// 准备游戏面板类
/// </summary>
internal sealed class GamePreparePanel : Panel
{
    /// <summary>
    /// 准备游戏面板的单例
    /// </summary>
    public static GamePreparePanel Instance { get; } = new();

    /// <summary>
    /// 标题标签
    /// </summary>
    private readonly Label _titleLabel;

    /// <summary>
    /// 简单按钮
    /// </summary>
    private readonly Button _btnEasy;

    /// <summary>
    /// 普通按钮
    /// </summary>
    private readonly Button _btnMedium;

    /// <summary>
    /// 困难按钮
    /// </summary>
    private readonly Button _btnHard;

    /// <summary>
    /// 地狱按钮
    /// </summary>
    private readonly Button _btnHell;

    /// <summary>
    /// 自定义按钮
    /// </summary>
    private readonly Button _btnCustom;

    /// <summary>
    /// 返回按钮
    /// </summary>
    private readonly Button _btnBack;

    /// <summary>
    /// 提示气泡
    /// </summary>
    private readonly ToolTip _toolTip;

    /// <summary>
    /// 私有构造函数, 初始化准备游戏面板
    /// </summary>
    private GamePreparePanel()
    {
        // 设置准备游戏面板属性
        Dock = DockStyle.Fill;
        BackColor = Color.LightYellow;

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

        // 添加简单按钮
        _btnEasy = new()
        {
            Text = "简单",
            Size = new(buttonWidth, buttonHeight),
            Location = new(buttonLeft, buttonTop),
            BackColor = Color.LightGreen,
            ForeColor = Color.DarkBlue,
            FlatStyle = FlatStyle.Flat,
            TabStop = false
        };
        _btnEasy.Click += (sender, e) => StartNewGame(new(DifficultyLevel.Easy));
        buttonTop += buttonHeight + buttonMargin;

        // 添加普通按钮
        _btnMedium = new()
        {
            Text = "普通",
            Size = new(buttonWidth, buttonHeight),
            Location = new(buttonLeft, buttonTop),
            BackColor = Color.Yellow,
            ForeColor = Color.DarkBlue,
            FlatStyle = FlatStyle.Flat,
            TabStop = false
        };
        _btnMedium.Click += (sender, e) => StartNewGame(new(DifficultyLevel.Medium));
        buttonTop += buttonHeight + buttonMargin;

        // 添加困难按钮
        _btnHard = new()
        {
            Text = "困难",
            Size = new(buttonWidth, buttonHeight),
            Location = new(buttonLeft, buttonTop),
            BackColor = Color.Red,
            ForeColor = Color.DarkBlue,
            FlatStyle = FlatStyle.Flat,
            TabStop = false
        };
        _btnHard.Click += (sender, e) => StartNewGame(new(DifficultyLevel.Hard));
        buttonTop += buttonHeight + buttonMargin;

        // 添加地狱按钮
        _btnHell = new()
        {
            Text = "地狱",
            Size = new(buttonWidth, buttonHeight),
            Location = new(buttonLeft, buttonTop),
            BackColor = Color.DarkRed,
            ForeColor = Color.DarkBlue,
            FlatStyle = FlatStyle.Flat,
            TabStop = false
        };
        _btnHell.Click += (sender, e) => StartNewGame(new(DifficultyLevel.Hell));
        buttonTop += buttonHeight + buttonMargin;

        // 添加自定义按钮
        _btnCustom = new()
        {
            Text = "自定义",
            Size = new(buttonWidth, buttonHeight),
            Location = new(buttonLeft, buttonTop),
            BackColor = Color.LightBlue,
            ForeColor = Color.DarkBlue,
            FlatStyle = FlatStyle.Flat,
            TabStop = false
        };
        _btnCustom.Click += (sender, e) =>
        {
            // 开始自定义难度游戏的逻辑(弹出对话框获取自定义参数)
            using var dialog = new CustomDifficultyDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var (width, height, mineCount) = dialog.CustomDifficulty;
                var customGame = new Game(width, height, mineCount);
                StartNewGame(customGame);
            }
        };
        buttonTop += buttonHeight + buttonMargin;

        // 添加返回菜单按钮
        _btnBack = new()
        {
            Text = "返回",
            Size = new(buttonWidth, buttonHeight),
            Location = new(buttonLeft, buttonTop),
            BackColor = Color.LightCoral,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            TabStop = false
        };
        _btnBack.Click += (sender, e) =>
        {
            MainForm.Instance.SwitchToPanel(PanelType.Menu);
            BottomStatusBar.Instance.SetStatus(StatusBarState.Ready);
        };

        // 添加控件到游戏准备面板
        Controls.Add(_titleLabel);
        Controls.Add(_btnEasy);
        Controls.Add(_btnMedium);
        Controls.Add(_btnHard);
        Controls.Add(_btnHell);
        Controls.Add(_btnCustom);
        Controls.Add(_btnBack);

        // 初始化提示气泡
        _toolTip = UIConstants.ToolTip;

        // 获取各难度的设置
        var easySettings = Constants.GetSettings(DifficultyLevel.Easy);
        var mediumSettings = Constants.GetSettings(DifficultyLevel.Medium);
        var hardSettings = Constants.GetSettings(DifficultyLevel.Hard);
        var hellSettings = Constants.GetSettings(DifficultyLevel.Hell);

        // 设置控件的悬浮提示
        _toolTip.SetToolTip(_btnEasy, $"开始简单难度的新游戏, 宽度为{easySettings.width}, 高度为{easySettings.height}, 地雷数为{easySettings.mineCount}");
        _toolTip.SetToolTip(_btnMedium, $"开始普通难度的新游戏, 宽度为{mediumSettings.width}, 高度为{mediumSettings.height}, 地雷数为{mediumSettings.mineCount}");
        _toolTip.SetToolTip(_btnHard, $"开始困难难度的新游戏, 宽度为{hardSettings.width}, 高度为{hardSettings.height}, 地雷数为{hardSettings.mineCount}");
        _toolTip.SetToolTip(_btnHell, $"开始地狱难度的新游戏, 宽度为{hellSettings.width}, 高度为{hellSettings.height}, 地雷数为{hellSettings.mineCount}");
        _toolTip.SetToolTip(_btnCustom, $"开始自定义难度的新游戏, 宽度、高度和地雷数由您自己选择");
        _toolTip.SetToolTip(_btnBack, $"返回主菜单");
    }

    /// <summary>
    /// 开启新游戏并自动切换到游戏面板
    /// </summary>
    /// <param name="game">要开始的新游戏</param>
    private static void StartNewGame(Game game)
    {
        try
        {
            MainForm.Instance.SwitchToPanel(PanelType.Game);
            GamePanel.Instance.StartGame(game);
        }
        catch (InvalidOperationException ex)
        {
            FileLogger.LogException(ex);
            _ = MessageBox.Show($"无法开始新游戏: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}