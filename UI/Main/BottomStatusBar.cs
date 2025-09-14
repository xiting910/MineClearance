using System.Diagnostics;
using MineClearance.Services;
using MineClearance.Models.Enums;

namespace MineClearance.UI.Main;

/// <summary>
/// 底部状态栏
/// </summary>
internal sealed class BottomStatusBar : StatusStrip
{
    /// <summary>
    /// 底部状态栏的单例
    /// </summary>
    public static BottomStatusBar Instance { get; } = new();

    /// <summary>
    /// 作者名字
    /// </summary>
    private const string AuthorName = "xiting910";

    /// <summary>
    /// 作者主页链接
    /// </summary>
    private const string AuthorHomepageUrl = "https://github.com/xiting910";

    /// <summary>
    /// GitHub 仓库链接
    /// </summary>
    private const string GitHubRepoUrl = "https://github.com/xiting910/MineClearance";

    /// <summary>
    /// 左侧状态标签
    /// </summary>
    private readonly ToolStripStatusLabel _statusLabel;

    /// <summary>
    /// 右侧信息标签1
    /// </summary>
    private readonly ToolStripStatusLabel _infoLabel1;

    /// <summary>
    /// 右侧作者主页链接标签
    /// </summary>
    private readonly ToolStripStatusLabel _authorLinkLabel;

    /// <summary>
    /// 右侧信息标签2
    /// </summary>
    private readonly ToolStripStatusLabel _infoLabel2;

    /// <summary>
    /// 右侧项目仓库链接标签
    /// </summary>
    private readonly ToolStripStatusLabel _repoLinkLabel;

    /// <summary>
    /// 右侧信息标签3
    /// </summary>
    private readonly ToolStripStatusLabel _infoLabel3;

    /// <summary>
    /// 私有构造函数, 初始化状态栏
    /// </summary>
    private BottomStatusBar()
    {
        // 设置状态栏属性
        Dock = DockStyle.Bottom;
        ShowItemToolTips = true;

        // 左侧状态标签
        _statusLabel = new()
        {
            Text = "状态: 就绪",
            Spring = true,
            TextAlign = ContentAlignment.MiddleLeft
        };

        // 右侧信息标签1
        _infoLabel1 = new()
        {
            Text = $"本项目由",
            IsLink = false,
            ForeColor = Color.Black,
            TextAlign = ContentAlignment.MiddleRight
        };

        // 右侧作者主页链接标签
        _authorLinkLabel = new()
        {
            Text = AuthorName,
            IsLink = true,
            ForeColor = Color.Blue,
            ToolTipText = AuthorHomepageUrl,
            TextAlign = ContentAlignment.MiddleRight
        };

        // 右侧作者主页链接点击事件处理
        _authorLinkLabel.Click += (s, e) =>
        {
            try
            {
                _ = Process.Start(new ProcessStartInfo
                {
                    FileName = AuthorHomepageUrl,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                FileLogger.LogException(ex);
                _ = MessageBox.Show($"无法打开链接: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        };

        // 右侧信息标签2
        _infoLabel2 = new()
        {
            Text = "一人开发。如您有建议或发现问题，请访问",
            IsLink = false,
            ForeColor = Color.Black,
            TextAlign = ContentAlignment.MiddleRight
        };

        // 右侧 GitHub 仓库链接
        _repoLinkLabel = new()
        {
            Text = "项目github仓库",
            IsLink = true,
            ForeColor = Color.Blue,
            ToolTipText = GitHubRepoUrl,
            TextAlign = ContentAlignment.MiddleRight
        };

        // 右侧 GitHub 项目仓库链接点击事件处理
        _repoLinkLabel.Click += (s, e) =>
        {
            try
            {
                _ = Process.Start(new ProcessStartInfo
                {
                    FileName = GitHubRepoUrl,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                FileLogger.LogException(ex);
                _ = MessageBox.Show($"无法打开链接: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        };

        // 右侧信息标签3
        _infoLabel3 = new()
        {
            Text = "提交 Issue 或 Pull Request",
            IsLink = false,
            ForeColor = Color.Black,
            TextAlign = ContentAlignment.MiddleRight
        };

        // 添加标签到状态栏
        _ = Items.Add(_statusLabel);
        _ = Items.Add(_infoLabel1);
        _ = Items.Add(_authorLinkLabel);
        _ = Items.Add(_infoLabel2);
        _ = Items.Add(_repoLinkLabel);
        _ = Items.Add(_infoLabel3);
    }

    /// <summary>
    /// 设置左侧状态文本
    /// </summary>
    /// <param name="status">状态</param>
    public void SetStatus(StatusBarState status)
    {
        _statusLabel.Text = status switch
        {
            StatusBarState.Ready => "状态: 就绪",
            StatusBarState.History => "状态: 历史记录",
            StatusBarState.Preparing => "状态: 准备游戏",
            StatusBarState.WaitingStart => "状态: 等待游戏开始",
            StatusBarState.InGame => "状态: 游戏中",
            StatusBarState.Paused => "状态: 游戏暂停",
            StatusBarState.GameWon => "状态: 游戏胜利",
            StatusBarState.GameLost => "状态: 游戏失败",
            _ => "状态: 未知"
        };
    }
}