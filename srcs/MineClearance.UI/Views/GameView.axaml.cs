using Avalonia.Controls;
using Avalonia.Input;
using MineClearance.Core.Models.Records;
using MineClearance.UI.ViewModels;

namespace MineClearance.UI.Views;

/// <summary>
/// 游戏视图, 提供棋盘渲染与指针交互
/// </summary>
public sealed partial class GameView : UserControl
{
    /// <summary>
    /// 是否按住左键
    /// </summary>
    private bool _isLeftPressed;

    /// <summary>
    /// 是否按住右键
    /// </summary>
    private bool _isRightPressed;

    /// <summary>
    /// 最后一次操作的格子位置, 用于滑动连续操作
    /// </summary>
    private Position _lastPosition;

    /// <summary>
    /// 创建游戏视图
    /// </summary>
    public GameView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// 棋盘指针按下: 左键开格/双击 chord, 右键三态循环/一键插旗
    /// </summary>
    /// <param name="sender">棋盘控件</param>
    /// <param name="e">指针按下事件参数</param>
    private void OnBoardPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is not GameViewModel viewModel) { return; }
        if (GetPositionAt(e) is not Position position) { return; }

        var properties = e.GetCurrentPoint(this).Properties;

        if (properties.IsLeftButtonPressed)
        {
            // 左键: 统一由视图模型按格子类型分发处理 (未打开开格, 数字格展开周围)
            viewModel.LeftClickAt(position);
            _isLeftPressed = true;
        }
        else if (properties.IsRightButtonPressed)
        {
            // 右键: 数字格一键插旗周围, 其他格三态循环标记
            viewModel.FlagAdjacentAt(position);
            viewModel.CycleMarkAt(position);
            _isRightPressed = true;
        }
        else
        {
            return;
        }

        _lastPosition = position;
        e.Handled = true;
    }

    /// <summary>
    /// 棋盘指针移动: 按住鼠标滑动时对经过的格子持续操作
    /// </summary>
    /// <param name="sender">棋盘控件</param>
    /// <param name="e">指针事件参数</param>
    private void OnBoardPointerMoved(object? sender, PointerEventArgs e)
    {
        if (DataContext is not GameViewModel viewModel) { return; }
        if (!_isLeftPressed && !_isRightPressed) { return; }
        if (GetPositionAt(e) is not Position position) { return; }

        // 未移动到新格子时不重复操作
        if (position == _lastPosition) { return; }
        _lastPosition = position;

        if (_isLeftPressed)
        {
            // 按住左键滑动: 与单击相同的分发处理, 未打开格子持续打开, 数字格展开周围
            viewModel.LeftClickAt(position);
        }
        else if (_isRightPressed)
        {
            viewModel.CycleMarkAt(position);
        }
    }

    /// <summary>
    /// 棋盘指针释放: 结束按住状态
    /// </summary>
    /// <param name="sender">棋盘控件</param>
    /// <param name="e">指针释放事件参数</param>
    private void OnBoardPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _isLeftPressed = false;
        _isRightPressed = false;
    }

    /// <summary>
    /// 将指针位置换算为棋盘格子位置
    /// </summary>
    /// <param name="e">指针事件参数</param>
    /// <returns>格子位置, 超出棋盘范围时返回 <see langword="null"/></returns>
    private Position? GetPositionAt(PointerEventArgs e)
    {
        if (DataContext is not GameViewModel viewModel || viewModel.Rows <= 0 || viewModel.Columns <= 0)
        {
            return null;
        }

        var position = e.GetPosition(BoardItemsControl);
        var bounds = BoardItemsControl.Bounds;
        if (bounds.Width <= 0 || bounds.Height <= 0) { return null; }

        var col = (int)(position.X / (bounds.Width / viewModel.Columns));
        var row = (int)(position.Y / (bounds.Height / viewModel.Rows));
        var pos = new Position(row, col);
        return pos.IsInBounds(viewModel.Rows, viewModel.Columns) ? pos : null;
    }
}
