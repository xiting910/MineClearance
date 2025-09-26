using MineClearance.Models.Enums;
using MineClearance.Services;

namespace MineClearance.UI.Main;

/// <summary>
/// 设置窗口类
/// </summary>
internal sealed partial class SettingForm : Form
{
    /// <summary>
    /// 设置窗口实例
    /// </summary>
    private static SettingForm? _instance;

    /// <summary>
    /// 显示设置窗口
    /// </summary>
    public static void ShowForm()
    {
        // 如果实例已存在且未释放, 则直接显示
        if (_instance is not null && !_instance.IsDisposed)
        {
            // 如果当前窗口状态为最小化, 则恢复到正常状态
            if (_instance.WindowState == FormWindowState.Minimized)
            {
                _instance.WindowState = FormWindowState.Normal;
            }

            // 如果当前窗口没有获得焦点, 则激活窗口
            if (!_instance.ContainsFocus)
            {
                _instance.Activate();
            }
            return;
        }

        // 创建新的实例并显示
        _instance = new();
        _instance.Show();
    }

    /// <summary>
    /// 日志最低记录级别提示标签
    /// </summary>
    private readonly Label _logLevelLabel;

    /// <summary>
    /// 日志最低记录级别选择框
    /// </summary>
    private readonly ComboBox _logLevelComboBox;

    /// <summary>
    /// 程序退出后等待日志写入的最长时间提示标签
    /// </summary>
    private readonly Label _logFlushDelayLabel;

    /// <summary>
    /// 程序退出后等待日志写入的最长时间
    /// </summary>
    private readonly NumericUpDown _logFlushDelayNumericUpDown;

    /// <summary>
    /// 日志文件最多保留时间提示标签
    /// </summary>
    private readonly Label _logRetentionLabel;

    /// <summary>
    /// 日志文件最多保留时间
    /// </summary>
    private readonly NumericUpDown _logRetentionNumericUpDown;

    /// <summary>
    /// 自动启动复选框
    /// </summary>
    private readonly CheckBox _autoStartCheckBox;

    /// <summary>
    /// 按钮列表
    /// </summary>
    private readonly List<Button> _buttons;

    /// <summary>
    /// 提示气泡
    /// </summary>
    private readonly ToolTip _toolTip;

    /// <summary>
    /// 构造函数, 初始化设置窗口
    /// </summary>
    private SettingForm()
    {
        // 设置窗口属性
        Text = "设置";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new(UIConstants.SettingFormMinWidth, UIConstants.SettingFormMinHeight);

        // 初始化日志最低记录级别提示标签
        _logLevelLabel = new()
        {
            Text = "日志最低记录级别:",
            AutoSize = true
        };

        // 初始化日志最低记录级别选择框
        _logLevelComboBox = new()
        {
            FlatStyle = FlatStyle.System,
            DropDownStyle = ComboBoxStyle.DropDownList,
            DataSource = Enum.GetValues<LogLevel>()
        };

        // 初始化程序退出后等待日志写入的最长时间提示标签
        _logFlushDelayLabel = new()
        {
            Text = "程序关闭后等待日志写入的最长时间(毫秒):",
            AutoSize = true
        };

        // 初始化程序关闭后等待日志写入的最长时间选择框
        _logFlushDelayNumericUpDown = new()
        {
            Minimum = 0,
            Maximum = 300000,
            AutoSize = true,
            Value = LogConfigManager.Config.MaxLogWaitTime
        };

        // 初始化日志文件最多保留时间提示标签
        _logRetentionLabel = new()
        {
            Text = "日志文件最多保留时间(天):",
            AutoSize = true
        };

        // 初始化日志文件最多保留时间选择框
        _logRetentionNumericUpDown = new()
        {
            Minimum = 1,
            Maximum = 30,
            AutoSize = true,
            Value = LogConfigManager.Config.MaxLogFileRetentionDays
        };

        // 初始化自动启动复选框
        _autoStartCheckBox = new()
        {
            TextAlign = ContentAlignment.MiddleCenter,
            Checked = AutoStartHelper.IsAutoStartEnabled(),
            Appearance = Appearance.Button,
            FlatStyle = FlatStyle.Flat
        };

        // 初始化按钮列表
        _buttons = [];

        // 初始化打开日志文件夹按钮
        var openLogFolderButton = new Button
        {
            Text = "打开日志文件夹",
            BackColor = Color.Gainsboro,
            FlatStyle = FlatStyle.Flat
        };
        openLogFolderButton.Click += OnOpenLogFolderButtonClick;
        _buttons.Add(openLogFolderButton);

        // 初始化创建桌面快捷方式按钮
        var createShortcutButton = new Button
        {
            Text = "创建桌面快捷方式",
            BackColor = Color.RoyalBlue,
            FlatStyle = FlatStyle.Flat
        };
        createShortcutButton.Click += OnCreateShortcutButtonClick;
        _buttons.Add(createShortcutButton);

        // 初始化重置按钮
        var resetButton = new Button
        {
            Text = "重置所有设置",
            BackColor = Color.Yellow,
            FlatStyle = FlatStyle.Flat
        };
        resetButton.Click += OnResetButtonClick;
        _buttons.Add(resetButton);

        // 初始化提示气泡
        _toolTip = UIConstants.ToolTip;

        // 初始化
        Initialize();

        // 设置按钮的悬浮提示
        _toolTip.SetToolTip(openLogFolderButton, "点击打开日志文件夹");
        _toolTip.SetToolTip(createShortcutButton, "点击创建桌面快捷方式");
        _toolTip.SetToolTip(resetButton, "点击重置所有设置");
    }

