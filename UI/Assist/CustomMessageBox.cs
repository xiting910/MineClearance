namespace MineClearance.UI.Assist;

/// <summary>
/// 自定义确认对话框, 支持不再提示勾选框, 以及不同按钮的唯一标识
/// </summary>
internal sealed class CustomMessageBox : Form
{
    /// <summary>
    /// 保存每个弹窗的"不再显示"状态
    /// </summary>
    private static readonly Dictionary<string, bool> _doNotShowAgainDict = [];

    /// <summary>
    /// 不再提示勾选框
    /// </summary>
    private CheckBox? _doNotShowAgainCheckBox;

    /// <summary>
    /// 显示自定义消息框, 如果该key已设置为不再显示, 直接返回Yes
    /// </summary>
    /// <param name="message">消息内容</param>
    /// <param name="caption">标题</param>
    /// <param name="key">唯一标识（如按钮名）</param>
    public static DialogResult Show(string message, string caption, string key)
    {
        // 如果该key已设置为不再显示, 直接返回Yes
        if (_doNotShowAgainDict.TryGetValue(key, out var skip) && skip)
        {
            return DialogResult.Yes;
        }

        // 创建自定义消息框实例
        using var form = new CustomMessageBox();
        form.Text = caption;

        // 设置消息内容和按钮
        var label = new Label
        {
            Text = message,
            AutoSize = true,
            Location = new((int)(5 * UIConstants.DpiScale), (int)(5 * UIConstants.DpiScale))
        };
        form.Controls.Add(label);

        // 创建"本次运行不再提示"勾选框
        form._doNotShowAgainCheckBox = new CheckBox
        {
            Text = "本次运行不再提示",
            Location = new((int)(5 * UIConstants.DpiScale), label.Bottom + (int)(10 * UIConstants.DpiScale)),
            AutoSize = true
        };
        form.Controls.Add(form._doNotShowAgainCheckBox);

        // 创建"是"和"否"按钮
        var btnYes = new Button
        {
            Text = "是",
            DialogResult = DialogResult.Yes,
            AutoSize = true
        };
        var btnNo = new Button
        {
            Text = "否",
            DialogResult = DialogResult.No,
            AutoSize = true
        };
        form.Controls.Add(btnYes);
        form.Controls.Add(btnNo);

        // 绑定 Accept 和 Cancel 按钮
        form.AcceptButton = btnYes;
        form.CancelButton = btnNo;

        // 计算对话框的宽度和高度
        var width = Math.Max(label.Width + (int)(10 * UIConstants.DpiScale), form._doNotShowAgainCheckBox.Width + btnYes.Width + btnNo.Width + (int)(40 * UIConstants.DpiScale));
        var height = form._doNotShowAgainCheckBox.Bottom + (int)(15 * UIConstants.DpiScale);

        // 设置按钮位置
        btnNo.Location = new(width - btnNo.Width - (int)(10 * UIConstants.DpiScale), label.Bottom + (int)(10 * UIConstants.DpiScale));
        btnYes.Location = new(btnNo.Left - btnYes.Width - (int)(10 * UIConstants.DpiScale), btnNo.Top);

        // 设置对话框的大小和位置
        form.ClientSize = new(width, height);
        form.StartPosition = FormStartPosition.CenterScreen;
        form.FormBorderStyle = FormBorderStyle.FixedDialog;
        form.MaximizeBox = false;
        form.MinimizeBox = false;

        // 显示对话框
        var result = form.ShowDialog();

        // 如果勾选了"不再提示", 设置当前key为不再显示
        if (form._doNotShowAgainCheckBox.Checked)
        {
            _doNotShowAgainDict[key] = true;
        }

        // 返回用户选择的结果
        return result;
    }
}