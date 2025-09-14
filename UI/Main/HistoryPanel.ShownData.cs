using System.Globalization;
using MineClearance.Models;
using MineClearance.Models.Enums;
using MineClearance.Utilities;
using MineClearance.UI.Assist;

namespace MineClearance.UI.Main;

// 历史面板的显示数据部分
internal partial class HistoryPanel
{
    /// <summary>
    /// 记录当前选中的行索引
    /// </summary>
    private int selectedRowIndex = -1;

    /// <summary>
    /// 统计信息结构体
    /// </summary>
    private readonly struct Stats
    {
        /// <summary>
        /// 总游戏次数
        /// </summary>
        public int Total { get; init; }

        /// <summary>
        /// 胜利次数
        /// </summary>
        public int Wins { get; init; }

        /// <summary>
        /// 总胜利用时
        /// </summary>
        public TimeSpan TotalDuration { get; init; }

        /// <summary>
        /// 最短胜利用时
        /// </summary>
        public TimeSpan ShortestDuration { get; init; }

        /// <summary>
        /// 总完成度
        /// </summary>
        public double TotalCompletion { get; init; }
    }

    /// <summary>
    /// 重启历史记录面板
    /// </summary>
    /// <param name="showHistory">是否显示详细历史记录</param>
    public void RestartHistoryPanel(bool showHistory = false)
    {
        // 如果是显示详细历史记录，则显示历史记录数据网格视图
        if (showHistory)
        {
            // 提示信息标签可见
            tipsLabel.Visible = true;

            // 切换到历史记录数据网格视图
            statisticsDataGridView.Visible = false;
            historyDataGridView.Visible = true;

            // 重置右键菜单实例(会自动更新历史记录数据网格视图)
            HistoryContextMenu.ResetInstances();
        }
        else
        {
            // 提示信息标签不可见
            tipsLabel.Visible = false;

            // 切换到统计信息数据网格视图
            historyDataGridView.Visible = false;
            statisticsDataGridView.Visible = true;

            // 更新统计信息数据网格视图
            UpdateStatisticsDataGridView();
        }
    }

    /// <summary>
    /// 更新统计信息数据网格视图
    /// </summary>
    private void UpdateStatisticsDataGridView()
    {
        // 获取所有游戏结果
        var gameResults = ResultManager.OriginalResults;

        // 清空统计信息数据网格视图
        statisticsDataGridView.Rows.Clear();

        // 全部难度的游戏结果统计
        var allDifficultyStats = new Stats() { ShortestDuration = TimeSpan.MaxValue };

        // 使用字典来统计每个难度的游戏结果
        var difficultyStats = new Dictionary<DifficultyLevel, Stats>
        {
            { DifficultyLevel.Easy, new Stats() { ShortestDuration = TimeSpan.MaxValue } },
            { DifficultyLevel.Medium, new Stats() { ShortestDuration = TimeSpan.MaxValue } },
            { DifficultyLevel.Hard, new Stats() { ShortestDuration = TimeSpan.MaxValue } },
            { DifficultyLevel.Hell, new Stats() { ShortestDuration = TimeSpan.MaxValue } },
            { DifficultyLevel.Custom, new Stats() { ShortestDuration = TimeSpan.MaxValue } }
        };

        // 统计游戏结果
        foreach (var result in gameResults)
        {
            // 更新全部难度的游戏结果
            allDifficultyStats = AddResultToStats(result, allDifficultyStats);

            // 更新对应难度的游戏结果
            difficultyStats[result.Difficulty] = AddResultToStats(result, difficultyStats[result.Difficulty]);
        }

        // 添加全部难度的统计信息
        AddStatsToStatisticsDataGridView("全部", allDifficultyStats);

        // 添加每个难度的统计信息
        foreach (var difficulty in difficultyStats.Keys)
        {
            var difficultyText = UIMethods.GetDifficultyText(difficulty);
            AddStatsToStatisticsDataGridView(difficultyText, difficultyStats[difficulty]);
        }

        // 重绘统计信息数据网格视图
        statisticsDataGridView.Invalidate();
    }

    /// <summary>
    /// 将游戏结果添加到stats结构体
    /// </summary>
    /// <param name="result">要添加的游戏结果</param>
    /// <param name="stats">要更新的统计信息</param>
    /// <returns>返回更新后的统计信息</returns>
    private static Stats AddResultToStats(GameResult result, Stats stats)
    {
        // 更新统计信息
        return new()
        {
            Total = stats.Total + 1,
            Wins = stats.Wins + (result.IsWin ? 1 : 0),
            TotalDuration = stats.TotalDuration + (result.IsWin ? result.Duration : TimeSpan.Zero),
            TotalCompletion = stats.TotalCompletion + (result.Completion ?? 100.0),
            ShortestDuration = (result.IsWin && result.Duration < stats.ShortestDuration) ? result.Duration : stats.ShortestDuration
        };
    }

