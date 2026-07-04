using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UE.Core.Architecture.Messages;
using UE.Core.Entities;
using UE.Core.Interfaces;

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
        Title = $"Relier {ls.Construct1.Construct.Name.Text} et {ls.Construct2.Construct.Name.Text}";
        InfoMessage = "Objectif : des colonnes à écart faible mais jamais négatif — la somme des liens fixe la difficulté de l'activation finale.";
        CanAutoConnect = engine.HasAbility(Ability.AutomaticallyConnect);
        RefreshBasket();
        SyncCells(6, i => _ls.Connection[i]);
        Dice.Roll(engine.DiceGenerator);
    }

    private void RefreshBasket()
    {
        CanUseBasket = !EngineRef.GameState.IsWasteBasketFull;
        BasketLabel = $"Jeter le dé sélectionné (corbeille : {EngineRef.GameState.WasteBasket}/{GameState.WasteBasketCapacity})";
    }

    [RelayCommand]
    private void AutoConnect()
    {
        EngineRef.UseAutomaticConnect(_ls);
        Finish($"Lien établi automatiquement grâce aux Textes Anciens (valeur {_ls.LinkBox}).");
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
            info.Add($"{lr.ComponentLost} composant(s) dépensé(s).");

        if (lr.IsLinkFinished)
        {
            if (lr.HitPointLost > 0)
                info.Add($"Colonnes négatives : −{lr.HitPointLost} PV.");

            string recovery = string.Empty;
            switch (ResolveHpAftermath(out string hpMessage))
            {
                case HpAftermath.Dead:
                    Finish(string.Join(" ", info) + " " + UiMessages.GameLost("vous succombez au contrecoup du lien"));
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
                Finish(string.Join(" ", info) + " Plus de composants pour compenser : le lien est réinitialisé, tout est à refaire." + recovery);
                return;
            }

            Finish(string.Join(" ", info) + $" Lien établi (valeur {lr.LinkBox}) !" + recovery);
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