    /// <summary>
    /// 初始化
    /// </summary>
    private void Initialize()
    {
        // 绑定程序关闭后等待日志写入的最长时间选择框的值改变事件
        _logFlushDelayNumericUpDown.ValueChanged += (sender, e) => LogConfigManager.Config.MaxLogWaitTime = (int)_logFlushDelayNumericUpDown.Value;

        // 绑定日志文件最多保留时间选择框的值改变事件
        _logRetentionNumericUpDown.ValueChanged += (sender, e) => LogConfigManager.Config.MaxLogFileRetentionDays = (int)_logRetentionNumericUpDown.Value;

        // 绑定开机自动启动复选框的值改变事件
        UpdateAutoStartCheckBox();
        _autoStartCheckBox.CheckedChanged += OnAutoStartCheckBoxCheckedChanged;

        // 添加控件到窗口
        Controls.Add(_logLevelLabel);
        Controls.Add(_logLevelComboBox);
        Controls.Add(_logFlushDelayLabel);
        Controls.Add(_logFlushDelayNumericUpDown);
        Controls.Add(_logRetentionLabel);
        Controls.Add(_logRetentionNumericUpDown);
        Controls.Add(_autoStartCheckBox);
        Controls.AddRange([.. _buttons]);

        // 设置悬浮提示
        _toolTip.SetToolTip(_logLevelLabel, "当程序要求记录日志时, 只有日志级别高于或等于该级别的日志才会被记录。如果您不知道选择哪个级别, 建议保持默认的级别");
        _toolTip.SetToolTip(_logLevelComboBox, "从下拉框选择最低日志级别");
        _toolTip.SetToolTip(_logFlushDelayLabel, "设置程序关闭后等待日志写入的最长时间, 如果日志没有在该时间内写入完成, 程序将强制退出");
        _toolTip.SetToolTip(_logFlushDelayNumericUpDown, $"输入程序关闭后等待日志写入的最长时间(毫秒), 范围: {_logFlushDelayNumericUpDown.Minimum} - {_logFlushDelayNumericUpDown.Maximum}");
        _toolTip.SetToolTip(_logRetentionLabel, "设置日志文件最多保留时间, 程序启动时超过该时间的日志文件将被自动删除");
        _toolTip.SetToolTip(_logRetentionNumericUpDown, $"输入日志文件最多保留时间(天), 范围: {_logRetentionNumericUpDown.Minimum} - {_logRetentionNumericUpDown.Maximum}");
        _toolTip.SetToolTip(_autoStartCheckBox, "点击启用或禁用开机自动启动, 注意: 该选项会直接修改注册表");

        // 订阅窗口大小变化事件
        Resize += (sender, e) => ResizeControls();
    }

    /// <summary>
    /// 根据是否启用开机自动启动来切换复选框文本和颜色
    /// </summary>
    private void UpdateAutoStartCheckBox()
    {
        if (_autoStartCheckBox.Checked)
        {
            _autoStartCheckBox.Text = "开机自动启动: 已启用";
            _autoStartCheckBox.BackColor = Color.LightGreen;
        }
        else
        {
            _autoStartCheckBox.Text = "开机自动启动: 已禁用";
            _autoStartCheckBox.BackColor = Color.AliceBlue;
        }
    }

