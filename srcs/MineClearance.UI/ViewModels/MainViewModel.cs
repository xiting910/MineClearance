using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MineClearance.Core.Enums;
using MineClearance.Core.Interfaces;
using MineClearance.Core.Models.Records;
using MineClearance.UI.Models;
using System;
using System.Collections.Generic;

namespace MineClearance.UI.ViewModels;

/// <summary>
/// 主视图模型, 负责难度选择与页面导航
/// </summary>
public sealed partial class MainViewModel : ObservableObject
{
    /// <summary>
    /// 游戏数据存储库, 用于访问存档与历史记录
    /// </summary>
    private readonly IGameDataRepository _dataRepository;

    /// <summary>
    /// 游戏管理器, 用于开始新游戏与恢复存档
    /// </summary>
    private readonly IGameManager _gameManager;

    /// <summary>
    /// 当前选中的难度
    /// </summary>
    [ObservableProperty]
    public partial GameDifficulty? SelectedDifficulty { get; set; }

    /// <summary>
    /// 当前是否选择自定义难度, 自定义时允许编辑棋盘参数
    /// </summary>
    [ObservableProperty]
    public partial bool IsCustomDifficulty { get; set; }

    /// <summary>
    /// 棋盘高度
    /// </summary>
    [ObservableProperty]
    public partial int? Height { get; set; }

    /// <summary>
    /// 棋盘宽度
    /// </summary>
    [ObservableProperty]
    public partial int? Width { get; set; }

    /// <summary>
    /// 地雷数量
    /// </summary>
    [ObservableProperty]
    public partial int? MineCount { get; set; }

    /// <summary>
    /// 地雷数量上限, 随宽高变化联动
    /// </summary>
    [ObservableProperty]
    public partial int? MaxMineCount { get; set; }

    /// <summary>
    /// 随机种子文本, 留空表示随机生成
    /// </summary>
    [ObservableProperty]
    public partial string SeedText { get; set; }

    /// <summary>
    /// 是否存在存档, 存在时显示"继续游戏"按钮
    /// </summary>
    [ObservableProperty]
    public partial bool HasSaveData { get; set; }

    /// <summary>
    /// 可选择的难度列表
    /// </summary>
    public IReadOnlyList<GameDifficulty> Difficulties { get; } = Enum.GetValues<GameDifficulty>();

    /// <summary>
    /// 参数输入框区域的悬浮提示, 非自定义难度时为提示文本, 自定义难度时为 <see langword="null"/> 不显示
    /// </summary>
    public string? ParameterInputTip => IsCustomDifficulty ? null : "注意：非自定义难度不允许输入参数";

    /// <summary>
    /// 请求退出程序的事件, 由视图层关闭主窗口
    /// </summary>
    public event Action? ExitRequested;

    /// <summary>
    /// 请求导航至指定目标的事件, 由壳视图模型处理
    /// </summary>
    public event Action<NavigationTarget>? NavigationRequested;

    /// <summary>
    /// 初始化主视图模型
    /// </summary>
    /// <param name="dataRepository">数据存储库</param>
    /// <param name="gameManager">游戏管理器</param>
    public MainViewModel(IGameDataRepository dataRepository, IGameManager gameManager)
    {
        _dataRepository = dataRepository;
        _gameManager = gameManager;

        var (height, width, mineCount) = GameConfig.FromDifficulty(GameDifficulty.Beginner);
        SelectedDifficulty = GameDifficulty.Beginner;
        Height = height;
        Width = width;
        MineCount = mineCount;
        MaxMineCount = (height * width) - 1;
        SeedText = string.Empty;
        HasSaveData = _dataRepository.SaveData is not null;
    }

    /// <summary>
    /// 是否自定义难度变化时刷新参数输入框的悬浮提示
    /// </summary>
    /// <param name="value">新值</param>
    partial void OnIsCustomDifficultyChanged(bool value)
    {
        OnPropertyChanged(nameof(ParameterInputTip));
    }

