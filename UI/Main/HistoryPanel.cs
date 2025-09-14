using MineClearance.Models.Enums;
using MineClearance.Utilities;
using MineClearance.UI.Assist;

namespace MineClearance.UI.Main;

/// <summary>
/// 历史记录面板类
/// </summary>
internal sealed partial class HistoryPanel : Panel
{
    /// <summary>
    /// 历史记录面板的单例
    /// </summary>
    public static HistoryPanel Instance { get; } = new();

    /// <summary>
    /// 历史记录顶部面板高度
    /// </summary>
    private static int HistoryTopPanelHeight => (int)(40 * UIConstants.DpiScale);

    /// <summary>
    /// 历史记录顶部面板
    /// </summary>
    private readonly Panel historyTopPanel;

    /// <summary>
    /// 提示信息标签
    /// </summary>
    private readonly Label tipsLabel;

    /// <summary>
    /// 统计信息列表框
    /// </summary>
    private readonly DoubleBufferedDataGridView statisticsDataGridView;

    /// <summary>
    /// 历史记录列表框
    /// </summary>
    private readonly DoubleBufferedDataGridView historyDataGridView;

    /// <summary>
    /// 历史记录列表框行的右键菜单
    /// </summary>
    private readonly ContextMenuStrip historyContextMenu;

    /// <summary>
    /// 提示气泡
    /// </summary>
    private readonly ToolTip _toolTip;

    /// <summary>
    /// 私有构造函数, 初始化历史记录面板
    /// </summary>
    private HistoryPanel()
    {
        // 设置历史记录面板属性
        Dock = DockStyle.Fill;

        // 初始化提示气泡
        _toolTip = UIConstants.ToolTip;

        // 创建提示标签
        tipsLabel = new()
        {
            ForeColor = Color.DarkBlue,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new("Arial", 6 * UIConstants.DpiScale, FontStyle.Regular),
            Text = "点击列头可进行排序和筛选, 点击序号可以删除对应的历史记录"
        };

        // 创建历史记录顶部信息面板
        historyTopPanel = CreateHistoryTopInfoPanel();

        // 创建统计信息数据网格视图
        statisticsDataGridView = CreateStatisticsDataGridView();

        // 创建历史记录数据网格视图
        historyDataGridView = CreateHistoryDataGridView();

        // 创建历史记录右键菜单
        historyContextMenu = CreateHistoryContextMenu();

        // 订阅列头点击事件
        historyDataGridView.ColumnHeaderMouseClick += (sender, e) =>
        {
            // 获取当前列的名字
            var columnName = historyDataGridView.Columns[e.ColumnIndex].Name;

            // 获取右键菜单实例
            var contextMenu = HistoryContextMenu.GetInstance(columnName);

            // 显示右键菜单
            contextMenu.Show(Cursor.Position);
        };

        // 订阅 ResultManager 的条件变化事件
        ResultManager.ConditionsChanged += UpdateHistoryDataGridView;

        // 订阅 CellValueNeeded 事件
        historyDataGridView.CellValueNeeded += OnHistoryDataGridViewCellValueNeeded;

        // 订阅单元格鼠标点击事件
        historyDataGridView.CellMouseClick += (sender, e) =>
        {
            // 检测行索引是否有效
            if (e.RowIndex < 0 || e.RowIndex >= ResultManager.Results.Count)
            {
                return;
            }

            // 只有点击序号列时才显示菜单
            if (historyDataGridView.Columns[e.ColumnIndex].Name == "序号")
            {
                selectedRowIndex = e.RowIndex;
                historyContextMenu.Items[0].Text = $"删除序号 {e.RowIndex + 1} 的游戏记录";
                historyContextMenu.Show(Cursor.Position);
            }
        };

        // 订阅选中变化事件
        historyDataGridView.SelectionChanged += (sender, e) => historyDataGridView.ClearSelection();

        // 添加历史记录顶部面板和数据网格视图到排行榜面板
        Controls.Add(historyTopPanel);
        Controls.Add(statisticsDataGridView);
        Controls.Add(historyDataGridView);
    }

