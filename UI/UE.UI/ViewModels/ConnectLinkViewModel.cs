using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UE.Core.Architecture.Messages;
using UE.Core.Entities;
using UE.Core.Interfaces;
using UE.UI.Localization;

namespace UE.UI.ViewModels;

public partial class ConnectLinkViewModel : DicePlacementPageViewModel, IHelpContextProvider
{
    public HelpContext HelpContext => HelpContext.Links;

    private readonly LinkState _ls;

    public string Title { get; }

    [ObservableProperty] private string _infoMessage = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPlacing))]
    private bool _isDone;

    public bool IsPlacing => !IsDone;

    [ObservableProperty] private string _outcomeText = string.Empty;
    [ObservableProperty] private string _basketLabel = string.Empty;
    [ObservableProperty] private bool _canUseBasket;
    [ObservableProperty] private bool _canAutoConnect;

    public ConnectLinkViewModel(IGameEngine engine, MainViewModel shell, LinkState ls)
        : base(engine, shell)
    {
        _ls = ls;
        Title = string.Format(L.ConnectTitle, ls.Construct1.Construct.Name.Text, ls.Construct2.Construct.Name.Text);
        InfoMessage = L.ConnectHint;
        CanAutoConnect = engine.HasAbility(Ability.AutomaticallyConnect);
        RefreshBasket();
        SyncCells(6, i => _ls.Connection[i]);
        Dice.Roll(engine.DiceGenerator);
    }

    private void RefreshBasket()
    {
        CanUseBasket = !EngineRef.GameState.IsWasteBasketFull;
        BasketLabel = string.Format(L.BasketLabelFmt, EngineRef.GameState.WasteBasket, GameState.WasteBasketCapacity);
    }

    [RelayCommand]
    private void AutoConnect()
    {
        EngineRef.UseAutomaticConnect(_ls);
        Finish(string.Format(L.AutoConnected, _ls.LinkBox));
    }

    [RelayCommand]
    private void Discard()
    {
        if (IsDone || EngineRef.GameState.IsWasteBasketFull || !TryTakeDie(null, out int value, out bool isFirst))
            return;
        EngineRef.WorkToLink(_ls.ID, 0, value, isFirst);
        RefreshBasket();
        RerollIfBothUsed();
    }

    protected override void PlaceDie(DiceCellViewModel cell)
    {
        if (IsDone || !TryTakeDie(cell, out int value, out bool isFirst))
            return;

        LinkResult lr = EngineRef.WorkToLink(_ls.ID, cell.Position, value, isFirst);
        Shell.RefreshStatus();
        RefreshBasket();

        var info = new List<string>();
        if (lr.ComponentLost > 0)
            info.Add(string.Format(L.ComponentsSpent, lr.ComponentLost));

        if (lr.IsLinkFinished)
        {
            if (lr.HitPointLost > 0)
                info.Add(string.Format(L.NegativeColumns, lr.HitPointLost));

            string recovery = string.Empty;
            switch (ResolveHpAftermath(out string hpMessage))
            {
                case HpAftermath.Dead:
                    Finish(string.Join(" ", info) + " " + UiMessages.GameLost(L.DiedToLinkBacklash));
                    return;
                case HpAftermath.TimeOut:
                    Finish(string.Join(" ", info) + " " + hpMessage + " " + UiMessages.GameLost(UiMessages.GameLostTime));
                    return;
                case HpAftermath.Unconscious:
                    recovery = " " + hpMessage;
                    break;
            }

            if (lr.HasFailed)
            {
                Finish(string.Join(" ", info) + " " + L.LinkFailedReset + recovery);
                return;
            }

            Finish(string.Join(" ", info) + " " + string.Format(L.LinkMade, lr.LinkBox) + recovery);
            return;
        }

        InfoMessage = string.Join(" ", info);
        SyncCells(6, i => _ls.Connection[i]);
        RerollIfBothUsed();
    }

    private void Finish(string outcome)
    {
        OutcomeText = outcome.Trim();
        IsDone = true;
        CanAutoConnect = false;
        SyncCells(6, i => _ls.Connection[i]);
        Shell.RefreshStatus();
    }

    [RelayCommand]
    private void Back() => Shell.ShowLinks();
}
