using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using UE.Core.Entities;
using UE.Core.Interfaces;
using UE.UI.Localization;

namespace UE.UI.ViewModels;

public partial class LinkItemViewModel : ViewModelBase
{
    private readonly MainViewModel _shell;
    private readonly LinkState _ls;

    public LinkItemViewModel(LinkState ls, IGameEngine engine, MainViewModel shell)
    {
        _shell = shell;
        _ls = ls;
        Name = $"{ls.Construct1.Construct.Name.Text} — {ls.Construct2.Construct.Name.Text}";
        string componentName = engine.GameDefinition.Components.Single(c => c.ID == ls.Link.ComponentID).Name.Text;
        // L'éligibilité vient du moteur (PossibleLinks) ; les branches ne servent qu'au message.
        CanConnect = engine.GameState.PossibleLinks.Contains(ls);
        if (CanConnect)
            Status = string.Format(L.LinkReady, componentName);
        else if (ls.IsLinkDone)
            Status = string.Format(L.LinkConnected, ls.LinkBox);
        else if (!ls.Construct1.HasBeenActivated || !ls.Construct2.HasBeenActivated)
            Status = L.LinkNeedsActivated;
        else
            Status = string.Format(L.LinkNoComponent, componentName);
    }

    public string Name { get; }

    public string Status { get; }

    public bool CanConnect { get; }

    [RelayCommand]
    private void Connect() => _shell.OpenLink(_ls);
}

public partial class LinksViewModel : ViewModelBase, IHelpContextProvider
{
    public HelpContext HelpContext => HelpContext.Links;

    private readonly MainViewModel _shell;

    public LinksViewModel(IGameEngine engine, MainViewModel shell)
    {
        _shell = shell;
        Links = engine.GameState.LinkStates
            .Select(ls => new LinkItemViewModel(ls, engine, shell))
            .ToList();
        int done = engine.GameState.ConnectedLinks.Count();
        Summary = string.Format(L.LinksSummary, done, engine.GameState.LinkStates.Count, engine.GameState.FinalActivationDifficulty);
        CanFinalActivate = engine.IsFinalActivationPossible;
    }

    public bool CanFinalActivate { get; }

    [RelayCommand]
    private void FinalActivation() => _shell.ShowFinalActivation();

    public string Title => L.LinksTitle;

    public string Summary { get; }

    public List<LinkItemViewModel> Links { get; }

    [RelayCommand]
    private void Back() => _shell.ShowRegions();
}
