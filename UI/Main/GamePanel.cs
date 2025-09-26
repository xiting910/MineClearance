using MineClearance.Models;
using MineClearance.Models.Enums;
using MineClearance.UI.Assist;
using Timer = System.Windows.Forms.Timer;

namespace MineClearance.UI.Main;

/// <summary>
/// 游戏面板类
/// </summary>
internal sealed partial class GamePanel : Panel
{
    /// <summary>
    /// 游戏面板的单例
    /// </summary>
    public static GamePanel Instance { get; } = new();

    /// <summary>
    /// 游戏计时器的时间间隔(毫秒)
    /// </summary>
    private const int GameTimerInterval = 10;

    /// <summary>
    /// 信息面板的高度
    /// </summary>
    private static int InfoPanelHeight => (int)(47 * UIConstants.DpiScale);

    /// <summary>
    /// 游戏计时器
    /// </summary>
    private readonly Timer _gameTimer;

    /// <summary>
    /// 游戏顶部信息栏
    /// </summary>
    private readonly Panel _infoPanel;

    /// <summary>
    /// 游戏区域面板, 显示游戏格子
    /// </summary>
    private readonly DoubleBufferedPanel _gameAreaPanel;

    /// <summary>
    /// 当前游戏难度标签
    /// </summary>
    private readonly Label _difficultyLabel;

    /// <summary>
    /// 剩余未标记地雷数标签
    /// </summary>
    private readonly Label _minesLeftLabel;

    /// <summary>
    /// 剩余未处理格子数标签
    /// </summary>
    private readonly Label _unopenedCountLabel;

    /// <summary>
    /// 完成度标签
    /// </summary>
    private readonly Label _completionLabel;

    /// <summary>
    /// 游戏时间标签
    /// </summary>
    private readonly Label _gameTimeLabel;

    /// <summary>
    /// 提示信息标签
    /// </summary>
    private readonly Label _hintLabel;

    /// <summary>
    /// 启用/禁止作弊复选框
    /// </summary>
    private readonly CheckBox _cheatCheckBox;

    /// <summary>
    /// 暂停/继续游戏复选框
    /// </summary>
    private readonly CheckBox _pauseResumeCheckBox;

    /// <summary>
    /// 重新开始按钮
    /// </summary>
    private readonly Button _restartButton;

    /// <summary>
    /// 返回菜单按钮
    /// </summary>
    private readonly Button _backToMenuButton;

    /// <summary>
    /// 提示气泡
    /// </summary>
    private readonly ToolTip _toolTip;

