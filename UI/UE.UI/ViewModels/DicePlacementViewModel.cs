using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UE.Core;
using UE.Core.Architecture.Messages;
using UE.Core.Interfaces;

namespace UE.UI.ViewModels;

/// <summary>Messages joueur partagés entre les écrans, adossés aux ressources localisées.</summary>
public static class UiMessages
{
    public static string EventsRolled => Localization.L.EventsRolled;

    public static string GameLostTime => Localization.L.GameLostTime;

    public static string Unconscious(int days) => string.Format(Localization.L.UnconsciousMsg, days);

    public static string GameLost(string reason) => string.Format(Localization.L.GameLostFmt, reason);
}

/// <summary>Issue de la résolution des PV après une action qui peut en coûter.</summary>
public enum HpAftermath
{
    Fine,
    Unconscious,
    Dead,
    TimeOut,
}

/// <summary>
/// Socle des pages qui placent une paire de dés sur une grille : cellules, dés,
/// protocole de prise de dé et résolution inconscience/mort partagés.
/// </summary>
public abstract partial class DicePlacementPageViewModel : ViewModelBase
{
    protected readonly IGameEngine EngineRef;
    protected readonly MainViewModel Shell;

    protected DicePlacementPageViewModel(IGameEngine engine, MainViewModel shell)
    {
        EngineRef = engine;
        Shell = shell;
    }

    public ObservableCollection<DiceCellViewModel> Cells { get; } = new();

    public DicePairViewModel Dice { get; } = new();

    /// <summary>Aligne les cellules sur l'état du moteur, en place quand c'est possible.</summary>
    protected void SyncCells(int count, Func<int, string> valueAt)
    {
        if (Cells.Count != count)
        {
            Cells.Clear();
            for (int i = 0; i < count; i++)
                Cells.Add(new DiceCellViewModel(i + 1, PlaceDie) { Value = valueAt(i) });
            return;
        }
        for (int i = 0; i < count; i++)
            Cells[i].Value = valueAt(i);
    }

    /// <summary>Prend le dé sélectionné pour une case vide ; isFirst = premier dé du lancer.</summary>
    protected bool TryTakeDie(DiceCellViewModel? cell, out int value, out bool isFirst)
    {
        value = 0;
        isFirst = !Dice.Die1Used && !Dice.Die2Used;
        if (cell is { IsEmpty: false })
            return false;
        int? taken = Dice.TakeSelected();
        if (taken is null)
            return false;
        value = taken.Value;
        return true;
    }

    protected void RerollIfBothUsed()
    {
        if (Dice.BothUsed)
            Dice.Roll(EngineRef.DiceGenerator);
    }

    /// <summary>
    /// Résout l'état des PV après une action : mort sous 0, inconscience à 0
    /// (récupération moteur + message), défaite au temps après récupération.
    /// </summary>
    protected HpAftermath ResolveHpAftermath(out string message)
    {
        message = string.Empty;
        if (EngineRef.GameState.CurrentHitPoint < 0)
            return HpAftermath.Dead;
        if (EngineRef.GameState.CurrentHitPoint == 0)
        {
            TimePassed t = EngineRef.RecoverFromUnconsciousness();
            Shell.RefreshStatus();
            message = UiMessages.Unconscious(t.DaysPassed);
            return EngineRef.IsGameLost ? HpAftermath.TimeOut : HpAftermath.Unconscious;
        }
        return HpAftermath.Fine;
    }

    protected abstract void PlaceDie(DiceCellViewModel cell);
}

/// <summary>Une case cliquable d'une grille de placement (fouille, activation, lien…).</summary>
public partial class DiceCellViewModel(int position, Action<DiceCellViewModel> place) : ObservableObject
{
    public int Position { get; } = position;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEmpty))]
    [NotifyPropertyChangedFor(nameof(Display))]
    private string _value = string.Empty;

    public bool IsEmpty => string.IsNullOrEmpty(Value);

    public string Display => IsEmpty ? "·" : Value;

    [RelayCommand]
    private void Place() => place(this);
}

/// <summary>La paire de dés en cours : valeurs, dé sélectionné, dés déjà posés.</summary>
public partial class DicePairViewModel : ObservableObject
{
    [ObservableProperty] private int _die1;
    [ObservableProperty] private int _die2;
    [ObservableProperty] private bool _die1Used;
    [ObservableProperty] private bool _die2Used;
    [ObservableProperty] private bool _isDie1Selected;
    [ObservableProperty] private bool _isDie2Selected;

    public bool BothUsed => Die1Used && Die2Used;

    public void Roll(IDiceRoller dice)
    {
        TwoDice td = dice.Get2d6();
        Die1 = td.First;
        Die2 = td.Second;
        Die1Used = false;
        Die2Used = false;
        IsDie1Selected = true;
        IsDie2Selected = false;
    }

    /// <summary>Consomme le dé sélectionné et sélectionne l'autre ; null si aucun dé jouable.</summary>
    public int? TakeSelected()
    {
        if (IsDie1Selected && !Die1Used)
        {
            Die1Used = true;
            IsDie1Selected = false;
            IsDie2Selected = !Die2Used;
            return Die1;
        }
        if (IsDie2Selected && !Die2Used)
        {
            Die2Used = true;
            IsDie2Selected = false;
            IsDie1Selected = !Die1Used;
            return Die2;
        }
        return null;
    }
}
