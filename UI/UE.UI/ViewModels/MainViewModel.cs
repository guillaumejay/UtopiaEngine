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

    public int GodsHandEnergy => _engine.GameState.GodsHandEnergy;

    public string HpText => string.Format(Localization.L.StatusHp, CurrentHitPoint);

    public string DayText => string.Format(Localization.L.StatusDay, CurrentDay);

    public string DaysRemainingText => string.Format(Localization.L.StatusDaysRemaining, DaysRemaining);

    public string ScoreText => string.Format(Localization.L.StatusScore, Score);

    public string GodsHandText => string.Format(Localization.L.StatusGodsHand, GodsHandEnergy);

    public string HelpLabel => Localization.L.Help;

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

    public void ShowCamp() => CurrentPage = new CampViewModel(_engine, this);

    private static string AutosavePath => AppData.PathFor("autosave.xml");

    public bool HasAutosave
    {
        get
        {
            try { return System.IO.File.Exists(AutosavePath); }
            catch { return false; }
        }
    }

    public void ContinueGame()
    {
        _engine.LoadGameState(AutosavePath);
        RefreshStatus();
        ShowRegions();
    }

    private void Autosave()
    {
        // Sauvegarde silencieuse après chaque action ; supprimée quand la partie est finie.
        try
        {
            if (_engine.IsFinished)
            {
                System.IO.File.Delete(AutosavePath);
                return;
            }
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(AutosavePath)!);
            _engine.SaveGameState(AutosavePath);
        }
        catch
        {
            // Plateformes sans système de fichiers persistant (navigateur) : on joue sans sauvegarde.
        }
    }

    public void RefreshStatus()
    {
        NotifyStatusChanged();
        Autosave();
    }

    /// <summary>Après un changement de langue : retraduit la barre de statut et recrée la page d'accueil.</summary>
    public void OnLanguageChanged()
    {
        OnPropertyChanged(nameof(HelpLabel));
        NotifyStatusChanged();
        CurrentPage = new HomeViewModel(this);
    }

    private void NotifyStatusChanged()
    {
        OnPropertyChanged(nameof(CurrentHitPoint));
        OnPropertyChanged(nameof(CurrentDay));
        OnPropertyChanged(nameof(DaysRemaining));
        OnPropertyChanged(nameof(Score));
        OnPropertyChanged(nameof(GodsHandEnergy));
        OnPropertyChanged(nameof(HpText));
        OnPropertyChanged(nameof(DayText));
        OnPropertyChanged(nameof(DaysRemainingText));
        OnPropertyChanged(nameof(ScoreText));
        OnPropertyChanged(nameof(GodsHandText));
    }
}
