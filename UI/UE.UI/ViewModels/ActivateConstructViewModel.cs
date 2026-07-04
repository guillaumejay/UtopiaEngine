using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UE.Core.Architecture;
using UE.Core.Architecture.Messages;
using UE.Core.Entities;
using UE.Core.Interfaces;

namespace UE.UI.ViewModels;

public partial class ActivateConstructViewModel : DicePlacementPageViewModel, IHelpContextProvider
{
    public HelpContext HelpContext => HelpContext.Constructs;

    private readonly ConstructState _cs;

    public string Title { get; }

    [ObservableProperty] private string _infoMessage = string.Empty;
    [ObservableProperty] private string _energyText = "Énergie : 0 / 4";
    [ObservableProperty] private string _tableLabel = "Tentative 1 sur 2";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPlacing))]
    private bool _isDone;

    public bool IsPlacing => !IsDone;

    [ObservableProperty] private string _outcomeText = string.Empty;
    [ObservableProperty] private bool _canUseFocusCharm;

    public ActivateConstructViewModel(IGameEngine engine, MainViewModel shell, ConstructState cs)
        : base(engine, shell)
    {
        _cs = cs;
        Title = $"Activation de {cs.Construct.Name.Text}";
        CanUseFocusCharm = engine.GameState.Inventory.FocusCharmCharged;
        UpdateTableLabel();
        SyncCells(8, i => _cs.CurrentActivationTable[i]);
        Dice.Roll(engine.DiceGenerator);
    }

    private void UpdateTableLabel() =>
        TableLabel = _cs.ActivationTable1.IsFull ? "Tentative 2 sur 2" : "Tentative 1 sur 2";

    [RelayCommand]
    private void UseFocusCharm()
    {
        EngineRef.UseFocusCharm(_cs.Construct.ID);
        CanUseFocusCharm = false;
        InfoMessage = "Charme de concentration : +2 énergie sur cette activation.";
        Shell.RefreshStatus();
    }

    protected override void PlaceDie(DiceCellViewModel cell)
    {
        if (IsDone || !TryTakeDie(cell, out int value, out _))
            return;

        ActivationResult ar = EngineRef.WorkToActivate(_cs, cell.Position, value);
        Shell.RefreshStatus();

        var info = new List<string>();
        if (ar.CurrentColumnValue is 1 or 2)
            info.Add($"Colonne terminée : +{ar.CurrentColumnValue} énergie.");
        if (ar.CurrentColumnValue == 0)
            info.Add("Colonne nulle : elle est effacée, à refaire.");
        if (ar.CurrentColumnValue == -1)
            info.Add("Contrecoup ! Colonne perdue et −1 PV.");
        if (ar.eventOccured)
            info.Add(UiMessages.EventsRolled);
        EnergyText = $"Énergie : {ar.EnergyPoints} / 4";

        switch (ResolveHpAftermath(out string hpMessage))
        {
            case HpAftermath.Dead:
                Finish(UiMessages.GameLost("vous succombez au contrecoup"));
                return;
            case HpAftermath.TimeOut:
                Finish(hpMessage + " " + UiMessages.GameLost(UiMessages.GameLostTime));
                return;
            case HpAftermath.Unconscious:
                info.Add(hpMessage);
                break;
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
            if (EngineRef.IsGameLost)
            {
                Finish(UiMessages.GameLost(UiMessages.GameLostTime));
                return;
            }
            UpdateTableLabel();
        }

        InfoMessage = string.Join(" ", info);
        SyncCells(8, i => _cs.CurrentActivationTable[i]);
        RerollIfBothUsed();
    }

    private void Finish(string outcome)
    {
        OutcomeText = outcome;
        IsDone = true;
        SyncCells(8, i => _cs.CurrentActivationTable[i]);
    }

    [RelayCommand]
    private void Back() => Shell.ShowConstructs();
}