    /// <summary>
    /// 创建历史记录顶部信息面板
    /// </summary>
    /// <returns>返回创建的面板</returns>
    private Panel CreateHistoryTopInfoPanel()
    {
        // 创建历史记录顶部面板
        var panel = new Panel
        {
            Size = new(UIConstants.MainFormWidth, HistoryTopPanelHeight),
            BackColor = Color.LightSalmon,
            Location = new(0, 0)
        };

        // 添加标题标签
        Label titleLabel = new()
        {
            Text = "历史记录",
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new("Arial", 10 * UIConstants.DpiScale, FontStyle.Bold),
            Location = new((int)(5 * UIConstants.DpiScale), 0),
            Size = new((int)(150 * UIConstants.DpiScale), HistoryTopPanelHeight)
        };
        panel.Controls.Add(titleLabel);
        _toolTip.SetToolTip(titleLabel, $"记录在本地文件: {Constants.HistoryFilePath} 的所有游戏结果");

        // 按钮X位置和Y位置
        var buttonXPosition = (int)(170 * UIConstants.DpiScale);
        var buttonYPosition = (int)(8 * UIConstants.DpiScale);

        // 按钮宽度和高度
        var buttonWidth = (int)(120 * UIConstants.DpiScale);
        var buttonHeight = (int)(25 * UIConstants.DpiScale);

        // 按钮水平间距
        var buttonSpacing = (int)(10 * UIConstants.DpiScale);

        // 添加按钮以显示统计信息
        Button btnShowStatistics = new()
        {
            Text = "显示统计信息",
            Size = new(buttonWidth, buttonHeight),
            Location = new(buttonXPosition, buttonYPosition),
            BackColor = Color.LightGreen,
            FlatStyle = FlatStyle.Flat,
            TabStop = false
        };
        btnShowStatistics.Click += (sender, e) => RestartHistoryPanel();
        buttonXPosition += buttonWidth + buttonSpacing;
        panel.Controls.Add(btnShowStatistics);
        _toolTip.SetToolTip(btnShowStatistics, "点击显示各难度游戏结果的统计信息");

        // 添加按钮以显示详细历史记录
        Button btnShowHistory = new()
        {
            Text = "显示详细历史记录",
            Size = new(buttonWidth, buttonHeight),
            Location = new(buttonXPosition, buttonYPosition),
            BackColor = Color.LightBlue,
            FlatStyle = FlatStyle.Flat,
            TabStop = false
        };
        btnShowHistory.Click += (sender, e) => RestartHistoryPanel(true);
        panel.Controls.Add(btnShowHistory);
        _toolTip.SetToolTip(btnShowHistory, "点击详细显示每一条历史记录, 支持排序、筛选和删除, 每次点击会清除所有筛选和排序条件");

        // 提示信息标签左侧起始位置
        var tipsLabelStartX = buttonXPosition + buttonWidth;

        // 更新按钮宽度
        buttonWidth = (int)(70 * UIConstants.DpiScale);

        // 更新按钮X位置
        buttonXPosition = UIConstants.MainFormWidth - buttonWidth - (4 * buttonSpacing);

        // 更新按钮间距
        buttonSpacing *= 3;

        // 添加按钮以返回菜单
        Button btnBackMenu = new()
        {
            Text = "返回菜单",
            Size = new(buttonWidth, buttonHeight),
            Location = new(buttonXPosition, buttonYPosition),
            BackColor = Color.LightCoral,
            FlatStyle = FlatStyle.Flat,
            TabStop = false
        };
        buttonXPosition -= buttonWidth + buttonSpacing;
        btnBackMenu.Click += (sender, e) =>
        {
            MainForm.Instance.SwitchToPanel(PanelType.Menu);
            BottomStatusBar.Instance.SetStatus(StatusBarState.Ready);
        };
        panel.Controls.Add(btnBackMenu);
        _toolTip.SetToolTip(btnBackMenu, "点击返回主菜单");

        // 添加按钮以清除历史记录
        Button btnClearHistory = new()
        {
            Text = "清除历史",
            Size = new(buttonWidth, buttonHeight),
            Location = new(buttonXPosition, buttonYPosition),
            BackColor = Color.Red,
            FlatStyle = FlatStyle.Flat,
            TabStop = false
        };
        btnClearHistory.Click += OnBtnClearHistoryClick;
        panel.Controls.Add(btnClearHistory);
        _toolTip.SetToolTip(btnClearHistory, "点击清除所有历史记录, 注意: 此操作不可恢复");

        // 提示标签宽度
        var tipsLabelWidth = buttonXPosition - tipsLabelStartX;

        // 设置提示标签位置和大小
        tipsLabel.Location = new(tipsLabelStartX, 0);
        tipsLabel.Size = new(tipsLabelWidth, HistoryTopPanelHeight);

        // 添加提示标签到历史记录顶部面板
        panel.Controls.Add(tipsLabel);

        // 返回创建的面板
        return panel;
    }

