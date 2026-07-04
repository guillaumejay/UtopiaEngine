using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UE.Core;
using UE.Core.Interfaces;
using UE.Core.Repository;

namespace UE.UI.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly IGameEngine _engine;

    [ObservableProperty]
    private ViewModelBase _currentPage;

    public MainViewModel()
    {
        _engine = new GameEngine(new XmlRepository(), new RandomDice());
        _engine.Init("Data\\DefinitionStandard.xml");
        _currentPage = new HomeViewModel(this);
    }

    public string Title => "Utopia Engine";

    public IGameEngine Engine => _engine;

    public int CurrentHitPoint => _engine.GameState.CurrentHitPoint;

    public int CurrentDay => _engine.GameState.CurrentDay;

    public int DaysRemaining => _engine.DaysRemaining;

    public int Score => _engine.Score;

    public string HelpLabel => HelpViewModel.IsFrench ? "Aide" : "Help";

    private ViewModelBase? _pageBeforeHelp;

    [RelayCommand]
    private void OpenHelp()
    {
        if (CurrentPage is HelpViewModel)
            return;
        _pageBeforeHelp = CurrentPage;
        HelpContext context = (CurrentPage as IHelpContextProvider)?.HelpContext ?? HelpContext.General;
        CurrentPage = new HelpViewModel(this, context);
    }

    public void CloseHelp() => CurrentPage = _pageBeforeHelp ?? new HomeViewModel(this);

    public void StartNewGame()
    {
        _engine.ResetGameState();
        RefreshStatus();
        ShowRegions();
    }

    public void ShowRegions() => CurrentPage = new RegionListViewModel(_engine, this);

    public void OpenSearch(int regionIndex) => CurrentPage = new SearchRegionViewModel(_engine, this, regionIndex);

    public void ShowConstructs() => CurrentPage = new ConstructsViewModel(_engine, this);

    public void OpenActivation(UE.Core.Entities.ConstructState cs) => CurrentPage = new ActivateConstructViewModel(_engine, this, cs);

    public void ShowLinks() => CurrentPage = new LinksViewModel(_engine, this);

    public void OpenLink(UE.Core.Entities.LinkState ls) => CurrentPage = new ConnectLinkViewModel(_engine, this, ls);

    public void ShowFinalActivation() => CurrentPage = new FinalActivationViewModel(_engine, this);

    public void RefreshStatus()
    {
        OnPropertyChanged(nameof(CurrentHitPoint));
        OnPropertyChanged(nameof(CurrentDay));
        OnPropertyChanged(nameof(DaysRemaining));
        OnPropertyChanged(nameof(Score));
    }
}
