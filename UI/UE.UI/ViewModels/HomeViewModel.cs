using CommunityToolkit.Mvvm.Input;

namespace UE.UI.ViewModels;

public partial class HomeViewModel(MainViewModel shell) : ViewModelBase
{
    public bool HasAutosave => shell.HasAutosave;

    [RelayCommand]
    private void NewGame() => shell.StartNewGame();

    [RelayCommand]
    private void Continue() => shell.ContinueGame();
}
