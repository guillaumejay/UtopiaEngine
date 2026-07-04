using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UE.Core.Architecture.Messages;
using UE.Core.Interfaces;
using UE.UI.Localization;

namespace UE.UI.ViewModels;

public partial class FinalActivationViewModel : ViewModelBase, IHelpContextProvider
{
    public HelpContext HelpContext => HelpContext.Final;

    private readonly IGameEngine _engine;
    private readonly MainViewModel _shell;

    [ObservableProperty] private string _difficultyText = string.Empty;
    [ObservableProperty] private string _log = string.Empty;
    [ObservableProperty] private bool _canSpendHp;
    [ObservableProperty] private decimal _hpToSpend = 1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsRolling))]
    private bool _isFinished;

    public bool IsRolling => !IsFinished;

    [ObservableProperty] private string _outcomeText = string.Empty;

    public FinalActivationViewModel(IGameEngine engine, MainViewModel shell)
    {
        _engine = engine;
        _shell = shell;
        Log = L.FinalIntro;
        CanSpendHp = engine.GameState.CurrentHitPoint > 0;
        MaxHpToSpend = engine.GameState.CurrentHitPoint;
        RefreshDifficulty();
    }

    public string Title => L.FinalTitle;

    public int MaxHpToSpend { get; }

    private void RefreshDifficulty() =>
        DifficultyText = string.Format(L.FinalDifficulty, _engine.GameState.FinalActivationDifficulty);

    private void AppendLog(string line) => Log += "\n" + line;

    [RelayCommand]
    private void SpendHp()
    {
        int hp = (int)HpToSpend;
        if (hp <= 0 || hp > _engine.GameState.CurrentHitPoint)
            return;
        _engine.SpendHPToReduceActivationDifficulty(hp);
        CanSpendHp = false;
        _shell.RefreshStatus();
        RefreshDifficulty();
        AppendLog(string.Format(L.SacrificeDone, hp));
    }

    [RelayCommand]
    private void Roll()
    {
        FinalActivationResult far = _engine.WorkForfinalActivation();
        _shell.RefreshStatus();
        AppendLog(string.Format(L.FinalRollLine, far.Roll.First, far.Roll.Second, far.Roll.Sum));

        if (far.GameWon)
        {
            Finish(L.Victory + "\n" + string.Format(L.FinalScore, _engine.Score));
            return;
        }

        AppendLog(L.FinalFail);
        if (far.eventOccured)
            AppendLog(UiMessages.EventsRolled);

        if (_engine.IsGameLost)
        {
            string reason = _engine.GameState.CurrentHitPoint < 0
                ? L.LifeExhausted
                : UiMessages.GameLostTime;
            Finish(UiMessages.GameLost(reason) + "\n" + string.Format(L.FinalScore, _engine.Score));
            return;
        }

        CanSpendHp = false;
    }

    private void Finish(string outcome)
    {
        OutcomeText = outcome;
        IsFinished = true;
        CanSpendHp = false;
    }

    [RelayCommand]
    private void Back() => _shell.ShowLinks();

    [RelayCommand]
    private void NewGame() => _shell.StartNewGame();
}
