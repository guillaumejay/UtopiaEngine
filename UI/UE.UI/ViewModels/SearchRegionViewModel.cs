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

public partial class SearchCellViewModel(SearchRegionViewModel owner, int position) : ObservableObject
{
    public int Position { get; } = position;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEmpty))]
    [NotifyPropertyChangedFor(nameof(Display))]
    private string _value = string.Empty;

    public bool IsEmpty => string.IsNullOrEmpty(Value);

    public string Display => IsEmpty ? "·" : Value;

    [RelayCommand]
    private void Place() => owner.PlaceSelectedDie(this);
}

public enum SearchPhase
{
    Placing,
    BoxFull,
    Done,
}

public partial class SearchRegionViewModel : ViewModelBase
{
    private readonly IGameEngine _engine;
    private readonly MainViewModel _shell;
    private readonly Region _region;
    private Table? _box;
    private int _result;
    private bool _modified;

    public string RegionName { get; }

    public ObservableCollection<SearchCellViewModel> Cells { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPlacing))]
    [NotifyPropertyChangedFor(nameof(IsBoxFull))]
    [NotifyPropertyChangedFor(nameof(IsDone))]
    private SearchPhase _phase;

    public bool IsPlacing => Phase == SearchPhase.Placing;

    public bool IsBoxFull => Phase == SearchPhase.BoxFull;

    public bool IsDone => Phase == SearchPhase.Done;

    [ObservableProperty] private int _die1;
    [ObservableProperty] private int _die2;
    [ObservableProperty] private bool _die1Used;
    [ObservableProperty] private bool _die2Used;
    [ObservableProperty] private bool _isDie1Selected;
    [ObservableProperty] private bool _isDie2Selected;

    [ObservableProperty] private string _infoMessage = string.Empty;
    [ObservableProperty] private string _resultText = string.Empty;
    [ObservableProperty] private string _outcomeText = string.Empty;
    [ObservableProperty] private bool _canUseDowsingRod;
    [ObservableProperty] private decimal _dowsingRodNumber = 50;
    [ObservableProperty] private bool _canContinue;

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
        RollDice();
    }

    private void RebuildCells()
    {
        Cells.Clear();
        for (int i = 0; i < 6; i++)
            Cells.Add(new SearchCellViewModel(this, i + 1) { Value = _box![i] });
    }

    private void RollDice()
    {
        TwoDice td = _engine.DiceGenerator.Get2d6();
        Die1 = td.First;
        Die2 = td.Second;
        Die1Used = false;
        Die2Used = false;
        IsDie1Selected = true;
        IsDie2Selected = false;
    }

    public void PlaceSelectedDie(SearchCellViewModel cell)
    {
        if (Phase != SearchPhase.Placing || !cell.IsEmpty)
            return;

        int value;
        if (IsDie1Selected && !Die1Used)
            value = Die1;
        else if (IsDie2Selected && !Die2Used)
            value = Die2;
        else
            return;

        _engine.PlaceSearchNumberOnRegion(_region.Index, cell.Position, value);
        cell.Value = value.ToString();

        if (IsDie1Selected)
        {
            Die1Used = true;
            IsDie1Selected = false;
            IsDie2Selected = !Die2Used;
        }
        else
        {
            Die2Used = true;
            IsDie2Selected = false;
            IsDie1Selected = !Die1Used;
        }

        if (Die1Used && Die2Used)
        {
            if (_box!.IsFull)
                BoxFilled();
            else
                RollDice();
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
            Encounter e = _region.GetEncounter(sr.MonsterLevel);
            lines.Add($"Rencontre : {e.Name.Text} (niveau {sr.MonsterLevel}) — combat pas encore implémenté dans cette interface.");
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

    [RelayCommand]
    private void Continue() => StartBox();

    [RelayCommand]
    private void Back() => _shell.ShowRegions();

    private void GameLost()
    {
        OutcomeText = "Partie perdue — le temps vous a rattrapé.";
        CanContinue = false;
        Phase = SearchPhase.Done;
    }
}
