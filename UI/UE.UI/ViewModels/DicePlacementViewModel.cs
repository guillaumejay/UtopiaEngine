using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UE.Core;
using UE.Core.Interfaces;

namespace UE.UI.ViewModels;

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
