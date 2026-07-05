using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using UE.Core.Entities;
using UE.Core.Interfaces;
using UE.UI.Localization;

namespace UE.UI.ViewModels;

public partial class ConstructItemViewModel(ConstructState cs, Region region, MainViewModel shell) : ViewModelBase
{
    public string Name { get; } = cs.Construct.Name.Text;

    public string RegionName { get; } = region.Name.Text;

    public string Status { get; } = !cs.HasBeenFound
        ? L.NotFoundState
        : cs.HasBeenActivated ? L.ActivatedState : L.FoundToActivateState;

    public bool CanActivate { get; } = cs.HasBeenFound && !cs.HasBeenActivated;

    [RelayCommand]
    private void Activate() => shell.OpenActivation(cs);
}

public partial class ConstructsViewModel : ViewModelBase, IHelpContextProvider
{
    public HelpContext HelpContext => HelpContext.Constructs;

    public List<ConstructItemViewModel> Constructs { get; }

    private readonly MainViewModel _shell;

    public ConstructsViewModel(IGameEngine engine, MainViewModel shell)
    {
        _shell = shell;
        Constructs = engine.GameState.Constructs
            .Select(cs => new ConstructItemViewModel(cs, engine.GetRegionForConstruct(cs), shell))
            .ToList();
    }

    public string Title => L.ConstructsTitle;

    [RelayCommand]
    private void Back() => _shell.ShowRegions();
}
