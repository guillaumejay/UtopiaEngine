using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UE.UI.Localization;

namespace UE.UI.ViewModels;

/// <summary>Langue proposée dans le sélecteur ; le nom s'affiche dans sa propre langue.</summary>
public sealed record LanguageOption(string Code, string DisplayName)
{
    public override string ToString() => DisplayName;
}

public partial class HomeViewModel : ViewModelBase
{
    private readonly MainViewModel _shell;

    [ObservableProperty]
    private LanguageOption _selectedLanguage;

    public HomeViewModel(MainViewModel shell)
    {
        _shell = shell;
        _selectedLanguage = Languages.First(l => l.Code == LanguageManager.Current);
    }

    public List<LanguageOption> Languages { get; } =
    [
        new("en", "English"),
        new("fr", "Français"),
    ];

    public bool HasAutosave => _shell.HasAutosave;

    partial void OnSelectedLanguageChanged(LanguageOption value)
    {
        if (value.Code == LanguageManager.Current)
            return;
        LanguageManager.Set(value.Code);
        _shell.OnLanguageChanged();
    }

    [RelayCommand]
    private void NewGame() => _shell.StartNewGame();

    [RelayCommand]
    private void Continue() => _shell.ContinueGame();
}