    /// <summary>
    /// 创建统计信息数据网格视图
    /// </summary>
    /// <returns>返回创建的数据网格视图</returns>
    private static DoubleBufferedDataGridView CreateStatisticsDataGridView()
    {
        // 设置数据网格视图的列头高度
        var columnHeaderHeight = (int)(97 * UIConstants.DpiScale);

        // 设置数据网格视图的行高
        var rowHeight = (int)(110 * UIConstants.DpiScale);

        // 统计信息数据网格视图的高度
        var dataGridViewHeight = UIConstants.MainFormHeight - HistoryTopPanelHeight - BottomStatusBar.Instance.Height - (int)(45 * UIConstants.DpiScale);

        // 创建统计信息数据网格视图
        var dataGridView = new DoubleBufferedDataGridView
        {
            Name = "StatisticsListView",
            BackColor = Color.White,
            ForeColor = Color.Black,
            RowHeadersVisible = false,
            AutoGenerateColumns = false,
            AllowUserToAddRows = false,
            AllowUserToResizeRows = false,
            AllowUserToResizeColumns = false,
            Location = new(0, HistoryTopPanelHeight),
            Size = new(UIConstants.MainFormWidth, dataGridViewHeight),
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
            ColumnHeadersHeight = columnHeaderHeight,
            EnableHeadersVisualStyles = false,
            RowTemplate = { Height = rowHeight }
        };

        // 保存每一列的名字和最小宽度的字典
        var columnDefinitions = new Dictionary<string, int>
        {
            { "难度", 0 },
            { "游戏次数", (int)(150 * UIConstants.DpiScale) },
            { "胜利次数", (int)(150 * UIConstants.DpiScale) },
            { "胜率", (int)(150 * UIConstants.DpiScale) },
            { "平均胜利用时", (int)(200 * UIConstants.DpiScale) },
            { "最短胜利用时", (int)(200 * UIConstants.DpiScale) },
            { "平均完成度", (int)(200 * UIConstants.DpiScale) }
        };

        // 剩余宽度
        var remainWidth = UIConstants.MainFormWidth - columnDefinitions.Values.Sum();
        remainWidth -= (int)(10 * UIConstants.DpiScale);

        // 难度列增加剩余宽度
        columnDefinitions["难度"] += remainWidth;

        // 添加所有列
        foreach (var (name, width) in columnDefinitions)
        {
            // 对齐方式
            var alignment = DataGridViewContentAlignment.MiddleCenter;

            // 创建列
            var column = new DataGridViewTextBoxColumn
            {
                Name = name,
                Width = width,
                ReadOnly = true,
                HeaderText = name,
                DefaultCellStyle = { Alignment = alignment },
                SortMode = DataGridViewColumnSortMode.NotSortable
            };

            // 设置列头对齐方式
            column.HeaderCell.Style.Alignment = alignment;
            column.HeaderCell.Style.Font = new("微软雅黑", 11F, FontStyle.Bold);
            column.HeaderCell.Style.BackColor = Color.LightSteelBlue;
            column.HeaderCell.Style.ForeColor = Color.DarkBlue;

            // 添加列到数据网格视图
            _ = dataGridView.Columns.Add(column);
        }

        // 选择时清除选择
        dataGridView.SelectionChanged += (s, e) => dataGridView.ClearSelection();
        return dataGridView;
    }