    /// <summary>
    /// 私有构造函数, 初始化游戏面板
    /// </summary>
    private GamePanel()
    {
        // 设置游戏面板属性
        Dock = DockStyle.Fill;

        // 初始化游戏计时器
        _gameTimer = new()
        {
            Interval = GameTimerInterval
        };

        // 初始化游戏实际时间计时器
        _stopwatch = new();

        // 初始化鼠标状态
        _isMouseDown = false;
        _mouseButton = MouseButtons.None;
        _mouseGridPosition = Position.Invalid;

        // 设置游戏左上角格子位置
        _gameStartPosition = new(0, 0);

        // 初始化信息面板
        _infoPanel = new()
        {
            Size = new(UIConstants.MainFormWidth, InfoPanelHeight),
            Location = new(0, 0),
            BackColor = Color.LightGoldenrodYellow
        };

        // 标签高度1
        var labelHeight1 = (int)(20 * UIConstants.DpiScale);

        // 标签高度2
        var labelHeight2 = InfoPanelHeight - labelHeight1;

        // 当前标签X轴位置
        var labelX = 0;

        // 初始化剩余未标记地雷数标签
        _minesLeftLabel = new()
        {
            BackColor = Color.Cyan,
            ForeColor = Color.Purple,
            Location = new(labelX, labelHeight1),
            Size = new((int)(140 * UIConstants.DpiScale), labelHeight2),
            TextAlign = ContentAlignment.MiddleCenter
        };
        labelX += _minesLeftLabel.Width;

        // 初始化剩余未处理格子数标签
        _unopenedCountLabel = new()
        {
            BackColor = Color.Cyan,
            ForeColor = Color.DarkOrange,
            Location = new(labelX, labelHeight1),
            Size = new((int)(115 * UIConstants.DpiScale), labelHeight2),
            TextAlign = ContentAlignment.MiddleCenter
        };
        labelX += _unopenedCountLabel.Width;

        // 初始化完成度标签
        _completionLabel = new()
        {
            BackColor = Color.Cyan,
            ForeColor = Color.Green,
            Location = new(labelX, labelHeight1),
            Size = new((int)(95 * UIConstants.DpiScale), labelHeight2),
            TextAlign = ContentAlignment.MiddleCenter
        };
        labelX += _completionLabel.Width;

        // 初始化游戏时间标签
        _gameTimeLabel = new()
        {
            BackColor = Color.Cyan,
            ForeColor = Color.Blue,
            Location = new(labelX, labelHeight1),
            Size = new((int)(100 * UIConstants.DpiScale), labelHeight2),
            TextAlign = ContentAlignment.MiddleCenter
        };
        labelX += _gameTimeLabel.Width;

        // 初始化当前游戏难度标签
        _difficultyLabel = new()
        {
            BackColor = Color.Cyan,
            ForeColor = Color.BlueViolet,
            Location = new(0, 0),
            Size = new(labelX, labelHeight1),
            TextAlign = ContentAlignment.BottomCenter
        };

        // 按钮Y轴位置
        var buttonY = (int)(14 * UIConstants.DpiScale);

        // 按钮高度
        var buttonHeight = (int)(20 * UIConstants.DpiScale);

        // 按钮宽度
        var buttonWidth = (int)(65 * UIConstants.DpiScale);

        // 按钮间距
        var buttonSpacing = (int)(15 * UIConstants.DpiScale);

        // 面板右侧与按钮的间距
        var panelRightSpacing = (int)(15 * UIConstants.DpiScale);

        // 按钮X轴位置
        var buttonX = UIConstants.MainFormWidth - panelRightSpacing - buttonSpacing - buttonWidth;

        // 初始化返回菜单按钮
        _backToMenuButton = new()
        {
            Text = "返回菜单",
            BackColor = Color.LightCoral,
            Location = new(buttonX, buttonY),
            Size = new(buttonWidth, buttonHeight),
            TextAlign = ContentAlignment.MiddleCenter,
            FlatStyle = FlatStyle.Flat,
            TabStop = false
        };
        buttonX -= buttonSpacing + buttonWidth;

        // 初始化重新开始按钮
        _restartButton = new()
        {
            Text = "重新开始",
            BackColor = Color.Yellow,
            Location = new(buttonX, buttonY),
            Size = new(buttonWidth, buttonHeight),
            TextAlign = ContentAlignment.MiddleCenter,
            FlatStyle = FlatStyle.Flat,
            TabStop = false
        };
        buttonX -= buttonSpacing + buttonWidth;

        // 初始化暂停/继续游戏复选框
        _pauseResumeCheckBox = new()
        {
            Text = "暂停游戏",
            BackColor = Color.Coral,
            Location = new(buttonX, buttonY),
            Size = new(buttonWidth, buttonHeight),
            TextAlign = ContentAlignment.MiddleCenter,
            Appearance = Appearance.Button,
            FlatStyle = FlatStyle.Flat,
            TabStop = false
        };
        buttonWidth += (int)(10 * UIConstants.DpiScale);
        buttonX -= buttonSpacing + buttonWidth;

        // 初始化启用/禁止作弊复选框
        _cheatCheckBox = new()
        {
            Text = "启用作弊",
            BackColor = Color.OrangeRed,
            Location = new(buttonX, buttonY),
            Size = new(buttonWidth, buttonHeight),
            TextAlign = ContentAlignment.MiddleCenter,
            Appearance = Appearance.Button,
            FlatStyle = FlatStyle.Flat,
            TabStop = false
        };
        buttonX -= buttonSpacing;

        // 初始化提示信息标签
        _hintLabel = new()
        {
            BackColor = Color.LightGreen,
            Text = "鼠标移动至此绿色部分可查看详细游戏提示",
            Location = new(labelX, 0),
            Size = new(buttonX - labelX, InfoPanelHeight),
            TextAlign = ContentAlignment.MiddleCenter
        };

        // 初始化游戏区域面板
        _gameAreaPanel = new()
        {
            BackColor = Color.White,
            Location = new(0, InfoPanelHeight),
            Size = new(UIConstants.MainFormWidth, UIConstants.MainFormHeight - InfoPanelHeight - BottomStatusBar.Instance.Height)
        };

        // 初始化提示气泡
        _toolTip = UIConstants.ToolTip;

        // 初始化游戏面板
        Initialize();
    }

