using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UE.Core.Architecture;
using UE.Core.Architecture.Messages;
using UE.Core.Entities;
using UE.Core.Interfaces;

namespace UE.UI.ViewModels;

public partial class ActivateConstructViewModel : ViewModelBase, IHelpContextProvider
{
    public HelpContext HelpContext => HelpContext.Constructs;

    private readonly IGameEngine _engine;
    private readonly MainViewModel _shell;
    private readonly ConstructState _cs;

    public string Title { get; }

    public ObservableCollection<DiceCellViewModel> Cells { get; } = new();

    public DicePairViewModel Dice { get; } = new();

    [ObservableProperty] private string _infoMessage = string.Empty;
    [ObservableProperty] private string _energyText = "Énergie : 0 / 4";
    [ObservableProperty] private string _tableLabel = "Tentative 1 sur 2";
    [ObservableProperty] private bool _isPlacing = true;
    [ObservableProperty] private bool _isDone;
    [ObservableProperty] private string _outcomeText = string.Empty;
    [ObservableProperty] private bool _canUseFocusCharm;

    public ActivateConstructViewModel(IGameEngine engine, MainViewModel shell, ConstructState cs)
    {
        _engine = engine;
        _shell = shell;
        _cs = cs;
        Title = $"Activation de {cs.Construct.Name.Text}";
        CanUseFocusCharm = engine.GameState.Inventory.FocusCharmCharged;
        UpdateTableLabel();
        RebuildCells();
        Dice.Roll(engine.DiceGenerator);
    }

    private void UpdateTableLabel() =>
        TableLabel = _cs.ActivationTable1.IsFull ? "Tentative 2 sur 2" : "Tentative 1 sur 2";

    private void RebuildCells()
    {
        Table table = _cs.CurrentActivationTable;
        Cells.Clear();
        for (int i = 0; i < 8; i++)
            Cells.Add(new DiceCellViewModel(i + 1, PlaceDie) { Value = table[i] });
    }

    [RelayCommand]
    private void UseFocusCharm()
    {
        _engine.UseFocusCharm(_cs.Construct.ID);
        CanUseFocusCharm = false;
        InfoMessage = "Charme de concentration : +2 énergie sur cette activation.";
        _shell.RefreshStatus();
    }

    private void PlaceDie(DiceCellViewModel cell)
    {
        if (!IsPlacing || !cell.IsEmpty)
            return;

        int? value = Dice.TakeSelected();
        if (value is null)
            return;

        ActivationResult ar = _engine.WorkToActivate(_cs, cell.Position, value.Value);
        _shell.RefreshStatus();

        var info = new List<string>();
        if (ar.CurrentColumnValue is 1 or 2)
            info.Add($"Colonne terminée : +{ar.CurrentColumnValue} énergie.");
        if (ar.CurrentColumnValue == 0)
            info.Add("Colonne nulle : elle est effacée, à refaire.");
        if (ar.CurrentColumnValue == -1)
            info.Add("Contrecoup ! Colonne perdue et −1 PV.");
        if (ar.eventOccured)
            info.Add("De nouveaux événements se produisent dans les régions !");
        EnergyText = $"Énergie : {ar.EnergyPoints} / 4";

        if (_engine.GameState.CurrentHitPoint < 0)
        {
            Finish("Partie perdue — vous succombez au contrecoup.");
            return;
        }

        if (_engine.GameState.CurrentHitPoint == 0)
        {
            TimePassed t = _engine.RecoverFromUnconsciousness();
            _shell.RefreshStatus();
            info.Add($"Inconscient ! Vous vous réveillez après {t.DaysPassed} jour(s), soigné.");
            if (_engine.IsGameLost)
            {
                Finish("Partie perdue — le temps vous a rattrapé.");
                return;
            }
        }

        if (ar.IsConstructActivated)
        {
            string bonus = ar.IsFieldFilled && ar.EnergyPoints > 4
                ? $" Le surplus ({ar.EnergyPoints - 4}) charge la God's Hand."
                : string.Empty;
            string auto = ar.IsFieldFilled && ar.EnergyPoints < 4
                ? " Les deux tentatives sont épuisées : le construct s'active de justesse, épuisé."
                : string.Empty;
            Finish($"{_cs.Construct.Name.Text} est activé !{bonus}{auto}");
            return;
        }

        if (ar.IsFieldFilled)
        {
            info.Add($"Tentative ratée ({ar.EnergyPoints} énergie sur 4 requises) : +1 jour, on repart sur la seconde table.");
            if (_engine.IsGameLost)
            {
                Finish("Partie perdue — le temps vous a rattrapé.");
                return;
            }
            UpdateTableLabel();
        }

        InfoMessage = string.Join(" ", info);
        RebuildCells();

        if (Dice.BothUsed)
            Dice.Roll(_engine.DiceGenerator);
    }

    private void Finish(string outcome)
    {
        OutcomeText = outcome;
        IsPlacing = false;
        IsDone = true;
        RebuildCells();
    }

    [RelayCommand]
    private void Back() => _shell.ShowConstructs();
}