    /// <summary>
    /// 根据窗口大小动态调整控件位置和大小
    /// </summary>
    private void ResizeControls()
    {
        // 获取当前窗口的宽度和高度
        var width = ClientSize.Width;
        var height = ClientSize.Height;

        // 日志最低记录级别选择框高度
        var comboBoxHeight = _logLevelComboBox.Height;

        // 日志等待时间选择框高度
        var numericUpDownHeight = _logFlushDelayNumericUpDown.Height;

        // 日志文件最多保留时间选择框高度
        var logRetentionHeight = _logRetentionNumericUpDown.Height;

        // 按钮高度
        var buttonHeight = (int)(30 * UIConstants.DpiScale);

        // 控件垂直间隔
        var controlSpacing = (int)(10 * UIConstants.DpiScale);

        // 控件总高度
        var totalHeight = comboBoxHeight + numericUpDownHeight + logRetentionHeight + buttonHeight * (_buttons.Count + 1) + controlSpacing * (_buttons.Count + 3);

        // 控件总宽度
        var totalWidth = _logFlushDelayLabel.Width + _logFlushDelayNumericUpDown.Width;

        // 当前控件起始X、Y坐标
        var currentX = (width - totalWidth) / 2;
        var currentY = (height - totalHeight) / 3;

        // box 控件的右侧X坐标
        var boxRightX = currentX + totalWidth;

        // 更新日志最低记录级别提示标签和选择框位置
        _logLevelLabel.Location = new(currentX, currentY);
        _logLevelComboBox.Location = new(boxRightX - _logLevelComboBox.Width, currentY);
        currentY += comboBoxHeight + controlSpacing;

        // 更新日志刷新延迟提示标签和选择框位置
        _logFlushDelayLabel.Location = new(currentX, currentY);
        _logFlushDelayNumericUpDown.Location = new(boxRightX - _logFlushDelayNumericUpDown.Width, currentY);
        currentY += numericUpDownHeight + controlSpacing;

        // 更新日志文件最多保留时间提示标签和选择框位置
        _logRetentionLabel.Location = new(currentX, currentY);
        _logRetentionNumericUpDown.Location = new(boxRightX - _logRetentionNumericUpDown.Width, currentY);
        currentY += logRetentionHeight + controlSpacing;

        // 更新按钮的大小和位置
        foreach (var button in _buttons)
        {
            button.Size = new(totalWidth, buttonHeight);
            button.Location = new(currentX, currentY);
            currentY += buttonHeight + controlSpacing;
        }

        // 更新开机自动启动复选框的大小和位置
        _autoStartCheckBox.Size = new(totalWidth, buttonHeight);
        _autoStartCheckBox.Location = new(currentX, currentY);
    }

    /// <summary>
    /// 重写OnLoad方法, 用于加载设置
    /// </summary>
    /// <param name="e">事件参数</param>
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        // 加载配置数据
        var config = FormConfigManager.Config;

        // 修改窗体大小
        Width = config.SettingFormWidth >= MinimumSize.Width ? config.SettingFormWidth : MinimumSize.Width;
        Height = config.SettingFormHeight >= MinimumSize.Height ? config.SettingFormHeight : MinimumSize.Height;

        // 获取当前屏幕的工作区
        var workingArea = Screen.GetWorkingArea(this);

        // 如果位置不在工作区内, 则居中显示
        if (config.SettingFormLeft < workingArea.Left || config.SettingFormTop < workingArea.Top || config.SettingFormLeft + Width > workingArea.Right || config.SettingFormTop + Height > workingArea.Bottom)
        {
            Left = workingArea.Left + (workingArea.Width - Width) / 2;
            Top = workingArea.Top + (workingArea.Height - Height) / 2;
        }
        else
        {
            // 修改窗体位置
            Left = config.SettingFormLeft;
            Top = config.SettingFormTop;
        }

        // 记录加载
        FileLogger.LogInfo($"成功加载设置窗体位置: {Left}, {Top} 和大小: {Width}, {Height}");
    }

    /// <summary>
    /// 重写OnShown方法, 用于在窗口显示后调整控件位置和大小
    /// </summary>
    /// <param name="e">事件参数</param>
    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);

        // 调整控件位置和大小
        ResizeControls();

        // 设置日志最低记录级别选择框的当前选中项
        _logLevelComboBox.SelectedItem = LogConfigManager.Config.MinLogLevel;

        // 绑定日志最低记录级别选择框的选中项改变事件
        _logLevelComboBox.SelectedIndexChanged += (sender, e) =>
        {
            if (_logLevelComboBox.SelectedItem is LogLevel selectedLevel)
            {
                LogConfigManager.Config.MinLogLevel = selectedLevel;
            }
        };
    }

    /// <summary>
    /// 重写OnFormClosing方法, 用于保存设置
    /// </summary>
    /// <param name="e">窗口关闭事件参数</param>
    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);

        // 保存窗口位置和大小
        FormConfigManager.Config = FormConfigManager.Config with
        {
            SettingFormLeft = Left,
            SettingFormTop = Top,
            SettingFormWidth = Width,
            SettingFormHeight = Height
        };

        // 记录保存
        FileLogger.LogInfo($"成功保存设置窗体位置: {Left}, {Top} 和大小: {Width}, {Height}");
    }

    /// <summary>
    /// 重写FormClosed方法, 用于释放实例
    /// </summary>
    /// <param name="e">窗口关闭事件参数</param>
    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        base.OnFormClosed(e);

        // 释放唯一实例
        _instance = null;
    }
}