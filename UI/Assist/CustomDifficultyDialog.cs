using MineClearance.Utilities;

namespace MineClearance.UI.Assist;

/// <summary>
/// 提供自定义难度设置对话框
/// </summary>
internal sealed class CustomDifficultyDialog : Form
{
    /// <summary>
    /// 自定义难度设置, 包含宽度、高度和地雷数
    /// </summary>
    public (int width, int height, int mineCount) CustomDifficulty { get; private set; }

    /// <summary>
    /// 宽度输入框
    /// </summary>
    private readonly NumericUpDown widthInput;

    /// <summary>
    /// 高度输入框
    /// </summary>
    private readonly NumericUpDown heightInput;

    /// <summary>
    /// 地雷数输入框
    /// </summary>
    private readonly NumericUpDown mineCountInput;

    /// <summary>
    /// 确定按钮
    /// </summary>
    private readonly Button okButton;

    /// <summary>
    /// 取消按钮
    /// </summary>
    private readonly Button cancelButton;

    /// <summary>
    /// 提示气泡
    /// </summary>
    private readonly ToolTip toolTip;

    /// <summary>
    /// 构造函数, 初始化自定义难度设置对话框
    /// </summary>
    public CustomDifficultyDialog()
    {
        // 对话框宽度和高度
        var dialogWidth = (int)(250 * UIConstants.DpiScale);
        var dialogHeight = (int)(175 * UIConstants.DpiScale);

        // 初始化控件和布局
        Text = "自定义难度";
        Size = new(dialogWidth, dialogHeight);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        // 输入标签和输入框的宽度、高度和位置
        var inputLabelWidth = (int)(60 * UIConstants.DpiScale);
        var inputWidth = (int)(80 * UIConstants.DpiScale);
        var inputHeight = (int)(25 * UIConstants.DpiScale);
        var inputX = (dialogWidth - inputLabelWidth - inputWidth) / 2;
        var inputY = (int)(15 * UIConstants.DpiScale);

        // 两个输入框之间的垂直间距
        var verticalSpacing = (int)(25 * UIConstants.DpiScale);

        // 宽度、高度和地雷数默认值
        var defaultWidth = 16;
        var defaultHeight = 16;
        var defaultMineCount = 40;

        // 创建宽度输入标签和输入框
        var widthLabel = new Label
        {
            Text = "宽度:",
            Location = new(inputX, inputY),
            Size = new(inputLabelWidth, inputHeight)
        };
        widthInput = new()
        {
            Location = new(inputX + inputLabelWidth, inputY),
            Size = new(inputWidth, inputHeight),
            Value = defaultWidth,
            Maximum = Constants.MaxBoardWidth,
            Minimum = 1
        };
        widthInput.ValueChanged += OnDimensionInputValueChanged;
        inputY += verticalSpacing;

        // 创建高度输入标签和输入框
        var heightLabel = new Label
        {
            Text = "高度:",
            Location = new(inputX, inputY),
            Size = new(inputLabelWidth, inputHeight)
        };
        heightInput = new()
        {
            Location = new(inputX + inputLabelWidth, inputY),
            Size = new(inputWidth, inputHeight),
            Value = defaultHeight,
            Maximum = Constants.MaxBoardHeight,
            Minimum = 1
        };
        heightInput.ValueChanged += OnDimensionInputValueChanged;
        inputY += verticalSpacing;

        // 创建地雷数输入标签和输入框
        var mineLabel = new Label
        {
            Text = "地雷数:",
            Location = new(inputX, inputY),
            Size = new(inputLabelWidth, inputHeight)
        };
        mineCountInput = new()
        {
            Location = new(inputX + inputLabelWidth, inputY),
            Size = new(inputWidth, inputHeight),
            Value = defaultMineCount,
            Maximum = (defaultWidth * defaultHeight) - 1,
            Minimum = 1
        };

        // 创建确定按钮
        okButton = new()
        {
            Text = "确定",
            Location = new(dialogWidth - (int)(120 * UIConstants.DpiScale), dialogHeight - (int)(65 * UIConstants.DpiScale)),
            Size = new((int)(40 * UIConstants.DpiScale), (int)(25 * UIConstants.DpiScale)),
            DialogResult = DialogResult.OK
        };

        // 创建取消按钮
        cancelButton = new()
        {
            Text = "取消",
            Location = new(dialogWidth - (int)(60 * UIConstants.DpiScale), dialogHeight - (int)(65 * UIConstants.DpiScale)),
            Size = new((int)(40 * UIConstants.DpiScale), (int)(25 * UIConstants.DpiScale)),
            DialogResult = DialogResult.Cancel
        };

        // 绑定 Accept 和 Cancel 按钮
        AcceptButton = okButton;
        CancelButton = cancelButton;

        // 添加OK按钮的点击事件处理
        okButton.Click += OnOkButtonClick;

        // 添加控件到窗体
        Controls.AddRange([widthLabel, widthInput, heightLabel, heightInput, mineLabel, mineCountInput, okButton, cancelButton]);

        // 初始化提示气泡
        toolTip = UIConstants.ToolTip;

        // 设置提示信息
        toolTip.SetToolTip(widthInput, $"请输入游戏区域的宽度, 范围: {widthInput.Minimum} - {widthInput.Maximum}");
        toolTip.SetToolTip(heightInput, $"请输入游戏区域的高度, 范围: {heightInput.Minimum} - {heightInput.Maximum}");
        toolTip.SetToolTip(mineCountInput, $"请输入地雷的数量, 范围: {mineCountInput.Minimum} - {mineCountInput.Maximum}");
        toolTip.SetToolTip(okButton, "点击以当前设置开始游戏");
        toolTip.SetToolTip(cancelButton, "点击取消选择并关闭对话框, 返回选择难度界面");
    }

