using CommunityToolkit.Mvvm.Input;

namespace UE.UI.ViewModels;

public partial class HomeViewModel(MainViewModel shell) : ViewModelBase
{
    [RelayCommand]
    private void NewGame() => shell.StartNewGame();
}
