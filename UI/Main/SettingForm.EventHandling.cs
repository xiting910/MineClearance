using MineClearance.Services;
using MineClearance.Utilities;
using System.Diagnostics;

namespace MineClearance.UI.Main;

// 设置窗体类的事件处理部分
internal partial class SettingForm
{
    /// <summary>
    /// 打开日志文件夹按钮点击事件处理
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="e">事件参数</param>
    private void OnOpenLogFolderButtonClick(object? sender, EventArgs e)
    {
        try
        {
            // 打开日志文件夹
            var logFolder = Constants.LogFolderPath;
            if (!Directory.Exists(logFolder))
            {
                _ = Directory.CreateDirectory(logFolder);
            }
            _ = Process.Start("explorer.exe", logFolder);
        }
        catch (Exception ex)
        {
            FileLogger.LogException(ex);
            _ = MessageBox.Show($"打开日志文件夹失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// 自动启动复选框状态变化事件处理
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="e">事件参数</param>
    private void OnAutoStartCheckBoxCheckedChanged(object? sender, EventArgs e)
    {
        if (sender is CheckBox cb)
        {
            try
            {
                if (cb.Checked)
                {
                    // 启用自动启动
                    AutoStartHelper.EnableAutoStart();
                }
                else
                {
                    // 禁用自动启动
                    AutoStartHelper.DisableAutoStart();
                }
            }
            catch (Exception ex)
            {
                // 如果发生异常, 切换复选框状态
                cb.CheckedChanged -= OnAutoStartCheckBoxCheckedChanged;
                cb.Checked = !cb.Checked;
                cb.CheckedChanged += OnAutoStartCheckBoxCheckedChanged;

                // 记录并显示错误信息
                FileLogger.LogException(ex);
                _ = MessageBox.Show($"设置开机自启动失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // 更新复选框文本和颜色
                UpdateAutoStartCheckBox();
            }
        }
    }

    /// <summary>
    /// 创建桌面快捷方式按钮点击事件处理
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="e">事件参数</param>
    private void OnCreateShortcutButtonClick(object? sender, EventArgs e)
    {
        // 弹窗提示用户输入快捷方式名称
        var shortcutName = Microsoft.VisualBasic.Interaction.InputBox("请输入快捷方式名称:", "创建桌面快捷方式", Path.GetFileNameWithoutExtension(Application.ExecutablePath));

        if (!string.IsNullOrWhiteSpace(shortcutName))
        {
            // 创建快捷方式
            try
            {
                ShortcutCreator.CreateDesktopShortcut(Application.ExecutablePath, shortcutName);
                _ = MessageBox.Show("快捷方式创建成功！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                FileLogger.LogException(ex);
                _ = MessageBox.Show($"创建快捷方式失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    /// <summary>
    /// 重置设置按钮点击事件处理
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="e">事件参数</param>
    private void OnResetButtonClick(object? sender, EventArgs e)
    {
        // 重置窗口配置为无效值
        FormConfigManager.Config = null;

        // 重置日志配置
        LogConfigManager.ResetToDefault();

        // 更新日志级别选择框
        _logLevelComboBox.SelectedItem = LogConfigManager.Config.MinLogLevel;

        // 更新日志等待时间选择框
        _logFlushDelayNumericUpDown.Value = LogConfigManager.Config.MaxLogWaitTime;

        // 更新日志保留时间选择框
        _logRetentionNumericUpDown.Value = LogConfigManager.Config.MaxLogFileRetentionDays;

        // 关闭自动启动
        _autoStartCheckBox.Checked = false;

        // 显示重置成功提示
        _ = MessageBox.Show("设置已重置为默认值！", "重置设置成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}