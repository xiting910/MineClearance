using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MineClearance.Core;
using MineClearance.Core.Enums;
using MineClearance.Core.Interfaces;
using MineClearance.Core.Models;
using MineClearance.Core.Models.Records;
using MineClearance.UI.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace MineClearance.UI.ViewModels;

/// <summary>
/// 游戏视图模型, 负责游戏状态绑定, 棋盘构建, 计时刷新与游戏操作
/// </summary>
public sealed partial class GameViewModel : ObservableObject
{
    /// <summary>
    /// 共享占位格子, 棋盘未生成或超出当前棋盘范围的格子引用它
    /// </summary>
    private static readonly Cell PlaceholderCell = new();

    /// <summary>
    /// 游戏管理器
    /// </summary>
    private readonly IGameManager _gameManager;

    /// <summary>
    /// 全局短暂提示视图模型
    /// </summary>
    private readonly ToastViewModel _toast;

    /// <summary>
    /// 界面状态刷新计时器, 定时触发游戏计时刷新
    /// </summary>
    private readonly DispatcherTimer _refreshTimer;

    /// <summary>
    /// UI 配置, 提供显示索引热键与首点复制索引开关
    /// </summary>
    private readonly UIOptions _uiOptions;

    /// <summary>
    /// 是否允许切换暂停状态, 游戏结束后不允许暂停或继续
    /// </summary>
    private bool CanTogglePause => !IsGameEnded;

    /// <summary>
    /// 固定大小的格子视图模型池, 按最大棋盘行列排列只创建一次, 通过可见性复用
    /// </summary>
    [ObservableProperty]
    public partial IReadOnlyList<CellViewModel> Cells { get; set; } = [];

    /// <summary>
    /// 是否存在游戏
    /// </summary>
    [ObservableProperty]
    public partial bool HasGame { get; set; }

    /// <summary>
    /// 棋盘行数
    /// </summary>
    [ObservableProperty]
    public partial int Rows { get; set; }

    /// <summary>
    /// 棋盘列数
    /// </summary>
    [ObservableProperty]
    public partial int Columns { get; set; }

    /// <summary>
    /// 游戏状态文本 (等待开始/进行中/已暂停/胜利/失败)
    /// </summary>
    [ObservableProperty]
    public partial string StatusText { get; set; } = string.Empty;

    /// <summary>
    /// 难度文本, 包含棋盘尺寸与地雷数
    /// </summary>
    [ObservableProperty]
    public partial string DifficultyText { get; set; } = string.Empty;

    /// <summary>
    /// 剩余未标记地雷数
    /// </summary>
    [ObservableProperty]
    public partial int RemainingMines { get; set; }

    /// <summary>
    /// 已打开格子数
    /// </summary>
    [ObservableProperty]
    public partial int OpenedCount { get; set; }

    /// <summary>
    /// 完成度文本
    /// </summary>
    [ObservableProperty]
    public partial string CompletionText { get; set; } = "0" + Core.Constants.PercentSign;

    /// <summary>
    /// 游戏时间文本
    /// </summary>
    [ObservableProperty]
    public partial string TimeText { get; set; } = "00:00";

    /// <summary>
    /// 随机种子
    /// </summary>
    [ObservableProperty]
    public partial int Seed { get; set; }

    /// <summary>
    /// 是否等待首次点击
    /// </summary>
    [ObservableProperty]
    public partial bool IsWaitingStarted { get; set; }

    /// <summary>
    /// 是否已暂停
    /// </summary>
    [ObservableProperty]
    public partial bool IsPaused { get; set; }

