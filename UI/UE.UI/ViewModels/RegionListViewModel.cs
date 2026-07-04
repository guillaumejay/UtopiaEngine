using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using UE.Core.Entities;
using UE.Core.Interfaces;
using UE.UI.Localization;

namespace UE.UI.ViewModels;

public partial class RegionItemViewModel(RegionState rs, Inventory inventory, MainViewModel shell) : ViewModelBase
{
    public int Index { get; } = rs.Region.Index;

    public string Name { get; } = rs.Region.Name.Text;

    public string ComponentInfo { get; } = string.Format(L.ComponentInfo,
        rs.Region.Component.Name.Text, inventory.GetComponentQuantityFor(rs.Region.Component.ID));

    public string ConstructInfo { get; } = string.Format(L.ConstructInfo,
        rs.Region.Construct.Name.Text, rs.ConstructFound ? L.FoundState : L.ToFindState);

    public string TreasureInfo { get; } = string.Format(L.TreasureInfo,
        rs.Region.LegendaryTreasure.Name.Text, rs.LegendaryTreasureFound ? L.FoundState : L.ToFindState);

    public string EventsText { get; } = string.Format(L.EventsPrefix,
        string.Join(", ", rs.Events.Select(x => x.Name.Text)));

    public bool HasEvents { get; } = rs.Events.Count > 0;

    [RelayCommand]
    private void Search() => shell.OpenSearch(Index);
}

public partial class RegionListViewModel : ViewModelBase, IHelpContextProvider
{
    public HelpContext HelpContext => HelpContext.Regions;

    public List<RegionItemViewModel> Regions { get; }

    private readonly MainViewModel _shell;

    public RegionListViewModel(IGameEngine engine, MainViewModel shell)
    {
        _shell = shell;
        Regions = engine.GameState.RegionStates
            .Select(rs => new RegionItemViewModel(rs, engine.GameState.Inventory, shell))
            .ToList();
        int found = engine.GameState.ConstructsFound.Count();
        int toActivate = engine.GameState.ConstructsUnactivated.Count();
        ConstructsLabel = toActivate > 0
            ? string.Format(L.ConstructsToActivate, toActivate)
            : string.Format(L.ConstructsFoundLabel, found);
        HasConstructs = found > 0;
        int possible = engine.GameState.PossibleLinks.Count();
        int connected = engine.GameState.ConnectedLinks.Count();
        LinksLabel = possible > 0
            ? string.Format(L.LinksPossibleLabel, possible)
            : string.Format(L.LinksConnectedLabel, connected);
        HasLinks = possible > 0 || connected > 0;
    }

    public string ConstructsLabel { get; }

    public bool HasConstructs { get; }

    public string LinksLabel { get; }

    public bool HasLinks { get; }

    [RelayCommand]
    private void ShowConstructs() => _shell.ShowConstructs();

    [RelayCommand]
    private void ShowLinks() => _shell.ShowLinks();

    [RelayCommand]
    private void ShowCamp() => _shell.ShowCamp();
}
