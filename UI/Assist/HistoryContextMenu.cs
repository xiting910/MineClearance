using MineClearance.Models;
using MineClearance.Models.Enums;
using MineClearance.Utilities;

namespace MineClearance.UI.Assist;

/// <summary>
/// 历史记录面板的右键菜单
/// </summary>
internal sealed class HistoryContextMenu : ContextMenuStrip
{
    /// <summary>
    /// 存储每个列名对应的右键菜单单例的字典
    /// </summary>
    private static readonly Dictionary<string, HistoryContextMenu> _instances = [];

    /// <summary>
    /// 获取或创建指定列名的右键菜单实例
    /// </summary>
    /// <param name="name">当前列的名字</param>
    /// <returns>返回对应列名的右键菜单实例</returns>
    public static HistoryContextMenu GetInstance(string name)
    {
        // 如果字典中已经存在该列名的实例，则直接返回
        if (_instances.TryGetValue(name, out var instance))
        {
            return instance;
        }

        // 否则创建一个新的实例并添加到字典中
        _instances[name] = new(name);
        return _instances[name];
    }

    /// <summary>
    /// 重置所有列名的右键菜单实例
    /// </summary>
    public static void ResetInstances()
    {
        // 清空字典中的所有实例
        foreach (var instance in _instances.Values)
        {
            instance.Dispose();
        }
        _instances.Clear();

        // 清除 ResultManager 的所有筛选和排序条件
        ResultManager.ClearConditions();
    }

    /// <summary>
    /// 构造函数, 初始化历史记录面板的右键菜单
    /// </summary>
    private HistoryContextMenu(string name)
    {
        // 排序菜单项
        var sortMenuItem = CreateSortMenuItems(name);
        if (sortMenuItem != null)
        {
            // 添加排序菜单项
            _ = Items.Add(sortMenuItem);
        }

        // 筛选菜单项
        var filterMenuItem = CreateFilterMenuItems(name);
        if (filterMenuItem != null)
        {
            // 添加筛选菜单项
            _ = Items.Add(filterMenuItem);
        }
    }

    /// <summary>
    /// 创建升序和降序菜单项
    /// </summary>
    /// <param name="name">要排序的属性名</param>
    /// <returns>返回包含升序和降序菜单项的 ToolStripMenuItem</returns>
    private static ToolStripMenuItem? CreateSortMenuItems(string name)
    {
        try
        {
            // 当前属性比较器优先级
            var priority = UIMethods.GetSortPriority(name);

            // 创建排序菜单项
            var sortMenuItem = new ToolStripMenuItem("排序(优先级越小的排序条件越先应用)");

            // 创建升序和降序菜单项
            var ascItem = new ToolStripMenuItem($"按{name}升序, 优先级{priority}") { CheckOnClick = true };
            var descItem = new ToolStripMenuItem($"按{name}降序, 优先级{priority}") { CheckOnClick = true };

            // 订阅升序菜单项的选中状态变化
            ascItem.CheckedChanged += (sender, e) =>
            {
                // 选中升序
                if (ascItem.Checked)
                {
                    // 设置降序菜单项为未选中状态
                    descItem.Checked = false;

                    // 添加升序排序条件
                    ResultManager.AddSortCondition(new GameResultComparer(name, SortOrder.Ascending), priority);
                }
                else
                {
                    // 如果升序未选中，则移除对应的排序条件
                    ResultManager.RemoveSortCondition(priority);
                }
            };

            // 订阅降序菜单项的选中状态变化
            descItem.CheckedChanged += (sender, e) =>
            {
                // 选中降序
                if (descItem.Checked)
                {
                    // 设置升序菜单项为未选中状态
                    ascItem.Checked = false;

                    // 添加降序排序条件
                    ResultManager.AddSortCondition(new GameResultComparer(name, SortOrder.Descending), priority);
                }
                else
                {
                    // 如果降序未选中，则移除对应的排序条件
                    ResultManager.RemoveSortCondition(priority);
                }
            };

            // 将升序和降序菜单项添加到排序菜单项中并返回
            _ = sortMenuItem.DropDownItems.Add(ascItem);
            _ = sortMenuItem.DropDownItems.Add(descItem);
            return sortMenuItem;
        }
        catch (ArgumentException)
        {
            // 如果获取优先级失败，返回 null
            return null;
        }
    }

