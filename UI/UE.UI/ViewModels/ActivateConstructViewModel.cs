using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UE.Core.Architecture;
using UE.Core.Architecture.Messages;
using UE.Core.Entities;
using UE.Core.Interfaces;
using UE.UI.Localization;

namespace UE.UI.ViewModels;

public partial class ActivateConstructViewModel : DicePlacementPageViewModel, IHelpContextProvider
{
    public HelpContext HelpContext => HelpContext.Constructs;

    private readonly ConstructState _cs;

    public string Title { get; }

    [ObservableProperty] private string _infoMessage = string.Empty;
    [ObservableProperty] private string _energyText = string.Format(L.EnergyLabel, 0);
    [ObservableProperty] private string _tableLabel = L.Attempt1;

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
        Title = string.Format(L.ActivationTitle, cs.Construct.Name.Text);
        CanUseFocusCharm = engine.GameState.Inventory.FocusCharmCharged;
        UpdateTableLabel();
        SyncCells(8, i => _cs.CurrentActivationTable[i]);
        Dice.Roll(engine.DiceGenerator);
    }

    private void UpdateTableLabel() =>
        TableLabel = _cs.ActivationTable1.IsFull ? L.Attempt2 : L.Attempt1;

    [RelayCommand]
    private void UseFocusCharm()
    {
        EngineRef.UseFocusCharm(_cs.Construct.ID);
        CanUseFocusCharm = false;
        InfoMessage = L.FocusCharmUsed;
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
            info.Add(string.Format(L.ColumnEnergy, ar.CurrentColumnValue));
        if (ar.CurrentColumnValue == 0)
            info.Add(L.ColumnZero);
        if (ar.CurrentColumnValue == -1)
            info.Add(L.ColumnBacklash);
        if (ar.eventOccured)
            info.Add(UiMessages.EventsRolled);
        EnergyText = string.Format(L.EnergyLabel, ar.EnergyPoints);

        switch (ResolveHpAftermath(out string hpMessage))
        {
            case HpAftermath.Dead:
                Finish(UiMessages.GameLost(L.DiedToBacklash));
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
                ? string.Format(L.SurplusToGodsHand, ar.EnergyPoints - 4)
                : string.Empty;
            string auto = ar.IsFieldFilled && ar.EnergyPoints < 4
                ? L.ExhaustedActivation
                : string.Empty;
            Finish(string.Format(L.ConstructActivated, _cs.Construct.Name.Text) + bonus + auto);
            return;
        }

        if (ar.IsFieldFilled)
        {
            info.Add(string.Format(L.AttemptFailed, ar.EnergyPoints));
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
