using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UE.Core.Architecture.Messages;
using UE.Core.Interfaces;

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
        Log = "Les six constructs sont reliés : l'Utopia Engine est prêt. Lancez 2d6 — atteignez ou dépassez la difficulté pour sauver le monde. Chaque échec coûte 1 PV et 1 jour.";
        CanSpendHp = engine.GameState.CurrentHitPoint > 0;
        MaxHpToSpend = engine.GameState.CurrentHitPoint;
        RefreshDifficulty();
    }

    public string Title => "Activation finale";

    public int MaxHpToSpend { get; }

    private void RefreshDifficulty() =>
        DifficultyText = $"Difficulté : {_engine.GameState.FinalActivationDifficulty} (2d6, somme à atteindre)";

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
        AppendLog($"Vous canalisez votre énergie vitale : −{hp} PV, difficulté réduite d'autant. Il n'y aura pas de second sacrifice.");
    }

    [RelayCommand]
    private void Roll()
    {
        FinalActivationResult far = _engine.WorkForfinalActivation();
        _shell.RefreshStatus();
        AppendLog($"Vous lancez {far.Roll.First} et {far.Roll.Second} : {far.Roll.Sum}.");

        if (far.GameWon)
        {
            Finish($"L'UTOPIA ENGINE S'ÉVEILLE — le monde est sauvé !\nScore final : {_engine.Score}.");
            return;
        }

        AppendLog("L'Engine gronde mais ne démarre pas : −1 PV, +1 jour.");
        if (far.eventOccured)
            AppendLog(UiMessages.EventsRolled);

        if (_engine.IsGameLost)
        {
            string reason = _engine.GameState.CurrentHitPoint < 0
                ? "votre énergie vitale est épuisée"
                : UiMessages.GameLostTime;
            Finish(UiMessages.GameLost(reason) + $"\nScore final : {_engine.Score}.");
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