    /// <summary>
    /// 难度变化时联动更新棋盘参数: 预设难度显示预设值并禁止编辑, 自定义难度允许编辑
    /// </summary>
    /// <param name="value">新选中的难度</param>
    partial void OnSelectedDifficultyChanged(GameDifficulty? value)
    {
        // 空值不处理, 由绑定控件保证不会出现空值
        if (value is null) { return; }

        // 切换到自定义难度时: 允许编辑, 以当前参数计算地雷上限
        if (value is GameDifficulty.Custom)
        {
            IsCustomDifficulty = true;
            UpdateMaxMineCount();
            return;
        }

        // 切换到预设难度: 显示预设值并禁止编辑, 清空种子
        IsCustomDifficulty = false;
        var (height, width, mineCount) = GameConfig.FromDifficulty(value.Value);
        Height = height;
        Width = width;
        MineCount = mineCount;
        MaxMineCount = (height * width) - 1;
        SeedText = string.Empty;
    }

    /// <summary>
    /// 高度变化时联动更新地雷数量上限
    /// </summary>
    /// <param name="value">新高度</param>
    partial void OnHeightChanged(int? value)
    {
        UpdateMaxMineCount();
    }

    /// <summary>
    /// 宽度变化时联动更新地雷数量上限
    /// </summary>
    /// <param name="value">新宽度</param>
    partial void OnWidthChanged(int? value)
    {
        UpdateMaxMineCount();
    }

    /// <summary>
    /// 开始新游戏: 先清空存档, 预设难度走指定难度分支, 自定义难度以输入框中的参数开始游戏
    /// </summary>
    [RelayCommand]
    private void StartNewGame()
    {
        // 获取当前选中的难度, 空值不处理, 由绑定控件保证不会出现空值
        var difficulty = SelectedDifficulty;
        if (difficulty is null) { return; }

        // 根据难度分支开始新游戏
        if (difficulty is GameDifficulty.Custom)
        {
            // 自定义难度: 以输入框中的棋盘参数构建配置
            var config = new GameConfig(Height ?? 0, Width ?? 0, MineCount ?? 0);

            // 验证配置有效性, 无效则不开始游戏
            if (!config.IsValid()) { return; }

            // 自定义难度允许玩家指定随机种子, 若输入框为空则随机生成
            _gameManager.StartNewGame(config, int.TryParse(SeedText.Trim(), out var parsed) ? parsed : null);
        }
        else
        {
            // 预设难度: 直接以指定难度开始游戏
            _gameManager.StartNewGame(difficulty.Value);
        }

        // 切换至游戏视图
        NavigationRequested?.Invoke(NavigationTarget.GameView);
    }

    /// <summary>
    /// 从存档恢复游戏并切换至游戏视图
    /// </summary>
    [RelayCommand]
    private void ContinueGame()
    {
        if (_dataRepository.SaveData is null) { return; }

        _gameManager.RestoreFromSaveData();
        NavigationRequested?.Invoke(NavigationTarget.GameView);
    }

    /// <summary>
    /// 请求切换至历史记录视图
    /// </summary>
    [RelayCommand]
    private void ShowHistory()
    {
        NavigationRequested?.Invoke(NavigationTarget.HistoryView);
    }

    /// <summary>
    /// 请求打开设置抽屉
    /// </summary>
    [RelayCommand]
    private void ShowSettings()
    {
        NavigationRequested?.Invoke(NavigationTarget.SettingsDrawer);
    }

    /// <summary>
    /// 退出程序, 由视图层关闭主窗口, 进行中的游戏由窗口关闭事件自动保存
    /// </summary>
    [RelayCommand]
    private void Exit()
    {
        ExitRequested?.Invoke();
    }

    /// <summary>
    /// 刷新存档状态, 由壳视图模型在切换到主视图时调用
    /// </summary>
    public void RefreshSaveDataState()
    {
        HasSaveData = _dataRepository.SaveData is not null;
    }

    /// <summary>
    /// 更新地雷数量上限并钳制当前值
    /// </summary>
    private void UpdateMaxMineCount()
    {
        MaxMineCount = Math.Max(0, ((Height ?? 0) * (Width ?? 0)) - 1);
        if (MineCount > MaxMineCount)
        {
            MineCount = MaxMineCount;
        }
    }
}
