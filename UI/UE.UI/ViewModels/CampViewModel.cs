using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UE.Core.Architecture.Messages;
using UE.Core.Interfaces;

namespace UE.UI.ViewModels;

public partial class CampViewModel : ViewModelBase, IHelpContextProvider
{
    public HelpContext HelpContext => HelpContext.Camp;

    private readonly IGameEngine _engine;
    private readonly MainViewModel _shell;

    [ObservableProperty] private string _skullsText = string.Empty;
    [ObservableProperty] private string _godsHandText = string.Empty;
    [ObservableProperty] private string _basketText = string.Empty;
    [ObservableProperty] private string _componentsText = string.Empty;
    [ObservableProperty] private string _itemsText = string.Empty;
    [ObservableProperty] private string _treasuresText = string.Empty;
    [ObservableProperty] private string _infoMessage = string.Empty;
    [ObservableProperty] private bool _canUseGodsHand;
    [ObservableProperty] private bool _canRest;
    [ObservableProperty] private decimal _restDays = 1;
    [ObservableProperty] private bool _isGameOver;

    public CampViewModel(IGameEngine engine, MainViewModel shell)
    {
        _engine = engine;
        _shell = shell;
        RefreshAll();
    }

    public string Title => "Campement";

    private void RefreshAll()
    {
        var gs = _engine.GameState;
        var def = _engine.GameDefinition;
        SkullsText = $"Crânes barrés : {gs.NumberOfSkullsCrossed}/{def.NumberOfSkulls} (chaque crâne = +1 jour de délai)";
        GodsHandText = $"Énergie God's Hand : {gs.GodsHandEnergy}/{def.GodsHandCapacity}";
        BasketText = $"Corbeille : {gs.WasteBasket}/{UE.Core.Entities.GameState.WasteBasketCapacity} dés jetés";
        ComponentsText = string.Join("\n", def.Components.Select(c =>
            $"{c.Name.Text} : {gs.Inventory.GetComponentQuantityFor(c.ID)}"));
        ItemsText = $"Baguette de sourcier : {Charge(gs.Inventory.DowsingRodCharged)}\n" +
                    $"Baguette de paralysie : {Charge(gs.Inventory.ParalysisWandCharged)}\n" +
                    $"Charme de concentration : {Charge(gs.Inventory.FocusCharmCharged)}";
        TreasuresText = gs.TreasuresFound.Any()
            ? string.Join("\n", gs.TreasuresFound.Select(t => t.Name.Text))
            : "Aucun trésor légendaire pour l'instant.";
        CanUseGodsHand = _engine.IsGodsHandUsable && !IsGameOver;
        CanRest = _engine.CanRest && !IsGameOver;
        _shell.RefreshStatus();
    }

    private static string Charge(bool charged) => charged ? "chargée" : "utilisée";

    [RelayCommand]
    private void UseGodsHand()
    {
        _engine.UseGodsHand();
        InfoMessage = "La God's Hand frappe : un crâne est barré, +1 jour de délai.";
        RefreshAll();
    }

    [RelayCommand]
    private void Rest()
    {
        int days = (int)RestDays;
        if (days <= 0 || !_engine.CanRest)
            return;
        TimePassed t = _engine.Rest(days);
        var info = new List<string> { $"Vous vous reposez {t.DaysPassed} jour(s) — PV : {_engine.GameState.CurrentHitPoint}." };
        if (t.eventOccured)
            info.Add(UiMessages.EventsRolled);
        if (_engine.IsGameLost)
        {
            info.Add(UiMessages.GameLost(UiMessages.GameLostTime));
            IsGameOver = true;
        }
        InfoMessage = string.Join(" ", info);
        RefreshAll();
    }

    [RelayCommand]
    private void Back() => _shell.ShowRegions();
}
