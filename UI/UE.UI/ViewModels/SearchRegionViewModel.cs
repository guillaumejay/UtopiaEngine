using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UE.Core;
using UE.Core.Architecture;
using UE.Core.Architecture.Messages;
using UE.Core.Entities;
using UE.Core.Interfaces;
using UE.UI.Localization;

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

    public string UseWandLabel { get; } = string.Format(L.UseWandButton, GameEngine.ParalysisWandBonus);

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
        RegionName = string.Format(L.SearchingFor, _region.Name.Text, _region.Construct.Name.Text);
        engine.EnterRegionForSearch(regionIndex);
        StartBox();
    }

    private void StartBox()
    {
        if (EngineRef.NumberOfAvailableSearchesBoxesFor(_region.Index) == 0)
        {
            FinishBox(L.RegionExhausted, canContinue: false);
            return;
        }

        _box = EngineRef.CurrentSearchBoxForRegion(_region.Index);
        TimePassed t = EngineRef.CrossRegionTracker(_region);
        var info = new List<string>();
        if (t.DaysPassed > 0)
            info.Add(string.Format(L.SearchCostsDays, t.DaysPassed));
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
        ResultText = string.Format(L.SearchResultLabel, _result);
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
        ResultText = string.Format(L.SearchResultModified, _result);
    }

    [RelayCommand]
    private void ApplySearch()
    {
        SearchResult sr = EngineRef.ApplySearch(_result, _region, _modified);
        Shell.RefreshStatus();

        var lines = new List<string>();
        if (sr.ConstructFound)
            lines.Add(string.Format(
                sr.FinalResult == 0 ? L.ConstructFoundPerfect : L.ConstructFoundMsg,
                _region.Construct.Name.Text));
        if (sr.NumberOfComposantFound > 0)
            lines.Add(string.Format(L.ComponentsFoundMsg, sr.NumberOfComposantFound, _region.Component.Name.Text));
        if (sr.MonsterLevel > 0)
        {
            StartCombat(_region.GetEncounter(sr.MonsterLevel));
            return;
        }
        if (lines.Count == 0)
            lines.Add(L.NothingFound);

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
        CombatLog = string.Format(L.MustFight, e.Name.Text, e.AttackText, e.HitText, e.IsSpirit ? L.SpiritSuffix : string.Empty);
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
        AppendCombatLog(string.Format(L.YouRoll, _combatDice.First, _combatDice.Second));
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
        AppendCombatLog(string.Format(L.WandApplied, GameEngine.ParalysisWandBonus, _combatDice.First, _combatDice.Second));
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
        AppendCombatLog(L.YouFlee);
        EndCombat();
    }

    private void ResolveCombatTurn()
    {
        CombatResult cr = EngineRef.ApplyCombatRoll(_combatDice!, _encounter!, _region);
        Shell.RefreshStatus();
        if (cr.HitpointLost > 0)
            AppendCombatLog(string.Format(L.YouLoseHp, cr.HitpointLost));

        if (cr.EncounterDead)
        {
            AppendCombatLog(string.Format(L.EncounterDefeated, _encounter!.Name.Text));
            if (cr.ComponentFound)
                AppendCombatLog(string.Format(L.LootComponent, _region.Component.Name.Text));
            if (cr.LegendaryTreasureFound)
                AppendCombatLog(string.Format(L.LootTreasure, _region.LegendaryTreasure.Name.Text));
            Shell.RefreshStatus();
        }

        switch (ResolveHpAftermath(out string hpMessage))
        {
            case HpAftermath.Dead:
                GameLost(L.DiedToWounds);
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

    private void GameLost(string? reason = null)
    {
        reason ??= UiMessages.GameLostTime;
        string prefix = IsCombat ? CombatLog + "\n" : string.Empty;
        FinishBox(prefix + UiMessages.GameLost(reason), canContinue: false);
    }
}