    /// <summary>
    /// 初始化
    /// </summary>
    private void Initialize()
    {
        // 订阅游戏计时器事件
        _gameTimer.Tick += OnGameTimerTick;

        // 订阅启用/禁止作弊复选框事件
        _cheatCheckBox.CheckedChanged += OnCheatCheckBoxCheckedChanged;

        // 订阅暂停/继续游戏复选框事件
        _pauseResumeCheckBox.CheckedChanged += OnPauseResumeCheckBoxCheckedChanged;

        // 订阅重新开始按钮事件
        _restartButton.Click += (sender, e) => RestartGame();

        // 订阅返回菜单按钮事件
        _backToMenuButton.Click += (sender, e) =>
        {
            // 结束当前游戏并返回菜单
            EndGame();
            MainForm.Instance.SwitchToPanel(PanelType.Menu);
            BottomStatusBar.Instance.SetStatus(StatusBarState.Ready);
        };

        // 添加信息面板控件
        _infoPanel.Controls.Add(_difficultyLabel);
        _infoPanel.Controls.Add(_minesLeftLabel);
        _infoPanel.Controls.Add(_unopenedCountLabel);
        _infoPanel.Controls.Add(_completionLabel);
        _infoPanel.Controls.Add(_gameTimeLabel);
        _infoPanel.Controls.Add(_hintLabel);
        _infoPanel.Controls.Add(_cheatCheckBox);
        _infoPanel.Controls.Add(_pauseResumeCheckBox);
        _infoPanel.Controls.Add(_restartButton);
        _infoPanel.Controls.Add(_backToMenuButton);

        // 添加鼠标按下事件
        _gameAreaPanel.MouseDown += OnGameAreaMouseDown;

        // 添加鼠标移动事件
        _gameAreaPanel.MouseMove += OnGameAreaMouseMove;

        // 添加鼠标释放事件
        _gameAreaPanel.MouseUp += OnGameAreaMouseUp;

        // 绘制格子事件
        _gameAreaPanel.Paint += (s, e) =>
        {
            // 获取当前绘图区域
            var clip = e.ClipRectangle;
            var rowStart = clip.Top / UIConstants.GridSize;
            var rowEnd = clip.Bottom / UIConstants.GridSize;
            var colStart = clip.Left / UIConstants.GridSize;
            var colEnd = clip.Right / UIConstants.GridSize;

            // 遍历指定区域的格子并绘制
            for (var row = rowStart; row <= rowEnd; ++row)
            {
                for (var col = colStart; col <= colEnd; ++col)
                {
                    DrawGrid(e.Graphics, row, col);
                }
            }
        };

        // 添加信息面板和游戏区域面板到主窗体
        Controls.Add(_infoPanel);
        Controls.Add(_gameAreaPanel);

        // 添加提示气泡
        _toolTip.SetToolTip(_cheatCheckBox, "点击启动作弊模式(若游戏已结束则无法启用), 一旦启用本局游戏将无法禁用, 且本局游戏结果将不再保存\n启用后, 当鼠标进入未打开的地雷格子时该格子会显示为红色, 错误插旗的格子会显示为橙色");
        _toolTip.SetToolTip(_pauseResumeCheckBox, "点击暂停当前游戏, 游戏时间也会暂停, 暂停时不能操作格子, 游戏未开始或已结束时无法暂停");
        _toolTip.SetToolTip(_restartButton, "点击重新开始一个与当前宽度、高度和总地雷数都相同的新游戏, 未结束的游戏将结束并且不会保存");
        _toolTip.SetToolTip(_backToMenuButton, "点击返回菜单, 未结束的游戏将结束并且不会保存");
        _toolTip.SetToolTip(_hintLabel, "提示: 灰色格子为未打开, 绿色格子表示插旗(作弊模式下橙色格子表示错误插旗), 均支持鼠标聚焦效果\n左键点击未打开格子将其打开, 右键点击则标记地雷(在打开一个格子之前无效), 支持按住鼠标滑动操作多个格子\n左键点击数字格子时, 如果周围插旗数量等于数字, 则打开周围所有未插旗的格子(注意: 如果错误插旗可能会导致打开到地雷)\n右键点击数字格子时, 如果周围未打开格子数量等于数字, 则插旗所有周围未插旗的格子\n当数字格子周围插旗的数量大于其数字时, 该数字格子会变为黄色表示警告\n将鼠标移动到左侧游戏信息栏的标签上可以查看该标签的更详细的信息");
        _toolTip.SetToolTip(_gameTimeLabel, $"计时器更新频率: {_gameTimer.Interval / 1000.0:0.###}s");
    }