    /// <summary>
    /// 宽度或者高度输入框的值改变事件处理
    /// </summary>
    /// <param name="sender">事件源</param>
    /// <param name="e">事件参数</param>
    private void OnDimensionInputValueChanged(object? sender, EventArgs e)
    {
        // 获取当前输入框的值
        var width = (int)widthInput.Value;
        var height = (int)heightInput.Value;

        // 允许的地雷数最大值
        var maxMineCount = (width * height) - 1;

        // 如果允许的地雷数最大值小于1, 则取消本次更改
        if (maxMineCount < 1)
        {
            widthInput.ValueChanged -= OnDimensionInputValueChanged;
            heightInput.ValueChanged -= OnDimensionInputValueChanged;
            widthInput.Value = CustomDifficulty.width;
            heightInput.Value = CustomDifficulty.height;
            widthInput.ValueChanged += OnDimensionInputValueChanged;
            heightInput.ValueChanged += OnDimensionInputValueChanged;

            // 显示错误提示
            _ = MessageBox.Show("宽度和高度的乘积必须大于1！", "输入错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // 更新地雷数的最大值
        mineCountInput.Maximum = maxMineCount;

        // 更新提示信息
        toolTip.SetToolTip(mineCountInput, $"请输入地雷的数量, 范围: {mineCountInput.Minimum} - {mineCountInput.Maximum}");

        // 更新当前设置
        CustomDifficulty = (width, height, 0);
    }

    /// <summary>
    /// 确定按钮点击事件处理
    /// </summary>
    /// <param name="sender">事件源</param>
    /// <param name="e">事件参数</param>
    private void OnOkButtonClick(object? sender, EventArgs e)
    {
        var width = (int)widthInput.Value;
        var height = (int)heightInput.Value;
        var mineCount = (int)mineCountInput.Value;

        // 验证地雷数不能超过或者等于总格子数
        if (mineCount >= width * height)
        {
            _ = MessageBox.Show("地雷数必须小于总格子数！", "输入错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        CustomDifficulty = (width, height, mineCount);
        DialogResult = DialogResult.OK;
        Close();
    }
}