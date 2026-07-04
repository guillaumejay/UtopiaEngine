using CommunityToolkit.Mvvm.ComponentModel;
using UE.Core;
using UE.Core.Repository;

namespace UE.UI.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly GameEngine _engine;

    [ObservableProperty]
    private ViewModelBase _currentPage;

    public MainViewModel()
    {
        _engine = new GameEngine(new XmlRepository(), new RandomDice());
        _engine.Init("Data\\DefinitionStandard.xml");
        _currentPage = new HomeViewModel(this);
    }

    public string Title => "Utopia Engine";

    public int CurrentHitPoint => _engine.GameState.CurrentHitPoint;

    public int CurrentDay => _engine.GameState.CurrentDay;

    public int DaysRemaining => _engine.DaysRemaining;

    public int Score => _engine.Score;

    public void StartNewGame()
    {
        _engine.ResetGameState();
        RefreshStatus();
        ShowRegions();
    }

    public void ShowRegions() => CurrentPage = new RegionListViewModel(_engine, this);

    public void OpenSearch(int regionIndex) => CurrentPage = new SearchRegionViewModel(_engine, this, regionIndex);

    public void RefreshStatus()
    {
        OnPropertyChanged(nameof(CurrentHitPoint));
        OnPropertyChanged(nameof(CurrentDay));
        OnPropertyChanged(nameof(DaysRemaining));
        OnPropertyChanged(nameof(Score));
    }
}