    /// <summary>
    /// 创建筛选菜单项
    /// </summary>
    /// <param name="name">要筛选的属性名</param>
    /// <returns>返回包含筛选菜单项的 ToolStripMenuItem</returns>
    private static ToolStripMenuItem? CreateFilterMenuItems(string name)
    {
        // 创建筛选菜单项
        var filterMenuItem = new ToolStripMenuItem("筛选(对于同一个属性的多个筛选条件, 只需要满足其中之一; 对于不同属性的筛选条件, 需要同时满足)");

        // 如果是难度, 则添加所有难度选项
        if (name == "难度")
        {
            foreach (var difficulty in Enum.GetValues<DifficultyLevel>())
            {
                // 格式化难度名称
                var difficultyName = UIMethods.GetDifficultyText(difficulty);

                // 创建筛选菜单项
                var filterItem = new ToolStripMenuItem($"筛选: {difficultyName}")
                {
                    CheckOnClick = true
                };

                // 订阅筛选菜单项的选中状态变化
                filterItem.CheckedChanged += (sender, e) =>
                {
                    // 筛选条件
                    bool filter(GameResult result)
                    {
                        return result.Difficulty == difficulty;
                    }

                    // 选中筛选
                    if (filterItem.Checked)
                    {
                        // 添加筛选条件
                        ResultManager.AddFilterCondition(filter, name);
                    }
                    else
                    {
                        // 移除筛选条件
                        ResultManager.RemoveFilterCondition(filter);
                    }
                };

                // 将筛选菜单项添加到筛选菜单项中
                _ = filterMenuItem.DropDownItems.Add(filterItem);
            }
            return filterMenuItem;
        }
        // 如果是结果, 则添加胜利和失败选项
        else if (name == "结果")
        {
            var winItem = new ToolStripMenuItem("筛选: 胜利") { CheckOnClick = true };
            var loseItem = new ToolStripMenuItem("筛选: 失败") { CheckOnClick = true };

            // 订阅筛选菜单项的选中状态变化
            winItem.CheckedChanged += (sender, e) =>
            {
                // 胜利筛选条件
                bool filter(GameResult result)
                {
                    return result.IsWin;
                }

                // 选中胜利筛选
                if (winItem.Checked)
                {
                    // 添加胜利筛选条件
                    ResultManager.AddFilterCondition(filter, name);
                }
                else
                {
                    // 移除胜利筛选条件
                    ResultManager.RemoveFilterCondition(filter);
                }
            };
            loseItem.CheckedChanged += (sender, e) =>
            {
                // 失败筛选条件
                bool filter(GameResult result)
                {
                    return !result.IsWin;
                }

                // 选中失败筛选
                if (loseItem.Checked)
                {
                    // 添加失败筛选条件
                    ResultManager.AddFilterCondition(filter, name);
                }
                else
                {
                    // 移除失败筛选条件
                    ResultManager.RemoveFilterCondition(filter);
                }
            };

            // 将筛选菜单项添加到筛选菜单项中
            _ = filterMenuItem.DropDownItems.Add(winItem);
            _ = filterMenuItem.DropDownItems.Add(loseItem);
            return filterMenuItem;
        }
        // 如果是开始时间, 则添加日期选择菜单项
        else if (name == "开始时间")
        {
            // 创建日期选择菜单项
            var dateFilterItem = new ToolStripMenuItem("选择日期") { CheckOnClick = true };

            // 筛选条件委托
            Func<GameResult, bool>? filter = null;
            dateFilterItem.CheckedChanged += (sender, e) =>
            {
                // 如果选中筛选
                if (dateFilterItem.Checked)
                {
                    using var datePicker = new DatePickerDialog();
                    if (datePicker.ShowDialog() == DialogResult.OK)
                    {
                        // 获取选择的日期
                        var selectedDates = datePicker.SelectedDates;
                        if (selectedDates.Count > 0)
                        {
                            // 筛选条件
                            filter = result => selectedDates.Contains(result.StartTime.Date);

                            // 添加筛选条件
                            ResultManager.AddFilterCondition(filter, name);
                            return;
                        }
                    }

                    // 如果没有选择日期, 则取消选中状态
                    dateFilterItem.Checked = false;
                }
                else
                {
                    // 移除筛选条件
                    if (filter != null)
                    {
                        ResultManager.RemoveFilterCondition(filter);
                        filter = null;
                    }
                }
            };

            // 将日期选择菜单项添加到筛选菜单项中并返回
            _ = filterMenuItem.DropDownItems.Add(dateFilterItem);
            return filterMenuItem;
        }

        return null;
    }
}