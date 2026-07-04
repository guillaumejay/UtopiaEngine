using System.Collections.Generic;
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

public partial class SearchRegionViewModel : DicePlacementPageViewModel, IHelpContextProvider
{
    public HelpContext HelpContext => Phase switch
    {
        SearchPhase.BoxFull => HelpContext.SearchResult,
        SearchPhase.Combat => HelpContext.Combat,
        SearchPhase.Done => HelpContext.AfterSearch,
        _ => HelpContext.Placing,
    };

    private readonly Region _region;
    private Table? _box;
    private int _result;
    private bool _modified;

    public string RegionName { get; }

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
        : base(engine, shell)
    {
        _region = engine.GetRegion(regionIndex);
        RegionName = $"{_region.Name.Text} — à la recherche de {_region.Construct.Name.Text}";
        engine.EnterRegionForSearch(regionIndex);
        StartBox();
    }

    private void StartBox()
    {
        if (EngineRef.NumberOfAvailableSearchesBoxesFor(_region.Index) == 0)
        {
            FinishBox("Région entièrement fouillée.", canContinue: false);
            return;
        }

        _box = EngineRef.CurrentSearchBoxForRegion(_region.Index);
        TimePassed t = EngineRef.CrossRegionTracker(_region);
        var info = new List<string>();
        if (t.DaysPassed > 0)
            info.Add($"Cette fouille coûte {t.DaysPassed} jour(s).");
        if (t.eventOccured)
            info.Add(UiMessages.EventsRolled);
        InfoMessage = string.Join(" ", info);
        Shell.RefreshStatus();

        if (EngineRef.IsGameLost)
        {
            GameLost();
            return;
        }

        _modified = false;
        SyncCells(6, i => _box![i]);
        Phase = SearchPhase.Placing;
        Dice.Roll(EngineRef.DiceGenerator);
    }

    protected override void PlaceDie(DiceCellViewModel cell)
    {
        if (Phase != SearchPhase.Placing || !TryTakeDie(cell, out int value, out _))
            return;

        EngineRef.PlaceSearchNumberOnRegion(_region.Index, cell.Position, value);
        cell.Value = value.ToString();

        if (Dice.BothUsed)
        {
            if (_box!.IsFull)
                BoxFilled();
            else
                Dice.Roll(EngineRef.DiceGenerator);
        }
    }

    private void BoxFilled()
    {
        _result = _box!.SearchResult;
        ResultText = $"Résultat de fouille : {_result}";
        CanSearchResult crs = EngineRef.CanModifySearchResult(_result, _region.Index);
        CanUseDowsingRod = crs.CanModify && crs.CanUseDowsingRod;
        Phase = SearchPhase.BoxFull;
    }

    [RelayCommand]
    private void UseDowsingRod()
    {
        _result = EngineRef.UseDowsingRod(_result, (int)DowsingRodNumber);
        _modified = true;
        CanUseDowsingRod = false;
        ResultText = $"Résultat de fouille : {_result} (modifié par la baguette)";
    }

    [RelayCommand]
    private void ApplySearch()
    {
        SearchResult sr = EngineRef.ApplySearch(_result, _region, _modified);
        Shell.RefreshStatus();

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

        if (EngineRef.IsGameLost)
        {
            GameLost();
            return;
        }

        FinishBox(string.Join("\n", lines));
    }

    private void StartCombat(Encounter e)
    {
        _encounter = e;
        EngineRef.StartFight();
        CombatLog = $"Vous devez combattre {e.Name.Text} (attaque sur {e.AttackText}, touché sur {e.HitText}{(e.IsSpirit ? ", esprit" : "")}).";
        CanFlee = EngineRef.HasAbility(Ability.IgnoreEncounter);
        CanRollCombat = true;
        ShowWandChoice = false;
        Phase = SearchPhase.Combat;
    }

    private void AppendCombatLog(string line) => CombatLog += "\n" + line;

    [RelayCommand]
    private void RollCombat()
    {
        _combatDice = EngineRef.RollCombatDice(_encounter!);
        AppendCombatLog($"Vous lancez {_combatDice.First} et {_combatDice.Second}.");
        CanRollCombat = false;
        if (EngineRef.GameState.Inventory.ParalysisWandCharged && !EngineRef.GameState.ParalysisWandInUse)
            ShowWandChoice = true;
        else
            ResolveCombatTurn();
    }

    [RelayCommand]
    private void UseWand()
    {
        EngineRef.UseParalysisWand();
        _combatDice!.ModifyBothDie(GameEngine.ParalysisWandBonus);
        AppendCombatLog($"Baguette de paralysie : +{GameEngine.ParalysisWandBonus} aux dés pour tout le combat, soit {_combatDice.First} et {_combatDice.Second}.");
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
        CombatResult cr = EngineRef.ApplyCombatRoll(_combatDice!, _encounter!, _region);
        Shell.RefreshStatus();
        if (cr.HitpointLost > 0)
            AppendCombatLog($"Vous perdez {cr.HitpointLost} PV.");

        if (cr.EncounterDead)
        {
            AppendCombatLog($"{_encounter!.Name.Text} est vaincu !");
            if (cr.ComponentFound)
                AppendCombatLog($"Butin : 1 × {_region.Component.Name.Text}.");
            if (cr.LegendaryTreasureFound)
                AppendCombatLog($"Butin : {_region.LegendaryTreasure.Name.Text} (trésor légendaire) !");
            Shell.RefreshStatus();
        }

        switch (ResolveHpAftermath(out string hpMessage))
        {
            case HpAftermath.Dead:
                GameLost("vous succombez à vos blessures");
                return;
            case HpAftermath.TimeOut:
                AppendCombatLog(hpMessage);
                GameLost();
                return;
            case HpAftermath.Unconscious:
                AppendCombatLog(hpMessage);
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

    private void EndCombat() => FinishBox(CombatLog);

    private void FinishBox(string outcome, bool? canContinue = null)
    {
        OutcomeText = outcome;
        CanContinue = canContinue ?? EngineRef.NumberOfAvailableSearchesBoxesFor(_region.Index) > 0;
        Phase = SearchPhase.Done;
    }

    [RelayCommand]
    private void Continue() => StartBox();

    [RelayCommand]
    private void Back() => Shell.ShowRegions();

    private void GameLost(string reason = UiMessages.GameLostTime)
    {
        string prefix = IsCombat ? CombatLog + "\n" : string.Empty;
        FinishBox(prefix + UiMessages.GameLost(reason), canContinue: false);
    }
}
