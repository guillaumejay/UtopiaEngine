using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using UE.UI.Localization;

namespace UE.UI.ViewModels;

public record HelpSection(string Key, string Title, string Body);

/// <summary>Contexte de jeu au moment où l'aide est ouverte, pour mettre en avant les bonnes sections.</summary>
public enum HelpContext
{
    General,
    Regions,
    Placing,
    SearchResult,
    Combat,
    AfterSearch,
    Constructs,
    Links,
    Final,
    Camp,
}

/// <summary>À implémenter par les pages qui savent quel contexte d'aide les concerne.</summary>
public interface IHelpContextProvider
{
    HelpContext HelpContext { get; }
}

public partial class HelpViewModel : ViewModelBase
{
    private static readonly Dictionary<HelpContext, string[]> RelevantKeys = new()
    {
        [HelpContext.General] = [],
        [HelpContext.Regions] = ["goal", "search", "time"],
        [HelpContext.Placing] = ["search", "results"],
        [HelpContext.SearchResult] = ["results", "items"],
        [HelpContext.Combat] = ["combat", "items"],
        [HelpContext.AfterSearch] = ["results", "time"],
        [HelpContext.Constructs] = ["constructs", "godshand", "items"],
        [HelpContext.Links] = ["constructs", "score"],
        [HelpContext.Final] = ["constructs", "godshand", "score"],
        [HelpContext.Camp] = ["godshand", "time", "items"],
    };

    private readonly MainViewModel _shell;

    public HelpViewModel(MainViewModel shell, HelpContext context = HelpContext.General)
    {
        _shell = shell;
        var all = AllSections();
        string[] keys = RelevantKeys[context];
        RelevantSections = keys.Select(k => all.Single(s => s.Key == k)).ToList();
        OtherSections = all.Where(s => !keys.Contains(s.Key)).ToList();
    }

    public string Title => L.HelpTitle;

    public string BackLabel => L.HelpBack;

    public string RelevantHeader => L.HelpForThisScreen;

    public string OthersHeader => L.HelpFullHelp;

    public List<HelpSection> RelevantSections { get; }

    public List<HelpSection> OtherSections { get; }

    public bool HasRelevantSections => RelevantSections.Count > 0;

    [RelayCommand]
    private void Back() => _shell.CloseHelp();

    private static List<HelpSection> AllSections() => new()
    {
        new("goal", L.HelpGoalTitle, L.HelpGoalBody),
        new("time", L.HelpTimeTitle, L.HelpTimeBody),
        new("search", L.HelpSearchTitle, L.HelpSearchBody),
        new("results", L.HelpResultsTitle, L.HelpResultsBody),
        new("combat", L.HelpCombatTitle, L.HelpCombatBody),
        new("items", L.HelpItemsTitle, L.HelpItemsBody),
        new("godshand", L.HelpGodshandTitle, L.HelpGodshandBody),
        new("constructs", L.HelpConstructsTitle, L.HelpConstructsBody),
        new("score", L.HelpScoreTitle, L.HelpScoreBody),
    };
}