    /// <summary>
    /// 游戏计时器事件处理
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="e">事件参数</param>
    private void OnGameTimerTick(object? sender, EventArgs e)
    {
        // 如果游戏实例为空, 不做任何处理
        if (_gameInstance is null)
        {
            return;
        }

        // 当前实际游戏时间
        var elapsed = _stopwatch.Elapsed;

        // 设置游戏用时
        _gameInstance.Duration = elapsed;

        // 更新游戏时间标签
        _gameTimeLabel.Text = $"游戏时间: {(int)elapsed.TotalMinutes:D2}:{elapsed.Seconds:D2}";
    }

    /// <summary>
    /// 格子状态改变事件处理
    /// </summary>
    /// <param name="position">格子位置</param>
    private void OnGridChanged(Position position)
    {
        // 获取格子行列坐标
        var row = position.Row + _gameStartPosition.Row;
        var col = position.Col + _gameStartPosition.Col;

        // 重绘指定格子
        _gameAreaPanel.Invalidate(new Rectangle(col * UIConstants.GridSize, row * UIConstants.GridSize, UIConstants.GridSize, UIConstants.GridSize));
    }

    /// <summary>
    /// 剩余未标记地雷数量改变事件处理
    /// </summary>
    /// <param name="changeNum">变化的数量</param>
    /// <exception cref="ArgumentNullException">当前游戏实例为空时抛出</exception>
    private void OnRemainingMinesChanged(int changeNum)
    {
        // 确保当前游戏实例不为空
        ArgumentNullException.ThrowIfNull(_gameInstance, nameof(_gameInstance));

        // 更新剩余未标记地雷数量
        _remainingMines += changeNum;

        // 确保显示的剩余未标记地雷数不小于0
        var showRemainingMines = Math.Max(0, _remainingMines);

        // 更新剩余未标记地雷数标签
        _minesLeftLabel.Text = $"剩余未标记地雷数: {showRemainingMines}";

        // 更新提示气泡
        _toolTip.SetToolTip(_minesLeftLabel, $"总地雷数: {_gameInstance.TotalMines}, 已标记地雷数: {_gameInstance.TotalMines - _remainingMines}");
    }

    /// <summary>
    /// 未处理格子数量改变事件处理
    /// </summary>
    /// <param name="changeNum">变化的数量</param>
    /// <exception cref="ArgumentNullException">当前游戏实例为空时抛出</exception>
    private void OnUnopenedCountChanged(int changeNum)
    {
        // 确保当前游戏实例不为空
        ArgumentNullException.ThrowIfNull(_gameInstance, nameof(_gameInstance));

        // 更新未处理格子数量
        _unopenedCount += changeNum;

        // 更新未处理格子数标签
        _unopenedCountLabel.Text = $"未处理格子数: {_unopenedCount}";

        // 计算总格子数
        var totalCount = _gameInstance.TotalMines + _gameInstance.TotalSafeCount;

        // 更新提示气泡
        _toolTip.SetToolTip(_unopenedCountLabel, $"总格子数: {totalCount}, 已处理格子数: {totalCount - _unopenedCount}");
    }

    /// <summary>
    /// 未打开的安全格子数量改变事件处理
    /// </summary>
    /// <param name="currentCount">当前未打开的安全格子数量</param>
    /// <exception cref="ArgumentNullException">当前游戏实例为空时抛出</exception>
    private void OnUnopenedSafeCountChanged(int currentCount)
    {
        ArgumentNullException.ThrowIfNull(_gameInstance, nameof(_gameInstance));

        // 计算已经打开的安全格子数量
        var openedSafeCount = _gameInstance.TotalSafeCount - currentCount;

        // 计算完成度
        var completion = openedSafeCount * 100.0 / _gameInstance.TotalSafeCount;

        // 更新完成度标签
        _completionLabel.Text = $"完成度: {completion:0.##}%";

        // 更新提示气泡
        _toolTip.SetToolTip(_completionLabel, $"已打开安全格子数: {openedSafeCount}, 剩余安全格子数: {currentCount}, 总安全格子数: {_gameInstance.TotalSafeCount}");
    }

