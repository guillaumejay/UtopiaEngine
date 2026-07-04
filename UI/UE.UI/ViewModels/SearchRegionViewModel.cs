using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UE.Core;
using UE.Core.Architecture;
using UE.Core.Architecture.Messages;
using UE.Core.Entities;
using UE.Core.Interfaces;

namespace UE.UI.ViewModels;

public enum SearchPhase
{
    Placing,
    BoxFull,
    Combat,
    Done,
}

public partial class SearchRegionViewModel : ViewModelBase, IHelpContextProvider
{
    public HelpContext HelpContext => Phase switch
    {
        SearchPhase.BoxFull => HelpContext.SearchResult,
        SearchPhase.Combat => HelpContext.Combat,
        SearchPhase.Done => HelpContext.AfterSearch,
        _ => HelpContext.Placing,
    };

    private readonly IGameEngine _engine;
    private readonly MainViewModel _shell;
    private readonly Region _region;
    private Table? _box;
    private int _result;
    private bool _modified;

    public string RegionName { get; }

    public ObservableCollection<DiceCellViewModel> Cells { get; } = new();

    public DicePairViewModel Dice { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPlacing))]
    [NotifyPropertyChangedFor(nameof(IsBoxFull))]
    [NotifyPropertyChangedFor(nameof(IsCombat))]
    [NotifyPropertyChangedFor(nameof(IsDone))]
    private SearchPhase _phase;

    public bool IsPlacing => Phase == SearchPhase.Placing;

    public bool IsBoxFull => Phase == SearchPhase.BoxFull;

    public bool IsCombat => Phase == SearchPhase.Combat;

    public bool IsDone => Phase == SearchPhase.Done;

    [ObservableProperty] private string _infoMessage = string.Empty;
    [ObservableProperty] private string _resultText = string.Empty;
    [ObservableProperty] private string _outcomeText = string.Empty;
    [ObservableProperty] private bool _canUseDowsingRod;
    [ObservableProperty] private decimal _dowsingRodNumber = 50;
    [ObservableProperty] private bool _canContinue;

    private Encounter? _encounter;
    private TwoDice? _combatDice;

    [ObservableProperty] private string _combatLog = string.Empty;
    [ObservableProperty] private bool _canRollCombat;
    [ObservableProperty] private bool _showWandChoice;
    [ObservableProperty] private bool _canFlee;

    public SearchRegionViewModel(IGameEngine engine, MainViewModel shell, int regionIndex)
    {
        _engine = engine;
        _shell = shell;
        _region = engine.GetRegion(regionIndex);
        RegionName = $"{_region.Name.Text} — à la recherche de {_region.Construct.Name.Text}";
        engine.EnterRegionForSearch(regionIndex);
        StartBox();
    }

    private void StartBox()
    {
        if (_engine.NumberOfAvailableSearchesBoxesFor(_region.Index) == 0)
        {
            OutcomeText = "Région entièrement fouillée.";
            CanContinue = false;
            Phase = SearchPhase.Done;
            return;
        }

        _box = _engine.CurrentSearchBoxForRegion(_region.Index);
        TimePassed t = _engine.CrossRegionTracker(_region);
        var info = new List<string>();
        if (t.DaysPassed > 0)
            info.Add($"Cette fouille coûte {t.DaysPassed} jour(s).");
        if (t.eventOccured)
            info.Add("De nouveaux événements se produisent dans les régions !");
        InfoMessage = string.Join(" ", info);
        _shell.RefreshStatus();

        if (_engine.IsGameLost)
        {
            GameLost();
            return;
        }

        _modified = false;
        RebuildCells();
        Phase = SearchPhase.Placing;
        Dice.Roll(_engine.DiceGenerator);
    }

    private void RebuildCells()
    {
        Cells.Clear();
        for (int i = 0; i < 6; i++)
            Cells.Add(new DiceCellViewModel(i + 1, PlaceSelectedDie) { Value = _box![i] });
    }

    private void PlaceSelectedDie(DiceCellViewModel cell)
    {
        if (Phase != SearchPhase.Placing || !cell.IsEmpty)
            return;

        int? value = Dice.TakeSelected();
        if (value is null)
            return;

        _engine.PlaceSearchNumberOnRegion(_region.Index, cell.Position, value.Value);
        cell.Value = value.Value.ToString();

        if (Dice.BothUsed)
        {
            if (_box!.IsFull)
                BoxFilled();
            else
                Dice.Roll(_engine.DiceGenerator);
        }
    }

    private void BoxFilled()
    {
        _result = _box!.SearchResult;
        ResultText = $"Résultat de fouille : {_result}";
        CanSearchResult crs = _engine.CanModifySearchResult(_result, _region.Index);
        CanUseDowsingRod = crs.CanModify && crs.CanUseDowsingRod;
        Phase = SearchPhase.BoxFull;
    }

    [RelayCommand]
    private void UseDowsingRod()
    {
        _result = _engine.UseDowsingRod(_result, (int)DowsingRodNumber);
        _modified = true;
        CanUseDowsingRod = false;
        ResultText = $"Résultat de fouille : {_result} (modifié par la baguette)";
    }

    [RelayCommand]
    private void ApplySearch()
    {
        SearchResult sr = _engine.ApplySearch(_result, _region, _modified);
        _shell.RefreshStatus();

        var lines = new List<string>();
        if (sr.ConstructFound)
            lines.Add(sr.FinalResult == 0
                ? $"{_region.Construct.Name.Text} trouvé et activé (zéro parfait, +5 énergie God's Hand) !"
                : $"{_region.Construct.Name.Text} trouvé !");
        if (sr.NumberOfComposantFound > 0)
            lines.Add($"{sr.NumberOfComposantFound} × {_region.Component.Name.Text} trouvé(s).");
        if (sr.MonsterLevel > 0)
        {
            StartCombat(_region.GetEncounter(sr.MonsterLevel));
            return;
        }
        if (lines.Count == 0)
            lines.Add("Rien trouvé cette fois-ci.");
        OutcomeText = string.Join("\n", lines);

        if (_engine.IsGameLost)
        {
            GameLost();
            return;
        }

        CanContinue = _engine.NumberOfAvailableSearchesBoxesFor(_region.Index) > 0;
        Phase = SearchPhase.Done;
    }

    private void StartCombat(Encounter e)
    {
        _encounter = e;
        _engine.StartFight();
        CombatLog = $"Vous devez combattre {e.Name.Text} (attaque sur {e.AttackText}, touché sur {e.HitText}{(e.IsSpirit ? ", esprit" : "")}).";
        CanFlee = _engine.HasAbility(Ability.IgnoreEncounter);
        CanRollCombat = true;
        ShowWandChoice = false;
        Phase = SearchPhase.Combat;
    }

    private void AppendCombatLog(string line) => CombatLog += "\n" + line;

    [RelayCommand]
    private void RollCombat()
    {
        _combatDice = _engine.DiceGenerator.Get2d6();
        if (_engine.GameState.ParalysisWandInUse)
            _combatDice.ModifyBothDie(2);
        if (_encounter!.IsSpirit && _engine.HasAbility(Ability.HelpAgainstSpirit))
            _combatDice.ModifyBothDie(1);
        AppendCombatLog($"Vous lancez {_combatDice.First} et {_combatDice.Second}.");
        CanRollCombat = false;
        if (_engine.GameState.Inventory.ParalysisWandCharged && !_engine.GameState.ParalysisWandInUse)
            ShowWandChoice = true;
        else
            ResolveCombatTurn();
    }

    [RelayCommand]
    private void UseWand()
    {
        _engine.UseParalysisWand();
        _combatDice!.ModifyBothDie(2);
        AppendCombatLog($"Baguette de paralysie : +2 aux dés pour tout le combat, soit {_combatDice.First} et {_combatDice.Second}.");
        ShowWandChoice = false;
        ResolveCombatTurn();
    }

    [RelayCommand]
    private void SkipWand()
    {
        ShowWandChoice = false;
        ResolveCombatTurn();
    }

    [RelayCommand]
    private void Flee()
    {
        AppendCombatLog("Vous fuyez le combat.");
        EndCombat();
    }

    private void ResolveCombatTurn()
    {
        CombatResult cr = _engine.ApplyCombatRoll(_combatDice!, _encounter!, _region);
        _shell.RefreshStatus();
        if (cr.HitpointLost > 0)
            AppendCombatLog($"Vous perdez {cr.HitpointLost} PV.");

        if (cr.EncounterDead)
        {
            AppendCombatLog($"{_encounter!.Name.Text} est vaincu !");
            if (cr.ComponentFound)
                AppendCombatLog($"Butin : 1 × {_region.Component.Name.Text}.");
            if (cr.LegendaryTreasureFound)
                AppendCombatLog($"Butin : {_region.LegendaryTreasure.Name.Text} (trésor légendaire) !");
            _shell.RefreshStatus();
        }

        if (_engine.GameState.CurrentHitPoint < 0)
        {
            GameLost("vous succombez à vos blessures");
            return;
        }

        if (_engine.GameState.CurrentHitPoint == 0)
        {
            TimePassed t = _engine.RecoverFromUnconsciousness();
            _shell.RefreshStatus();
            AppendCombatLog($"Inconscient ! Vous vous réveillez après {t.DaysPassed} jour(s), soigné.");
            if (_engine.IsGameLost)
            {
                GameLost();
                return;
            }
            EndCombat();
            return;
        }

        if (cr.EncounterDead)
        {
            EndCombat();
            return;
        }

        CanRollCombat = true;
    }

    private void EndCombat()
    {
        OutcomeText = CombatLog;
        CanContinue = _engine.NumberOfAvailableSearchesBoxesFor(_region.Index) > 0;
        Phase = SearchPhase.Done;
    }

    [RelayCommand]
    private void Continue() => StartBox();

    [RelayCommand]
    private void Back() => _shell.ShowRegions();

    private void GameLost(string reason = "le temps vous a rattrapé")
    {
        string prefix = IsCombat ? CombatLog + "\n" : string.Empty;
        OutcomeText = $"{prefix}Partie perdue — {reason}.";
        CanContinue = false;
        Phase = SearchPhase.Done;
    }
}
