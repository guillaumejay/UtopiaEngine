using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UE.Core.Architecture.Messages;
using UE.Core.Interfaces;
using UE.UI.Localization;

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

    public string Title => L.Camp;

    private void RefreshAll()
    {
        var gs = _engine.GameState;
        var def = _engine.GameDefinition;
        SkullsText = string.Format(L.SkullsFmt, gs.NumberOfSkullsCrossed, def.NumberOfSkulls);
        GodsHandText = string.Format(L.GodsHandFmt, gs.GodsHandEnergy, def.GodsHandCapacity);
        BasketText = string.Format(L.BasketFmt, gs.WasteBasket, UE.Core.Entities.GameState.WasteBasketCapacity);
        ComponentsText = string.Join("\n", def.Components.Select(c =>
            $"{c.Name.Text} : {gs.Inventory.GetComponentQuantityFor(c.ID)}"));
        ItemsText = $"{L.DowsingRodName} : {Charge(gs.Inventory.DowsingRodCharged)}\n" +
                    $"{L.ParalysisWandName} : {Charge(gs.Inventory.ParalysisWandCharged)}\n" +
                    $"{L.FocusCharmName} : {Charge(gs.Inventory.FocusCharmCharged)}";
        TreasuresText = gs.TreasuresFound.Any()
            ? string.Join("\n", gs.TreasuresFound.Select(t => t.Name.Text))
            : L.NoTreasures;
        CanUseGodsHand = _engine.IsGodsHandUsable && !IsGameOver;
        CanRest = _engine.CanRest && !IsGameOver;
        _shell.RefreshStatus();
    }

    private static string Charge(bool charged) => charged ? L.ChargedState : L.UsedState;

    [RelayCommand]
    private void UseGodsHand()
    {
        _engine.UseGodsHand();
        InfoMessage = L.SkullCrossed;
        RefreshAll();
    }

    [RelayCommand]
    private void Rest()
    {
        int days = (int)RestDays;
        if (days <= 0 || !_engine.CanRest)
            return;
        TimePassed t = _engine.Rest(days);
        var info = new List<string> { string.Format(L.Rested, t.DaysPassed, _engine.GameState.CurrentHitPoint) };
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