    /// <summary>
    /// 根据鼠标当前位置返回所在的游戏格子位置
    /// </summary>
    /// <param name="mousePosition">鼠标位置</param>
    /// <returns>对应的格子位置, 如果不在任何格子上则返回无效位置</returns>
    private Position GetGridPositionAtMousePosition(Point mousePosition)
    {
        // 计算鼠标位置对应的游戏格子行列
        var col = mousePosition.X / UIConstants.GridSize - _gameStartPosition.Col;
        var row = mousePosition.Y / UIConstants.GridSize - _gameStartPosition.Row;

        if (row >= 0 && row < _gameInstance?.Board.Height && col >= 0 && col < _gameInstance?.Board.Width)
        {
            // 返回对应的位置
            return new(row, col);
        }

        // 如果鼠标位置不在任何格子上, 返回无效位置
        return Position.Invalid;
    }

    /// <summary>
    /// 绘制指定位置的格子
    /// </summary>
    /// <param name="g">绘图对象</param>
    /// <param name="row">行索引</param>
    /// <param name="col">列索引</param>
    private void DrawGrid(Graphics g, int row, int col)
    {
        // 如果游戏实例为空, 不绘制
        if (_gameInstance is null || row < _gameStartPosition.Row || col < _gameStartPosition.Col)
        {
            return;
        }

        // 获取游戏的行列坐标
        var gameRow = row - _gameStartPosition.Row;
        var gameCol = col - _gameStartPosition.Col;

        // 如果行列索引无效, 不绘制
        if (gameRow >= _gameInstance.Board.Height || gameCol >= _gameInstance.Board.Width)
        {
            return;
        }

        // 获取格子矩形区域
        var rect = new Rectangle(col * UIConstants.GridSize, row * UIConstants.GridSize, UIConstants.GridSize, UIConstants.GridSize);

        // 当前格子是否真的是地雷格子
        var isRealMine = _gameInstance.Board.IsMine(gameRow, gameCol);

        // 鼠标是否在当前格子上
        var isMouseOver = _mouseGridPosition.Row == gameRow && _mouseGridPosition.Col == gameCol;

        // 当前格子周围的地雷数量
        var surroundingMines = _gameInstance.Board.Grids[gameRow, gameCol].SurroundingMines;

        // 是否是数字格子
        var isNumberGrid = false;

        // 根据格子类型选择颜色
        var fillColor = Color.IndianRed;
        switch (_gameInstance.Board.Grids[gameRow, gameCol].Type)
        {
            case GridType.Empty:
                fillColor = Color.White;
                break;
            case GridType.Unopened:
                fillColor = _isGameLost ? isRealMine ? Color.Red : Color.LightGray : _isGameWon ? isRealMine ? Color.Green : Color.LightGray : isMouseOver ? _isCheatEnabled && isRealMine ? Color.Red : Color.Gray : Color.LightGray;
                break;
            case GridType.Flagged:
                fillColor = _isGameLost || _isGameWon ? isRealMine ? Color.Green : Color.Orange : _isCheatEnabled && !isRealMine ? isMouseOver ? Color.DarkOrange : Color.Orange : isMouseOver ? Color.DarkGreen : Color.Green;
                break;
            case GridType.Number:
                fillColor = isMouseOver && !_isGameLost && !_isGameWon ? Color.WhiteSmoke : Color.White;
                isNumberGrid = true;
                break;
            case GridType.WarningNumber:
                fillColor = Color.Yellow;
                isNumberGrid = true;
                break;
            case GridType.Mine:
                fillColor = Color.DarkRed;
                break;
            default:
                break;
        }

        // 绘制格子背景
        using var brush = new SolidBrush(fillColor);
        g.FillRectangle(brush, rect);

        // 绘制格子边框
        using var pen = new Pen(Color.Black, 1);
        g.DrawRectangle(pen, rect);

        // 如果是数字格子, 绘制数字
        if (isNumberGrid)
        {
            // 根据周围地雷数量设置文本和颜色
            var (text, textColor) = surroundingMines switch
            {
                1 => ("1", Color.Blue),
                2 => ("2", Color.Green),
                3 => ("3", Color.Red),
                4 => ("4", Color.Purple),
                5 => ("5", Color.Maroon),
                6 => ("6", Color.Turquoise),
                7 => ("7", Color.Black),
                8 => ("8", Color.Gray),
                _ => ("?", Color.PaleVioletRed)
            };

            // 计算文本位置并绘制
            var textSize = g.MeasureString(text, DefaultFont);
            var textX = rect.X + (rect.Width - textSize.Width) / 2;
            var textY = rect.Y + (rect.Height - textSize.Height) / 2;
            using var textBrush = new SolidBrush(textColor);
            g.DrawString(text, DefaultFont, textBrush, textX, textY);
        }
    }
}