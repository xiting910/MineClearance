using System.Globalization;

namespace MineClearance.UI.Assist;

/// <summary>
/// 提供自定义日期选择对话框
/// </summary>
internal sealed class DatePickerDialog : Form
{
    /// <summary>
    /// 获取选择的日期列表(只读)
    /// </summary>
    public IReadOnlyList<DateTime> SelectedDates => selectedDates.ToList().AsReadOnly();

    /// <summary>
    /// 已选择的日期列表ListBox
    /// </summary>
    private readonly ListBox selectedDatesListBox;

    /// <summary>
    /// 按钮集合
    /// </summary>
    private readonly List<Button> buttons;

    /// <summary>
    /// 提示信息标签
    /// </summary>
    private readonly Label infoLabel;

    /// <summary>
    /// 日期选择器
    /// </summary>
    private readonly DateTimePicker dateTimePicker;

    /// <summary>
    /// 选择的日期列表
    /// </summary>
    private readonly HashSet<DateTime> selectedDates;

    /// <summary>
    /// 提示气泡
    /// </summary>
    private readonly ToolTip toolTip;

    /// <summary>
    /// 构造函数
    /// </summary>
    public DatePickerDialog()
    {
        // 对话框大小
        var dialogWidth = (int)(500 * UIConstants.DpiScale);
        var dialogHeight = (int)(400 * UIConstants.DpiScale);

        // 设置对话框属性
        Text = "选择日期";
        Size = new(dialogWidth, dialogHeight);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        // 已选择的日期列表框大小
        var listBoxWidth = (int)(480 * UIConstants.DpiScale);
        var listBoxHeight = (int)(280 * UIConstants.DpiScale);

        // 初始化已选择的日期列表框
        selectedDatesListBox = new ListBox
        {
            Location = new((int)(5 * UIConstants.DpiScale), (int)(5 * UIConstants.DpiScale)),
            Size = new(listBoxWidth, listBoxHeight),
            SelectionMode = SelectionMode.MultiExtended
        };

        // 添加提示信息
        _ = selectedDatesListBox.Items.Add("当前已选择的日期:");

        // 初始化添加按钮
        var addButton = new Button { Text = "添加" };
        addButton.Click += AddButton_Click;

        // 初始化删除按钮
        var removeButton = new Button { Text = "删除" };
        removeButton.Click += RemoveButton_Click;

        // 初始化确定按钮
        var okButton = new Button
        {
            Text = "确定",
            DialogResult = DialogResult.OK
        };

        // 初始化取消按钮
        var cancelButton = new Button
        {
            Text = "取消",
            DialogResult = DialogResult.Cancel
        };

        // 绑定 Accept 和 Cancel 按钮
        AcceptButton = okButton;
        CancelButton = cancelButton;

        // 添加按钮到列表
        buttons = [addButton, removeButton, okButton, cancelButton];

        // 按钮的水平间距
        var buttonSpacing = (int)(10 * UIConstants.DpiScale);

        // 按钮的宽度和高度
        var buttonWidth = (dialogWidth - ((buttons.Count + 2) * buttonSpacing)) / buttons.Count;
        var buttonHeight = (int)(25 * UIConstants.DpiScale);

        // 按钮位置
        var buttonX = buttonSpacing;
        var buttonY = dialogHeight - buttonHeight - (int)(50 * UIConstants.DpiScale);

        // 设置每个按钮的位置和大小
        foreach (var button in buttons)
        {
            button.Size = new(buttonWidth, buttonHeight);
            button.Location = new(buttonX, buttonY);
            buttonX += buttonWidth + buttonSpacing;
        }

        // 日期选择器的大小
        var datePickerWidth = (int)(300 * UIConstants.DpiScale);
        var datePickerHeight = (int)(15 * UIConstants.DpiScale);

        // 信息提示标签的大小
        var infoLabelWidth = (int)(100 * UIConstants.DpiScale);
        var infoLabelHeight = (int)(15 * UIConstants.DpiScale);

        // 间隔宽度
        var spacing = (int)(10 * UIConstants.DpiScale);

        // 当前X和Y位置
        var currentX = (dialogWidth - datePickerWidth - infoLabelWidth - spacing) / 2;
        var currentY = buttonY - datePickerHeight - spacing;

        // 初始化信息提示标签
        infoLabel = new Label
        {
            Text = "要添加的日期:",
            Location = new(currentX, currentY),
            Size = new(infoLabelWidth, infoLabelHeight),
            TextAlign = ContentAlignment.MiddleLeft
        };
        currentX += infoLabelWidth + spacing;

        // 初始化日期选择器
        dateTimePicker = new DateTimePicker
        {
            Location = new(currentX, currentY),
            Size = new(datePickerWidth, datePickerHeight),
            Format = DateTimePickerFormat.Long,
            Value = DateTime.Now
        };

        // 添加控件到对话框
        Controls.Add(selectedDatesListBox);
        Controls.AddRange([.. buttons]);
        Controls.Add(infoLabel);
        Controls.Add(dateTimePicker);

        // 初始化选择的日期列表
        selectedDates = [];

        // 初始化提示气泡
        toolTip = UIConstants.ToolTip;

        // 设置提示信息
        toolTip.SetToolTip(addButton, "添加输入的日期");
        toolTip.SetToolTip(removeButton, "删除列表框中选中的日期");
        toolTip.SetToolTip(okButton, "完成选择, 并应用所选全部日期");
        toolTip.SetToolTip(cancelButton, "取消操作");
    }

    /// <summary>
    /// 添加按钮点击事件处理
    /// </summary>
    private void AddButton_Click(object? sender, EventArgs e)
    {
        // 添加选择的日期到列表
        if (selectedDates.Add(dateTimePicker.Value.Date))
        {
            // 如果添加成功, 则更新列表框
            _ = selectedDatesListBox.Items.Add(dateTimePicker.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        }
    }

    /// <summary>
    /// 删除按钮点击事件处理
    /// </summary>
    private void RemoveButton_Click(object? sender, EventArgs e)
    {
        // 获取选中的索引
        var selectedIndices = selectedDatesListBox.SelectedIndices;

        // 从后往前删除，避免索引错乱
        for (var i = selectedIndices.Count - 1; i >= 0; --i)
        {
            // 获取当前索引
            var idx = selectedIndices[i];

            // 如果是第一个提示信息, 则跳过
            if (idx == 0)
            {
                continue;
            }

            // 从 HashSet 中移除对应的日期
            if (DateTime.TryParse(selectedDatesListBox.Items[idx].ToString(), out var date))
            {
                _ = selectedDates.Remove(date);
            }

            // 从 ListBox 移除
            selectedDatesListBox.Items.RemoveAt(idx);
        }
    }
}