    /// <summary>
    /// 将stats结构体添加到统计信息数据网格视图
    /// </summary>
    /// <param name="difficultyText">要添加的难度文本</param>
    /// <param name="stats">要添加的统计信息</param>
    private void AddStatsToStatisticsDataGridView(string difficultyText, Stats stats)
    {
        // 计算胜率、平均胜利用时和平均完成度
        var winRate = stats.Total > 0 ? (double)stats.Wins / stats.Total * 100 : 0;
        var avgDuration = stats.Wins > 0 ? TimeSpan.FromMilliseconds(stats.TotalDuration.TotalMilliseconds / stats.Wins) : TimeSpan.Zero;
        var avgCompletion = stats.Total > 0 ? stats.TotalCompletion / stats.Total : 0;

        // 格式化用时为 xx:xx.xx 格式
        var formattedDuration = $"{(int)avgDuration.TotalMinutes:D2}:{avgDuration.Seconds:D2}.{avgDuration.Milliseconds / 10:D2}";

        // 格式化最短胜利用时为 xx:xx.xx 格式
        var formattedShortestDuration = stats.ShortestDuration == TimeSpan.MaxValue ? "无" : $"{(int)stats.ShortestDuration.TotalMinutes:D2}:{stats.ShortestDuration.Seconds:D2}.{stats.ShortestDuration.Milliseconds / 10:D2}";

        // 添加当前难度的统计信息到数据网格视图
        _ = statisticsDataGridView.Rows.Add(
            difficultyText,
            stats.Total,
            stats.Wins,
            $"{winRate:0.##}%",
            formattedDuration,
            formattedShortestDuration,
            $"{avgCompletion:0.##}%"
        );
    }

    /// <summary>
    /// 更新历史记录数据网格视图
    /// </summary>
    private void UpdateHistoryDataGridView()
    {
        historyDataGridView.RowCount = ResultManager.Results.Count;
        historyDataGridView.Invalidate();

        // 滚动到最顶部
        if (historyDataGridView.RowCount > 0)
        {
            historyDataGridView.FirstDisplayedScrollingRowIndex = 0;
        }
    }

    /// <summary>
    /// DataGridView 的单元格值需要时触发的事件
    /// </summary>
    private void OnHistoryDataGridViewCellValueNeeded(object? sender, DataGridViewCellValueEventArgs e)
    {
        // 检测行索引是否有效
        if (e.RowIndex < 0 || e.RowIndex >= ResultManager.Results.Count)
        {
            return;
        }

        // 检测列索引是否有效
        if (e.ColumnIndex < 0 || e.ColumnIndex >= historyDataGridView.Columns.Count)
        {
            return;
        }

        // 获取当前行的游戏结果
        var result = ResultManager.Results[e.RowIndex];

        // 根据列索引设置单元格值
        e.Value = historyDataGridView.Columns[e.ColumnIndex].Name switch
        {
            "序号" => e.RowIndex + 1,
            "开始时间" => result.StartTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
            "难度" => UIMethods.GetDifficultyText(result.Difficulty),
            "结果" => result.IsWin ? "胜利" : "失败",
            "完成度" => $"{result.Completion ?? 100.0:0.##}%",
            "用时" => $"{(int)result.Duration.TotalMinutes:D2}:{result.Duration.Seconds:D2}.{result.Duration.Milliseconds / 10:D2}",
            "宽度" => result.BoardWidth ?? Constants.GetSettings(result.Difficulty).width,
            "高度" => result.BoardHeight ?? Constants.GetSettings(result.Difficulty).height,
            "地雷数" => result.MineCount ?? Constants.GetSettings(result.Difficulty).mineCount,
            _ => "",
        };
    }

    /// <summary>
    /// 删除选中的历史记录
    /// </summary>
    private async void DeleteHistoryRecord()
    {
        if (selectedRowIndex >= 0 && selectedRowIndex < ResultManager.Results.Count)
        {
            var confirmResult = CustomMessageBox.Show("确定要删除选中的历史记录吗？\n注意: 一旦删除将无法找回！！！", "删除历史记录", "删除指定历史记录");
            if (confirmResult == DialogResult.Yes)
            {
                await ResultManager.RemoveResultAt(selectedRowIndex);
                selectedRowIndex = -1;
            }
        }
    }

    /// <summary>
    /// 清除历史记录按钮点击事件处理
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="e">事件参数</param>
    private async void OnBtnClearHistoryClick(object? sender, EventArgs e)
    {
        // 添加确认对话框
        var confirmResult = MessageBox.Show("确定要清除所有历史记录吗？\n注意: 一旦清除将无法找回！！！", "清除历史记录", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);

        // 用户选择取消，直接返回
        if (confirmResult != DialogResult.Yes)
        {
            return;
        }

        // 清空历史记录数据
        await ResultManager.ClearAllResultsAsync();

        // 弹窗提示清除成功
        _ = MessageBox.Show("历史记录已清除！", "清除成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}