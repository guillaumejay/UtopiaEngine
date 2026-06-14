using UE.Core;
using UE.Core.Repository;

namespace UE.UI.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly GameEngine _engine;

    public MainViewModel()
    {
        _engine = new GameEngine(new XmlRepository(), new RandomDice());
        _engine.Init("Data\\DefinitionStandard.xml");
    }

    public string Title => "Utopia Engine";

    public int CurrentHitPoint => _engine.GameState.CurrentHitPoint;

    public int CurrentDay => _engine.GameState.CurrentDay;

    public int DaysRemaining => _engine.DaysRemaining;

    public int Score => _engine.Score;
}