    /// <summary>
    /// 是否已结束, 变化时同步刷新暂停命令可用性
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PauseResumeCommand))]
    public partial bool IsGameEnded { get; set; }

    /// <summary>
    /// 是否胜利
    /// </summary>
    [ObservableProperty]
    public partial bool IsWin { get; set; }

    /// <summary>
    /// 暂停/继续按钮文本
    /// </summary>
    [ObservableProperty]
    public partial string PauseButtonText { get; set; } = "暂停";

    /// <summary>
    /// 是否正在显示所有格子索引, 等待开始时按住热键为 true
    /// </summary>
    [ObservableProperty]
    public partial bool IsShowingIndexes { get; set; }

    /// <summary>
    /// 棋盘像素宽度, 由当前列数与格子大小计算, 棋盘容器按当前棋盘自适应尺寸
    /// </summary>
    public double BoardPixelWidth => Columns * Constants.CellSize;

    /// <summary>
    /// 棋盘像素高度, 由当前行数与格子大小计算, 棋盘容器按当前棋盘自适应尺寸
    /// </summary>
    public double BoardPixelHeight => Rows * Constants.CellSize;

    /// <summary>
    /// 显示索引热键, Key.None 表示未设置, 由窗口在等待开始时按此键切换索引显示
    /// </summary>
    public Key ShowIndexHotKey => _uiOptions.ShowIndexHotKey;

    /// <summary>
    /// 请求返回主视图的事件, 由壳视图模型处理
    /// </summary>
    public event Action? MainViewRequested;

    /// <summary>
    /// 首次点击格子时请求复制索引的事件, 携带索引文本, 由视图写入剪贴板
    /// </summary>
    public event Action<string>? FirstClickIndexRequested;

    /// <summary>
    /// 创建游戏视图模型
    /// </summary>
    /// <param name="gameManager">游戏管理器</param>
    /// <param name="toastViewModel">全局短暂提示视图模型</param>
    /// <param name="uiOptions">UI 配置</param>
    public GameViewModel(IGameManager gameManager, ToastViewModel toastViewModel, UIOptions uiOptions)
    {
        _gameManager = gameManager;
        _toast = toastViewModel;
        _uiOptions = uiOptions;

        // 构建固定大小的格子视图模型池, 只创建一次, 此后所有游戏复用
        BuildCellPool();

        // 棋盘区域初始化为最大尺寸, 使游戏视图在启动布局时即实例化全部格子控件 (预热)
        Rows = Core.Constants.MaxBoardHeight;
        Columns = Core.Constants.MaxBoardWidth;

        // 预热: 占位格子全部设为可见, 使启动布局同时完成全部格子的实例化与布局
        foreach (var cellViewModel in Cells)
        {
            cellViewModel.IsVisible = true;
        }

        // 订阅游戏管理器事件: 属性变化前退订旧游戏, 属性变化后订阅新游戏
        gameManager.PropertyChanging += OnGameManagerPropertyChanging;
        gameManager.PropertyChanged += OnGameManagerPropertyChanged;

        // 启动界面状态刷新计时器
        _refreshTimer = new(
            TimeSpan.FromMilliseconds(Constants.UiRefreshIntervalMilliseconds),
            DispatcherPriority.Background,
            OnRefreshTimerTick
        );
        _refreshTimer.Start();

        // 绑定当前已存在的游戏
        if (gameManager.Game is not null)
        {
            BindGame(gameManager.Game);
        }
    }

    /// <summary>
    /// 行数变化时通知棋盘像素高度
    /// </summary>
    /// <param name="value">新的行数</param>
    partial void OnRowsChanged(int value)
    {
        OnPropertyChanged(nameof(BoardPixelHeight));
    }

    /// <summary>
    /// 列数变化时通知棋盘像素宽度
    /// </summary>
    /// <param name="value">新的列数</param>
    partial void OnColumnsChanged(int value)
    {
        OnPropertyChanged(nameof(BoardPixelWidth));
    }

    /// <summary>
    /// 索引显示状态变化时同步所有格子的索引可见性
    /// </summary>
    /// <param name="value">新的显示状态</param>
    partial void OnIsShowingIndexesChanged(bool value)
    {
        foreach (var cell in Cells)
        {
            cell.ShowIndex = value;
        }
    }

    /// <summary>
    /// 显示操作提示的 Toast 通知, 包含首次点击机制与鼠标操作说明
    /// </summary>
    [RelayCommand]
    private void ShowHelp()
    {
        _toast.Show(
            "🎮 操作提示\n" +
            "首次点击后生成地雷, 确保首次点击的格子及周围格子尽量不会是地雷\n" +
            "左键: 未打开格子打开, 数字格子在周围插旗数匹配时展开周围\n" +
            "右键: 数字格在周围未打开格子数匹配时一键插旗周围, 其他格循环标记 旗/问号/取消\n" +
            "在首次点击前不允许右键操作\n" +
            "按住左键/右键滑动: 持续操作"
        );
    }

    /// <summary>
    /// 重新开始当前游戏
    /// </summary>
    [RelayCommand]
    private void Restart()
    {
        _gameManager.RestartCurrentGame();
    }

    /// <summary>
    /// 暂停或继续当前游戏, 游戏结束时不可用
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanTogglePause))]
    private void PauseResume()
    {
        if (_gameManager.Game is not { } game) { return; }

        if (game.Status is GameStatus.Paused)
        {
            game.CancelPause();
        }
        else
        {
            game.Pause();
        }
    }

    /// <summary>
    /// 强制返回主视图, 不保存进度也不提示
    /// </summary>
    [RelayCommand]
    private void ExitWithoutSave()
    {
        _gameManager.ExitWithoutSaving();
        MainViewRequested?.Invoke();
    }

    /// <summary>
    /// 返回主视图: 进行中 (含暂停) 的游戏保存进度并提示, 等待开始或已结束的游戏直接退出
    /// </summary>
    [RelayCommand]
    private async Task SaveAndBackAsync()
    {
        if (_gameManager.Game is { HasProgress: true })
        {
            // 有实际进度的游戏: 保存并提示
            var saved = await _gameManager.SaveAndExitAsync(App.ExitCts.Token);
            _toast.Show(saved ? "游戏进度已保存, 下次可继续游戏" : "保存失败");
        }
        else
        {
            // 等待开始或已结束的游戏: 无进度可存, 直接退出
            _gameManager.ExitWithoutSaving();
        }
        MainViewRequested?.Invoke();
    }

    /// <summary>
    /// 游戏可暂停时暂停游戏
    /// </summary>
    public void PauseIfPerformable()
    {
        if (_gameManager.Game is { IsPerformable: true } game)
        {
            game.Pause();
        }
    }

    /// <summary>
    /// 取消暂停恢复游戏
    /// </summary>
    public void ResumeIfPaused()
    {
        if (_gameManager.Game is { Status: GameStatus.Paused } game)
        {
            game.CancelPause();
        }
    }

    /// <summary>
    /// 左键单击处理: 按格子类型分发, 未打开格子打开 (踩雷时记录位置), 数字格子展开周围
    /// </summary>
    /// <param name="position">格子位置</param>
    public void LeftClickAt(Position position)
    {
        if (_gameManager.Game is not { IsPerformable: true } game) { return; }

        var isFirstClick = game.Status is GameStatus.WaitingStarted;
        switch (game.Board[position].Type)
        {
            case CellType.Unopened:
                game.OpenCell(position);
                if (isFirstClick && _uiOptions.CopyIndexOnFirstClick)
                {
                    FirstClickIndexRequested?.Invoke(position.ToIndex(Columns).ToString());
                }
                break;

            case CellType.Number or CellType.WarningNumber:
                game.OpenAdjacentCells(position);
                break;
        }
    }

    /// <summary>
    /// 在指定位置三态循环标记: 未打开 → 旗 → 问号 → 取消标记, 仅在游戏进行中时有效
    /// </summary>
    /// <param name="position">格子位置</param>
    public void CycleMarkAt(Position position)
    {
        if (_gameManager.Game is not { Status: GameStatus.InProgress } game)
        {
            return;
        }

        switch (game.Board[position].Type)
        {
            case CellType.Unopened: game.FlagCell(position); break;
            case CellType.Flagged: game.QuestionCell(position); break;
            case CellType.Question: game.UnmarkCell(position); break;
        }
    }

    /// <summary>
    /// 标记数字格周围所有未打开格子为旗, 仅对数字格有效且在游戏进行中时
    /// </summary>
    /// <param name="position">数字格位置</param>
    public void FlagAdjacentAt(Position position)
    {
        if (_gameManager.Game is { Status: GameStatus.InProgress } game &&
            game.Board[position].Type is CellType.Number)
        {
            game.FlagAdjacentCells(position);
        }
    }

    /// <summary>
    /// 显示 Toast 提示, 由视图在剪贴板写入完成后按结果调用
    /// </summary>
    /// <param name="message">提示文本</param>
    public void Show(string message)
    {
        _toast.Show(message);
    }

    /// <summary>
    /// 构建固定大小的格子视图模型池, 按最大棋盘尺寸创建一次, 此后所有游戏复用
    /// </summary>
    private void BuildCellPool()
    {
        Cells = [.. Position.GetAllPositions(Core.Constants.MaxBoardHeight, Core.Constants.MaxBoardWidth)
            .Select(position => new CellViewModel(PlaceholderCell, position))];
    }

    /// <summary>
    /// 绑定游戏实例并初始化界面状态
    /// </summary>
    /// <param name="game">游戏实例</param>
    private void BindGame(IGame game)
    {
        game.PropertyChanged += OnGamePropertyChanged;
        game.Board.PropertyChanged += OnBoardPropertyChanged;
        HasGame = true;

        (Rows, Columns, var mineCount) = game.Config;
        Seed = game.Seed;
        DifficultyText = $"{game.Difficulty.GetDescription()} ({Rows}x{Columns}, {mineCount} 雷)";

        foreach (var cellViewModel in Cells)
        {
            cellViewModel.Game = game;
        }

        UpdateBoard();
        UpdateStatus();
    }

    /// <summary>
    /// 解绑游戏实例并清理订阅, 应在 GameManager 属性变化前调用, 格子池常驻复用无需清理
    /// </summary>
    /// <param name="game">游戏实例</param>
    private void UnbindGame(IGame? game)
    {
        foreach (var cellViewModel in Cells)
        {
            cellViewModel.Game = null;
        }

        game?.Board.PropertyChanged -= OnBoardPropertyChanged;
        game?.PropertyChanged -= OnGamePropertyChanged;
    }

    /// <summary>
    /// 更新全部格子: 固定格子池不重建, 仅替换内部格子引用并切换可见性, 超出当前棋盘的部分隐藏
    /// </summary>
    private void UpdateBoard()
    {
        if (_gameManager.Game is not { } game) { return; }
        foreach (var pos in Position.GetAllPositions(
            Core.Constants.MaxBoardHeight, Core.Constants.MaxBoardWidth))
        {
            var cellViewModel = Cells[pos.ToIndex(Core.Constants.MaxBoardWidth)];
            var isInBoard = pos.IsInBounds(Rows, Columns);
            cellViewModel.IsVisible = isInBoard;
            cellViewModel.IndexText = isInBoard ? pos.ToIndex(Columns).ToString() : string.Empty;
            cellViewModel.UpdateCell(isInBoard ? game.Board[pos] : PlaceholderCell);
        }
        UpdateStatus();
    }

    /// <summary>
    /// 更新状态相关的界面显示
    /// </summary>
    private void UpdateStatus()
    {
        if (_gameManager.Game is not { } game)
        {
            StatusText = string.Empty;
            IsWaitingStarted = false;
            IsPaused = false;
            IsGameEnded = false;
            IsWin = false;
            RemainingMines = 0;
            OpenedCount = 0;
            IsShowingIndexes = false;
            return;
        }

        StatusText = game.Status.GetDescription();
        IsWaitingStarted = game.Status is GameStatus.WaitingStarted;
        IsPaused = game.Status is GameStatus.Paused;
        IsGameEnded = game.Status is GameStatus.Won or GameStatus.Lost;
        IsWin = game.Status is GameStatus.Won;
        PauseButtonText = IsPaused ? "继续" : "暂停";
        CompletionText = (game.Completion * Core.Constants.PercentBase).ToString(Core.Constants.FloatFormat)
            + Core.Constants.PercentSign;
        RemainingMines = game.Config.MineCount - game.Board.FlagCount;
        OpenedCount = game.Board.OpenedCount;

        // 游戏开始 (等待状态结束) 后强制隐藏索引, 即使热键仍被按住
        if (!IsWaitingStarted)
        {
            IsShowingIndexes = false;
        }

        // 游戏结束时通过 Toast 提示结果
        if (IsGameEnded)
        {
            var result = game.Result;
            if (result is not null)
            {
                var completion = result.Completion is double value
                    ? ", 完成度: "
                    + (value * Core.Constants.PercentBase).ToString(Core.Constants.FloatFormat)
                    + Core.Constants.PercentSign
                    : string.Empty;
                _toast.Show(IsWin
                    ? $"🎉 游戏胜利! 用时: {FormatTime(result.Duration)}"
                    : $"💣 游戏失败, 用时: {FormatTime(result.Duration)}{completion}"
                );
            }
        }
    }

    /// <summary>
    /// 游戏属性变化前: 退订旧游戏事件, 此时 GameManager.Game 仍为旧实例
    /// </summary>
    /// <param name="sender">游戏管理器</param>
    /// <param name="e">属性变化前事件参数</param>
    private void OnGameManagerPropertyChanging(object? sender, PropertyChangingEventArgs e)
    {
        if (e.PropertyName == nameof(IGameManager.Game))
        {
            UnbindGame(_gameManager.Game);
        }
    }

    /// <summary>
    /// 游戏属性变化后: 订阅新游戏事件或清空界面状态
    /// </summary>
    /// <param name="sender">游戏管理器</param>
    /// <param name="e">属性变化事件参数</param>
    private void OnGameManagerPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(IGameManager.Game)) { return; }

        if (_gameManager.Game is { } game)
        {
            BindGame(game);
        }
        else
        {
            HasGame = false;
            UpdateStatus();
        }
    }

    /// <summary>
    /// 游戏属性变化时更新界面: Status 变化时更新状态, Board 在游戏创建时已生成且之后不变, 无需监听
    /// </summary>
    /// <param name="sender">游戏实例</param>
    /// <param name="e">属性变化事件参数</param>
    private void OnGamePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(IGame.Status): UpdateStatus(); break;
            case nameof(IGame.Result): UpdateStatus(); break;
            case nameof(IGame.Completion):
                if (_gameManager.Game is { } game)
                {
                    CompletionText = (game.Completion * Core.Constants.PercentBase)
                        .ToString(Core.Constants.FloatFormat)
                        + Core.Constants.PercentSign;
                }
                break;
        }
    }

    /// <summary>
    /// 棋盘计数变化时更新统计信息
    /// </summary>
    /// <param name="sender">棋盘</param>
    /// <param name="e">属性变化事件参数</param>
    private void OnBoardPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_gameManager.Game is not { } game) { return; }

        RemainingMines = game.Config.MineCount - game.Board.FlagCount;
        OpenedCount = game.Board.OpenedCount;
    }

    /// <summary>
    /// 定时刷新游戏计时显示
    /// </summary>
    /// <param name="sender">计时器</param>
    /// <param name="e">计时器事件参数</param>
    private void OnRefreshTimerTick(object? sender, EventArgs e)
    {
        if (_gameManager.Game is not { } game) { return; }
        TimeText = FormatTime(game.Timer.Elapsed);
    }

    /// <summary>
    /// 格式化时间为 MM:SS
    /// </summary>
    /// <param name="time">时间</param>
    /// <returns>格式化后的时间文本</returns>
    private static string FormatTime(TimeSpan time)
    {
        return $"{(int)time.TotalMinutes:00}:{time.Seconds:00}";
    }
}
