using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CommunityToolkit.Mvvm.Input;

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
    };

    private readonly MainViewModel _shell;

    public HelpViewModel(MainViewModel shell, HelpContext context = HelpContext.General)
    {
        _shell = shell;
        var all = IsFrench ? FrenchSections() : EnglishSections();
        string[] keys = RelevantKeys[context];
        RelevantSections = keys.Select(k => all.Single(s => s.Key == k)).ToList();
        OtherSections = all.Where(s => !keys.Contains(s.Key)).ToList();
    }

    // Même logique que UE.Core.Tools.Language : FR si la culture est française, sinon anglais.
    public static bool IsFrench => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToUpperInvariant() == "FR";

    public string Title => IsFrench ? "Aide — les mécanismes du jeu" : "Help — game mechanics";

    public string BackLabel => IsFrench ? "Retour au jeu" : "Back to game";

    public string RelevantHeader => IsFrench ? "Pour cet écran" : "For this screen";

    public string OthersHeader => IsFrench ? "Toute l'aide" : "Full help";

    public List<HelpSection> RelevantSections { get; }

    public List<HelpSection> OtherSections { get; }

    public bool HasRelevantSections => RelevantSections.Count > 0;

    [RelayCommand]
    private void Back() => _shell.CloseHelp();

    private static List<HelpSection> FrenchSections() => new()
    {
        new("goal", "But du jeu",
            "Vous êtes le dernier artificier. Retrouvez les six constructs enfouis dans les six régions, " +
            "activez-les, reliez-les entre eux, puis tentez l'activation finale de l'Utopia Engine " +
            "avant la fin du monde. Vous perdez si le temps s'épuise ou si vos points de vie tombent sous zéro."),

        new("time", "Le temps",
            "Presque tout coûte des jours : fouiller une région déjà entamée, se reposer, échouer une activation… " +
            "Vous disposez de 15 jours au départ. Chaque crâne barré (via la God's Hand) prolonge le délai d'un jour, " +
            "jusqu'à 8 crânes. Certains jours sont des jours d'événement : de nouveaux événements sont tirés " +
            "aléatoirement dans les régions et modifient la fouille ou le combat tant qu'ils sont actifs."),

        new("search", "La fouille",
            "Chaque région contient 6 boîtes de recherche de 6 cases (3 en haut, 3 en bas). " +
            "On lance deux dés, et on place chacun dans une case libre ; on relance jusqu'à remplir la boîte. " +
            "Le rang du haut forme un nombre à 3 chiffres, celui du bas aussi : le résultat de la fouille est " +
            "la différence (haut − bas). Tout l'art consiste à placer les gros chiffres au bon endroit !"),

        new("results", "Résultats de fouille",
            "• 0 (zéro parfait) : le construct de la région est trouvé ET activé immédiatement, +5 d'énergie God's Hand.\n" +
            "• 1 à 10 : le construct est trouvé (s'il l'était déjà : 2 composants à la place).\n" +
            "• 11 à 99 : 1 composant de la région.\n" +
            "• Négatif ou 100 et plus : un monstre attaque ! Plus le résultat est extrême, plus le monstre est " +
            "puissant (niveau 1 à 5 : ±100 → niv. 2, ±200 → niv. 3, ±300 → niv. 4, ±400/500 → niv. 5)."),

        new("combat", "Le combat",
            "À chaque tour, on lance deux dés. Chaque dé inférieur ou égal à la valeur d'attaque du monstre " +
            "vous coûte 1 PV ; un dé supérieur ou égal à sa valeur de touche le tue. Un monstre vaincu peut " +
            "laisser du butin : un composant, ou le trésor légendaire de la région pour un monstre de niveau 5. " +
            "À 0 PV, vous tombez inconscient : 6 jours perdus (4 avec la Corne de Guérison), puis PV au maximum. " +
            "Sous 0 PV, vous êtes mort."),

        new("items", "Les objets",
            "Trois objets à usage unique par partie :\n" +
            "• Baguette de sourcier : ajoute ou retranche jusqu'à 100 à un résultat de fouille.\n" +
            "• Baguette de paralysie : +2 aux deux dés pour tout le combat en cours (décidé après avoir vu le tirage).\n" +
            "• Charme de concentration : aide lors de l'activation d'un construct.\n" +
            "Chaque objet encore chargé en fin de partie vaut 10 points."),

        new("godshand", "La God's Hand",
            "Une jauge d'énergie alimentée par les zéros parfaits et certains pouvoirs. " +
            "Dépensez 3 énergies pour barrer un crâne : +1 jour de délai et +5 points."),

        new("constructs", "Constructs, liens et activation finale",
            "Un construct trouvé doit être activé (placement de dés sur sa table d'activation, en dépensant des " +
            "composants). Les constructs activés se relient deux à deux ; six liens sont nécessaires. " +
            "Une fois le réseau complet, l'activation finale décide de la victoire : 2d6 contre la difficulté finale. " +
            "Chaque échec coûte 1 PV et 1 jour.\n" +
            "⚠ Dans cette interface, l'activation, les liens et l'activation finale ne sont pas encore jouables."),

        new("score", "Le score",
            "PV restants + 10 par construct trouvé + 5 par construct activé + 10 par trésor légendaire " +
            "+ 5 par lien + 5 par crâne barré + 20 par zéro parfait non modifié + 10 par objet non utilisé. " +
            "Victoire : +50, plus 5 par jour restant."),
    };

    private static List<HelpSection> EnglishSections() => new()
    {
        new("goal", "Goal",
            "You are the last artificer. Find the six constructs buried in the six regions, activate them, " +
            "link them together, then attempt the final activation of the Utopia Engine before the world ends. " +
            "You lose if time runs out or if your hit points drop below zero."),

        new("time", "Time",
            "Almost everything costs days: searching an already-searched region, resting, failing an activation… " +
            "You start with 15 days. Each crossed skull (via the God's Hand) extends the deadline by one day, " +
            "up to 8 skulls. Some days are event days: new events are rolled randomly across the regions and " +
            "modify searching or fighting while active."),

        new("search", "Searching",
            "Each region holds 6 search boxes of 6 cells (3 top, 3 bottom). Roll two dice and place each one " +
            "in a free cell; reroll until the box is full. The top row forms a 3-digit number, so does the " +
            "bottom row: the search result is the difference (top − bottom). The art is putting the right " +
            "digits in the right places!"),

        new("results", "Search results",
            "• 0 (perfect zero): the region's construct is found AND activated immediately, +5 God's Hand energy.\n" +
            "• 1 to 10: the construct is found (if already found: 2 components instead).\n" +
            "• 11 to 99: 1 component from the region.\n" +
            "• Negative or 100 and above: a monster attacks! The more extreme the result, the stronger the " +
            "monster (level 1 to 5: ±100 → lvl 2, ±200 → lvl 3, ±300 → lvl 4, ±400/500 → lvl 5)."),

        new("combat", "Combat",
            "Each turn, roll two dice. Every die at or below the monster's attack value costs you 1 HP; " +
            "a die at or above its hit value kills it. A defeated monster may drop loot: a component, or the " +
            "region's legendary treasure for a level-5 monster. At 0 HP you fall unconscious: 6 days lost " +
            "(4 with the Healing Horn), then HP restored. Below 0 HP, you are dead."),

        new("items", "Items",
            "Three once-per-game items:\n" +
            "• Dowsing rod: add or subtract up to 100 from a search result.\n" +
            "• Paralysis wand: +2 to both dice for the rest of the current fight (decided after seeing the roll).\n" +
            "• Focus charm: helps when activating a construct.\n" +
            "Each item still charged at game end is worth 10 points."),

        new("godshand", "The God's Hand",
            "An energy gauge fed by perfect zeros and certain powers. Spend 3 energy to cross a skull: " +
            "+1 day on the deadline and +5 points."),

        new("constructs", "Constructs, links and final activation",
            "A found construct must be activated (dice placement on its activation table, spending components). " +
            "Activated constructs are linked in pairs; six links are needed. Once the network is complete, the " +
            "final activation decides the game: 2d6 against the final difficulty. Each failure costs 1 HP and 1 day.\n" +
            "⚠ In this interface, activation, linking and the final activation are not playable yet."),

        new("score", "Scoring",
            "Remaining HP + 10 per found construct + 5 per activated construct + 10 per legendary treasure " +
            "+ 5 per link + 5 per crossed skull + 20 per unmodified perfect zero + 10 per unused item. " +
            "Victory: +50, plus 5 per remaining day."),
    };
}