    /// <summary>
    /// 创建历史记录数据网格视图
    /// </summary>
    /// <returns>返回创建的历史记录数据网格视图</returns>
    private static DoubleBufferedDataGridView CreateHistoryDataGridView()
    {
        // 设置数据网格视图的列头高度
        var columnHeaderHeight = (int)(32 * UIConstants.DpiScale);

        // 设置数据网格视图的行高
        var rowHeight = (int)(25 * UIConstants.DpiScale);

        // 历史记录数据网格视图的宽度和高度
        var dataGridViewWidth = UIConstants.MainFormWidth - (int)(12 * UIConstants.DpiScale);
        var dataGridViewHeight = UIConstants.MainFormHeight - HistoryTopPanelHeight - BottomStatusBar.Instance.Height - (int)(45 * UIConstants.DpiScale);

        // 创建历史记录数据网格视图
        var dataGridView = new DoubleBufferedDataGridView
        {
            Name = "HistoryListView",
            VirtualMode = true,
            BackColor = Color.White,
            ForeColor = Color.Black,
            RowHeadersVisible = false,
            AutoGenerateColumns = false,
            AllowUserToAddRows = false,
            AllowUserToResizeRows = false,
            AllowUserToResizeColumns = false,
            Location = new(0, HistoryTopPanelHeight),
            Size = new(dataGridViewWidth, dataGridViewHeight),
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
            ColumnHeadersHeight = columnHeaderHeight,
            EnableHeadersVisualStyles = false,
            RowTemplate = { Height = rowHeight }
        };

        // 保存每一列的名字和最小宽度的字典
        var columnDefinitions = new Dictionary<string, int>
        {
            { "序号", (int)(80 * UIConstants.DpiScale) },
            { "开始时间", 0 },
            { "难度", (int)(160 * UIConstants.DpiScale) },
            { "结果", (int)(160 * UIConstants.DpiScale) },
            { "完成度", (int)(160 * UIConstants.DpiScale) },
            { "用时", (int)(160 * UIConstants.DpiScale) },
            { "宽度", (int)(100 * UIConstants.DpiScale) },
            { "高度", (int)(100 * UIConstants.DpiScale) },
            { "地雷数", (int)(100 * UIConstants.DpiScale) }
        };

        // 剩余宽度
        var remainWidth = dataGridViewWidth - columnDefinitions.Values.Sum();
        remainWidth -= (int)(19 * UIConstants.DpiScale);

        // 开始时间列增加剩余宽度
        columnDefinitions["开始时间"] += remainWidth;

        // 添加所有列
        foreach (var (name, width) in columnDefinitions)
        {
            // 对齐方式
            var alignment = DataGridViewContentAlignment.MiddleCenter;

            // 创建列
            var column = new DataGridViewTextBoxColumn
            {
                Name = name,
                Width = width,
                ReadOnly = true,
                HeaderText = name,
                DefaultCellStyle = { Alignment = alignment },
                SortMode = DataGridViewColumnSortMode.NotSortable
            };

            // 设置列头对齐方式
            column.HeaderCell.Style.Alignment = alignment;
            column.HeaderCell.Style.Font = new("微软雅黑", 11F, FontStyle.Bold);
            column.HeaderCell.Style.BackColor = Color.LightSteelBlue;
            column.HeaderCell.Style.ForeColor = Color.DarkBlue;

            // 添加列到数据网格视图
            _ = dataGridView.Columns.Add(column);
        }
        return dataGridView;
    }

    /// <summary>
    /// 创建历史记录右键菜单
    /// </summary>
    /// <returns>历史记录右键菜单</returns>
    private ContextMenuStrip CreateHistoryContextMenu()
    {
        var contextMenu = new ContextMenuStrip();

        // 添加菜单项
        _ = contextMenu.Items.Add("删除记录", null, (s, e) => DeleteHistoryRecord());
        return contextMenu;
    }
}