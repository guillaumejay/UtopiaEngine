using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using UE.Core.Entities;
using UE.Core.Interfaces;

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
        if (ls.IsLinkDone)
        {
            Status = $"Connecté (valeur {ls.LinkBox})";
        }
        else if (!ls.Construct1.HasBeenActivated || !ls.Construct2.HasBeenActivated)
        {
            Status = "Indisponible — les deux constructs doivent être activés";
        }
        else if (engine.GameState.NumberOfComponents(ls.Link.ComponentID) == 0)
        {
            Status = $"Indisponible — aucun composant {componentName}";
        }
        else
        {
            Status = $"Prêt à relier (coût : 1 × {componentName})";
            CanConnect = true;
        }
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
        Summary = $"{done} lien(s) sur {engine.GameState.LinkStates.Count} — difficulté d'activation finale actuelle : {engine.GameState.FinalActivationDifficulty}";
        CanFinalActivate = engine.IsFinalActivationPossible;
    }

    public bool CanFinalActivate { get; }

    [RelayCommand]
    private void FinalActivation() => _shell.ShowFinalActivation();

    public string Title => "Liens entre constructs";

    public string Summary { get; }

    public List<LinkItemViewModel> Links { get; }

    [RelayCommand]
    private void Back() => _shell.ShowRegions();
}
