using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UE.Core.Architecture.Messages;
using UE.Core.Entities;
using UE.Core.Interfaces;

namespace UE.UI.ViewModels;

public partial class ConnectLinkViewModel : ViewModelBase, IHelpContextProvider
{
    public HelpContext HelpContext => HelpContext.Links;

    private readonly IGameEngine _engine;
    private readonly MainViewModel _shell;
    private readonly LinkState _ls;

    public string Title { get; }

    public ObservableCollection<DiceCellViewModel> Cells { get; } = new();

    public DicePairViewModel Dice { get; } = new();

    [ObservableProperty] private string _infoMessage = string.Empty;
    [ObservableProperty] private bool _isPlacing = true;
    [ObservableProperty] private bool _isDone;
    [ObservableProperty] private string _outcomeText = string.Empty;
    [ObservableProperty] private string _basketLabel = string.Empty;
    [ObservableProperty] private bool _canUseBasket;
    [ObservableProperty] private bool _canAutoConnect;

    public ConnectLinkViewModel(IGameEngine engine, MainViewModel shell, LinkState ls)
    {
        _engine = engine;
        _shell = shell;
        _ls = ls;
        Title = $"Relier {ls.Construct1.Construct.Name.Text} et {ls.Construct2.Construct.Name.Text}";
        InfoMessage = "Objectif : des colonnes à écart faible mais jamais négatif — la somme des liens fixe la difficulté de l'activation finale.";
        CanAutoConnect = engine.HasAbility(Ability.AutomaticallyConnect);
        RefreshBasket();
        RebuildCells();
        Dice.Roll(engine.DiceGenerator);
    }

    private void RefreshBasket()
    {
        CanUseBasket = !_engine.GameState.IsWasteBasketFull;
        BasketLabel = $"Jeter le dé sélectionné (corbeille : {_engine.GameState.WasteBasket}/10)";
    }

    private void RebuildCells()
    {
        Cells.Clear();
        for (int i = 0; i < 6; i++)
            Cells.Add(new DiceCellViewModel(i + 1, PlaceDie) { Value = _ls.Connection[i] });
    }

    [RelayCommand]
    private void AutoConnect()
    {
        _engine.UseAutomaticConnect(_ls);
        _shell.RefreshStatus();
        Finish($"Lien établi automatiquement grâce aux Textes Anciens (valeur {_ls.LinkBox}).");
    }

    [RelayCommand]
    private void Discard()
    {
        if (!IsPlacing || _engine.GameState.IsWasteBasketFull)
            return;
        bool isFirst = !Dice.Die1Used && !Dice.Die2Used;
        int? value = Dice.TakeSelected();
        if (value is null)
            return;
        _engine.WorkToLink(_ls.ID, 0, value.Value, isFirst);
        RefreshBasket();
        if (Dice.BothUsed)
            Dice.Roll(_engine.DiceGenerator);
    }

    private void PlaceDie(DiceCellViewModel cell)
    {
        if (!IsPlacing || !cell.IsEmpty)
            return;

        bool isFirst = !Dice.Die1Used && !Dice.Die2Used;
        int? value = Dice.TakeSelected();
        if (value is null)
            return;

        LinkResult lr = _engine.WorkToLink(_ls.ID, cell.Position, value.Value, isFirst);
        _shell.RefreshStatus();
        RefreshBasket();

        var info = new List<string>();
        if (lr.ComponentLost > 0)
            info.Add($"{lr.ComponentLost} composant(s) dépensé(s).");

        if (lr.IsLinkFinished)
        {
            if (lr.HitPointLost > 0)
                info.Add($"Colonnes négatives : −{lr.HitPointLost} PV.");

            if (_engine.GameState.CurrentHitPoint < 0)
            {
                Finish(string.Join(" ", info) + " Partie perdue — vous succombez au contrecoup du lien.");
                return;
            }

            string recovery = string.Empty;
            if (_engine.GameState.CurrentHitPoint == 0)
            {
                TimePassed t = _engine.RecoverFromUnconsciousness();
                _shell.RefreshStatus();
                recovery = $" Inconscient ! Vous vous réveillez après {t.DaysPassed} jour(s), soigné.";
                if (_engine.IsGameLost)
                {
                    Finish(string.Join(" ", info) + recovery + " Partie perdue — le temps vous a rattrapé.");
                    return;
                }
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
        RebuildCells();

        if (Dice.BothUsed)
            Dice.Roll(_engine.DiceGenerator);
    }

    private void Finish(string outcome)
    {
        OutcomeText = outcome.Trim();
        IsPlacing = false;
        IsDone = true;
        CanAutoConnect = false;
        RebuildCells();
        _shell.RefreshStatus();
    }

    [RelayCommand]
    private void Back() => _shell.ShowLinks();
}
