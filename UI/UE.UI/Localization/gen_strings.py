# Source unique des chaînes UI : génère Strings.resx (EN neutre), Strings.fr.resx et L.cs.
# Usage : python gen_strings.py
import html, io, os

S = [
    # (Key, EN, FR)
    ("Help", "Help", "Aide"),
    ("StatusHp", "HP: {0}", "PV : {0}"),
    ("StatusDay", "Day: {0}", "Jour : {0}"),
    ("StatusDaysRemaining", "Days left: {0}", "Jours restants : {0}"),
    ("StatusScore", "Score: {0}", "Score : {0}"),
    ("StatusGodsHand", "GH energy: {0}", "Énergie GH : {0}"),

    ("HomeTagline", "Rebuild the Utopia Engine before the world ends.", "Reconstruisez l'Utopia Engine avant la fin du monde."),
    ("NewGame", "New game", "Nouvelle partie"),
    ("ContinueSaved", "Continue the saved game", "Continuer la partie sauvegardée"),

    ("Camp", "Camp", "Campement"),
    ("SearchAction", "Search", "Fouiller"),
    ("ConstructsToActivate", "Constructs ({0} to activate)", "Constructs ({0} à activer)"),
    ("ConstructsFoundLabel", "Constructs ({0} found)", "Constructs ({0} trouvé(s))"),
    ("LinksPossibleLabel", "Links ({0} possible)", "Liens ({0} possible(s))"),
    ("LinksConnectedLabel", "Links ({0} connected)", "Liens ({0} connecté(s))"),
    ("ComponentInfo", "Component: {0} ({1})", "Composant : {0} ({1})"),
    ("ConstructInfo", "Construct: {0} ({1})", "Construct : {0} ({1})"),
    ("TreasureInfo", "Treasure: {0} ({1})", "Trésor : {0} ({1})"),
    ("FoundState", "found", "trouvé"),
    ("ToFindState", "to find", "à trouver"),
    ("EventsPrefix", "Events: {0}", "Événements : {0}"),

    ("SearchingFor", "{0} — hunting for {1}", "{0} — à la recherche de {1}"),
    ("SearchCostsDays", "This search costs {0} day(s).", "Cette fouille coûte {0} jour(s)."),
    ("RegionExhausted", "Region fully searched.", "Région entièrement fouillée."),
    ("SearchResultLabel", "Search result: {0}", "Résultat de fouille : {0}"),
    ("SearchResultModified", "Search result: {0} (modified by the rod)", "Résultat de fouille : {0} (modifié par la baguette)"),
    ("UseDowsingRod", "Use the dowsing rod", "Utiliser la baguette de sourcier"),
    ("ApplySearch", "Apply the search", "Appliquer la fouille"),
    ("SearchAgain", "Search again", "Fouiller à nouveau"),
    ("BackToRegions", "Back to regions", "Retour aux régions"),
    ("NothingFound", "Nothing found this time.", "Rien trouvé cette fois-ci."),
    ("ConstructFoundPerfect", "{0} found and activated (perfect zero, +5 God's Hand energy)!", "{0} trouvé et activé (zéro parfait, +5 énergie God's Hand) !"),
    ("ConstructFoundMsg", "{0} found!", "{0} trouvé !"),
    ("ComponentsFoundMsg", "{0} × {1} found.", "{0} × {1} trouvé(s)."),
    ("DiceLabel", "Dice:", "Dés :"),
    ("DiceHint", "(pick a die, then a cell)", "(choisissez un dé, puis une case)"),

    ("MustFight", "You must fight {0} (attack on {1}, hit on {2}{3}).", "Vous devez combattre {0} (attaque sur {1}, touché sur {2}{3})."),
    ("SpiritSuffix", ", spirit", ", esprit"),
    ("YouRoll", "You roll {0} and {1}.", "Vous lancez {0} et {1}."),
    ("RollCombatDice", "Roll the combat dice", "Lancer les dés de combat"),
    ("Flee", "Flee", "Fuir"),
    ("YouFlee", "You flee the fight.", "Vous fuyez le combat."),
    ("UseWandButton", "Paralysis wand (+{0})", "Baguette de paralysie (+{0})"),
    ("SkipWand", "Continue without the wand", "Continuer sans baguette"),
    ("WandApplied", "Paralysis wand: +{0} to both dice for the whole fight, now {1} and {2}.", "Baguette de paralysie : +{0} aux dés pour tout le combat, soit {1} et {2}."),
    ("YouLoseHp", "You lose {0} HP.", "Vous perdez {0} PV."),
    ("EncounterDefeated", "{0} is defeated!", "{0} est vaincu !"),
    ("LootComponent", "Loot: 1 × {0}.", "Butin : 1 × {0}."),
    ("LootTreasure", "Loot: {0} (legendary treasure)!", "Butin : {0} (trésor légendaire) !"),
    ("DiedToWounds", "you succumb to your wounds", "vous succombez à vos blessures"),

    ("EventsRolled", "New events unfold across the regions!", "De nouveaux événements se produisent dans les régions !"),
    ("GameLostTime", "time has caught up with you", "le temps vous a rattrapé"),
    ("UnconsciousMsg", "Unconscious! You wake up after {0} day(s), healed.", "Inconscient ! Vous vous réveillez après {0} jour(s), soigné."),
    ("GameLostFmt", "Game lost — {0}.", "Partie perdue — {0}."),

    ("ActivationTitle", "Activating {0}", "Activation de {0}"),
    ("Attempt1", "Attempt 1 of 2", "Tentative 1 sur 2"),
    ("Attempt2", "Attempt 2 of 2", "Tentative 2 sur 2"),
    ("EnergyLabel", "Energy: {0} / 4", "Énergie : {0} / 4"),
    ("ColumnEnergy", "Column complete: +{0} energy.", "Colonne terminée : +{0} énergie."),
    ("ColumnZero", "Zero column: cleared, redo it.", "Colonne nulle : elle est effacée, à refaire."),
    ("ColumnBacklash", "Backlash! Column lost and −1 HP.", "Contrecoup ! Colonne perdue et −1 PV."),
    ("FocusCharmUsed", "Focus charm: +2 energy on this activation.", "Charme de concentration : +2 énergie sur cette activation."),
    ("UseFocusCharm", "Focus charm (+2 energy)", "Charme de concentration (+2 énergie)"),
    ("ConstructActivated", "{0} is activated!", "{0} est activé !"),
    ("SurplusToGodsHand", " The surplus ({0}) charges the God's Hand.", " Le surplus ({0}) charge la God's Hand."),
    ("ExhaustedActivation", " Both attempts spent: the construct activates anyway, exhausted.", " Les deux tentatives sont épuisées : le construct s'active de justesse, épuisé."),
    ("AttemptFailed", "Attempt failed ({0} energy of 4 required): +1 day, moving on to the second table.", "Tentative ratée ({0} énergie sur 4 requises) : +1 jour, on repart sur la seconde table."),
    ("DiedToBacklash", "you succumb to the backlash", "vous succombez au contrecoup"),
    ("BackToConstructs", "Back to constructs", "Retour aux constructs"),

    ("ConstructsTitle", "Constructs", "Constructs"),
    ("NotFoundState", "Not found", "Non trouvé"),
    ("ActivatedState", "Activated ✓", "Activé ✓"),
    ("FoundToActivateState", "Found, to activate", "Trouvé, à activer"),
    ("ActivateAction", "Activate", "Activer"),

    ("LinksTitle", "Construct links", "Liens entre constructs"),
    ("LinksSummary", "{0} link(s) of {1} — current final activation difficulty: {2}", "{0} lien(s) sur {1} — difficulté d'activation finale actuelle : {2}"),
    ("LinkConnected", "Connected (value {0})", "Connecté (valeur {0})"),
    ("LinkNeedsActivated", "Unavailable — both constructs must be activated", "Indisponible — les deux constructs doivent être activés"),
    ("LinkNoComponent", "Unavailable — no {0} component left", "Indisponible — aucun composant {0}"),
    ("LinkReady", "Ready to link (cost: 1 × {0})", "Prêt à relier (coût : 1 × {0})"),
    ("ConnectAction", "Link", "Relier"),
    ("FinalActivationButton", "⚡ Final activation!", "⚡ Activation finale !"),
    ("ConnectTitle", "Linking {0} and {1}", "Relier {0} et {1}"),
    ("ConnectHint", "Goal: low-difference columns, never negative — the sum of the link values sets the final activation difficulty.", "Objectif : des colonnes à écart faible mais jamais négatif — la somme des liens fixe la difficulté de l'activation finale."),
    ("BasketLabelFmt", "Discard the selected die (basket: {0}/{1})", "Jeter le dé sélectionné (corbeille : {0}/{1})"),
    ("AutoConnectButton", "Ancient Texts: connect automatically (value 2)", "Textes Anciens : connecter automatiquement (valeur 2)"),
    ("AutoConnected", "Link made automatically thanks to the Ancient Texts (value {0}).", "Lien établi automatiquement grâce aux Textes Anciens (valeur {0})."),
    ("ComponentsSpent", "{0} component(s) spent.", "{0} composant(s) dépensé(s)."),
    ("NegativeColumns", "Negative columns: −{0} HP.", "Colonnes négatives : −{0} PV."),
    ("LinkFailedReset", "No components left to compensate: the link is reset, start over.", "Plus de composants pour compenser : le lien est réinitialisé, tout est à refaire."),
    ("LinkMade", "Link made (value {0})!", "Lien établi (valeur {0}) !"),
    ("DiedToLinkBacklash", "you succumb to the link's backlash", "vous succombez au contrecoup du lien"),
    ("BackToLinks", "Back to links", "Retour aux liens"),

    ("FinalTitle", "Final activation", "Activation finale"),
    ("FinalIntro", "The six constructs are linked: the Utopia Engine is ready. Roll 2d6 — reach or exceed the difficulty to save the world. Each failure costs 1 HP and 1 day.", "Les six constructs sont reliés : l'Utopia Engine est prêt. Lancez 2d6 — atteignez ou dépassez la difficulté pour sauver le monde. Chaque échec coûte 1 PV et 1 jour."),
    ("FinalDifficulty", "Difficulty: {0} (2d6, sum to reach)", "Difficulté : {0} (2d6, somme à atteindre)"),
    ("SacrificeHpButton", "Sacrifice HP (−1 difficulty per HP)", "Sacrifier des PV (−1 difficulté par PV)"),
    ("SacrificeDone", "You channel your life force: −{0} HP, difficulty reduced by as much. There will be no second sacrifice.", "Vous canalisez votre énergie vitale : −{0} PV, difficulté réduite d'autant. Il n'y aura pas de second sacrifice."),
    ("FinalRollButton", "Attempt the activation (2d6)", "Tenter l'activation (2d6)"),
    ("FinalRollLine", "You roll {0} and {1}: {2}.", "Vous lancez {0} et {1} : {2}."),
    ("FinalFail", "The Engine rumbles but does not start: −1 HP, +1 day.", "L'Engine gronde mais ne démarre pas : −1 PV, +1 jour."),
    ("Victory", "THE UTOPIA ENGINE AWAKENS — the world is saved!", "L'UTOPIA ENGINE S'ÉVEILLE — le monde est sauvé !"),
    ("FinalScore", "Final score: {0}.", "Score final : {0}."),
    ("LifeExhausted", "your life force is spent", "votre énergie vitale est épuisée"),

    ("SkullsFmt", "Skulls crossed: {0}/{1} (each skull = +1 day of deadline)", "Crânes barrés : {0}/{1} (chaque crâne = +1 jour de délai)"),
    ("GodsHandFmt", "God's Hand energy: {0}/{1}", "Énergie God's Hand : {0}/{1}"),
    ("BasketFmt", "Waste basket: {0}/{1} dice discarded", "Corbeille : {0}/{1} dés jetés"),
    ("CrossSkullButton", "Cross a skull (3 energy, +1 day)", "Barrer un crâne (3 énergies, +1 jour)"),
    ("SkullCrossed", "The God's Hand strikes: a skull is crossed, +1 day of deadline.", "La God's Hand frappe : un crâne est barré, +1 jour de délai."),
    ("RestHint", "Rest: recover 1 HP per day spent.", "Repos : 1 PV récupéré par jour passé."),
    ("RestButton", "Rest", "Se reposer"),
    ("Rested", "You rest {0} day(s) — HP: {1}.", "Vous vous reposez {0} jour(s) — PV : {1}."),
    ("ComponentsHeader", "Components", "Composants"),
    ("ItemsHeader", "Items", "Objets"),
    ("TreasuresHeader", "Legendary treasures", "Trésors légendaires"),
    ("NoTreasures", "No legendary treasure yet.", "Aucun trésor légendaire pour l'instant."),
    ("ChargedState", "charged", "chargée"),
    ("UsedState", "used", "utilisée"),
    ("DowsingRodName", "Dowsing rod", "Baguette de sourcier"),
    ("ParalysisWandName", "Paralysis wand", "Baguette de paralysie"),
    ("FocusCharmName", "Focus charm", "Charme de concentration"),

    ("HelpTitle", "Help — game mechanics", "Aide — les mécanismes du jeu"),
    ("HelpBack", "Back to game", "Retour au jeu"),
    ("HelpForThisScreen", "For this screen", "Pour cet écran"),
    ("HelpFullHelp", "Full help", "Toute l'aide"),

    ("HelpGoalTitle", "Goal", "But du jeu"),
    ("HelpGoalBody",
     "You are the last artificer. Find the six constructs buried in the six regions, activate them, link them together, then attempt the final activation of the Utopia Engine before the world ends. You lose if time runs out or if your hit points drop below zero.",
     "Vous êtes le dernier artificier. Retrouvez les six constructs enfouis dans les six régions, activez-les, reliez-les entre eux, puis tentez l'activation finale de l'Utopia Engine avant la fin du monde. Vous perdez si le temps s'épuise ou si vos points de vie tombent sous zéro."),
    ("HelpTimeTitle", "Time", "Le temps"),
    ("HelpTimeBody",
     "Almost everything costs days: searching an already-searched region, resting, failing an activation… You start with 15 days. Each crossed skull (via the God's Hand) extends the deadline by one day, up to 8 skulls. Some days are event days: new events are rolled randomly across the regions and modify searching or fighting while active.",
     "Presque tout coûte des jours : fouiller une région déjà entamée, se reposer, échouer une activation… Vous disposez de 15 jours au départ. Chaque crâne barré (via la God's Hand) prolonge le délai d'un jour, jusqu'à 8 crânes. Certains jours sont des jours d'événement : de nouveaux événements sont tirés aléatoirement dans les régions et modifient la fouille ou le combat tant qu'ils sont actifs."),
    ("HelpSearchTitle", "Searching", "La fouille"),
    ("HelpSearchBody",
     "Each region holds 6 search boxes of 6 cells (3 top, 3 bottom). Roll two dice and place each one in a free cell; reroll until the box is full. The top row forms a 3-digit number, so does the bottom row: the search result is the difference (top − bottom). The art is putting the right digits in the right places!",
     "Chaque région contient 6 boîtes de recherche de 6 cases (3 en haut, 3 en bas). On lance deux dés, et on place chacun dans une case libre ; on relance jusqu'à remplir la boîte. Le rang du haut forme un nombre à 3 chiffres, celui du bas aussi : le résultat de la fouille est la différence (haut − bas). Tout l'art consiste à placer les gros chiffres au bon endroit !"),
    ("HelpResultsTitle", "Search results", "Résultats de fouille"),
    ("HelpResultsBody",
     "• 0 (perfect zero): the region's construct is found AND activated immediately, +5 God's Hand energy.\n• 1 to 10: the construct is found (if already found: 2 components instead).\n• 11 to 99: 1 component from the region.\n• Negative or 100 and above: a monster attacks! The more extreme the result, the stronger the monster (level 1 to 5: ±100 → lvl 2, ±200 → lvl 3, ±300 → lvl 4, ±400/500 → lvl 5).",
     "• 0 (zéro parfait) : le construct de la région est trouvé ET activé immédiatement, +5 d'énergie God's Hand.\n• 1 à 10 : le construct est trouvé (s'il l'était déjà : 2 composants à la place).\n• 11 à 99 : 1 composant de la région.\n• Négatif ou 100 et plus : un monstre attaque ! Plus le résultat est extrême, plus le monstre est puissant (niveau 1 à 5 : ±100 → niv. 2, ±200 → niv. 3, ±300 → niv. 4, ±400/500 → niv. 5)."),
    ("HelpCombatTitle", "Combat", "Le combat"),
    ("HelpCombatBody",
     "Each turn, roll two dice. Every die at or below the monster's attack value costs you 1 HP; a die at or above its hit value kills it. A defeated monster may drop loot: a component, or the region's legendary treasure for a level-5 monster. At 0 HP you fall unconscious: 6 days lost (4 with the Healing Horn), then HP restored. Below 0 HP, you are dead.",
     "À chaque tour, on lance deux dés. Chaque dé inférieur ou égal à la valeur d'attaque du monstre vous coûte 1 PV ; un dé supérieur ou égal à sa valeur de touche le tue. Un monstre vaincu peut laisser du butin : un composant, ou le trésor légendaire de la région pour un monstre de niveau 5. À 0 PV, vous tombez inconscient : 6 jours perdus (4 avec la Corne de Guérison), puis PV au maximum. Sous 0 PV, vous êtes mort."),
    ("HelpItemsTitle", "Items", "Les objets"),
    ("HelpItemsBody",
     "Three once-per-game items:\n• Dowsing rod: add or subtract up to 100 from a search result.\n• Paralysis wand: +2 to both dice for the rest of the current fight (decided after seeing the roll).\n• Focus charm: helps when activating a construct.\nEach item still charged at game end is worth 10 points.",
     "Trois objets à usage unique par partie :\n• Baguette de sourcier : ajoute ou retranche jusqu'à 100 à un résultat de fouille.\n• Baguette de paralysie : +2 aux deux dés pour tout le combat en cours (décidé après avoir vu le tirage).\n• Charme de concentration : aide lors de l'activation d'un construct.\nChaque objet encore chargé en fin de partie vaut 10 points."),
    ("HelpGodshandTitle", "The God's Hand", "La God's Hand"),
    ("HelpGodshandBody",
     "An energy gauge fed by perfect zeros and certain powers. Spend 3 energy to cross a skull: +1 day on the deadline and +5 points.",
     "Une jauge d'énergie alimentée par les zéros parfaits et certains pouvoirs. Dépensez 3 énergies pour barrer un crâne : +1 jour de délai et +5 points."),
    ("HelpConstructsTitle", "Constructs, links and final activation", "Constructs, liens et activation finale"),
    ("HelpConstructsBody",
     "A found construct must be activated: place dice on its 4-column table. A completed column whose difference (top − bottom) is ±5 yields 2 energy, ±4 yields 1, 0 clears the column (redo it), any other difference locks it and costs 1 HP. You need 4 energy to activate; any surplus charges the God's Hand. A failed table costs 1 day and grants a second table — if that one fails too, the construct activates anyway, exhausted.\nActivated constructs are then linked in pairs (six links, a 3-column table each). Starting a link costs 1 component. A column whose bottom exceeds its top costs 1 HP and 1 extra component (otherwise the whole link resets). The waste basket lets you discard a die instead of placing it — 10 dice maximum for the whole game. The sum of the link values sets the final activation difficulty: aim low! Once all six links are made, the final activation decides the game: 2d6 against that difficulty, each failure costing 1 HP and 1 day. Before the first roll, you may sacrifice HP to reduce the difficulty by that amount (once only).",
     "Un construct trouvé doit être activé : on place des dés sur sa table de 4 colonnes. Une colonne complète dont l'écart (haut − bas) vaut ±5 rapporte 2 énergies, ±4 en rapporte 1, 0 efface la colonne (à refaire), tout autre écart la verrouille et coûte 1 PV. Il faut 4 énergies pour activer ; le surplus charge la God's Hand. Une table ratée coûte 1 jour et donne droit à une seconde table — si elle échoue aussi, le construct s'active quand même, épuisé.\nLes constructs activés se relient ensuite deux à deux (six liens, table de 3 colonnes chacun). Démarrer un lien coûte 1 composant. Une colonne dont le bas dépasse le haut coûte 1 PV et 1 composant de plus (sinon le lien est entièrement à refaire). La corbeille permet de jeter un dé au lieu de le placer — 10 dés maximum pour toute la partie. La somme des valeurs de liens fixe la difficulté de l'activation finale : visez bas ! Une fois les six liens établis, l'activation finale décide de la victoire : 2d6 contre cette difficulté, chaque échec coûte 1 PV et 1 jour. Avant le premier lancer, vous pouvez sacrifier des PV pour réduire la difficulté d'autant (une seule fois)."),
    ("HelpScoreTitle", "Scoring", "Le score"),
    ("HelpScoreBody",
     "Remaining HP + 10 per found construct + 5 per activated construct + 10 per legendary treasure + 5 per link + 5 per crossed skull + 20 per unmodified perfect zero + 10 per unused item. Victory: +50, plus 5 per remaining day.",
     "PV restants + 10 par construct trouvé + 5 par construct activé + 10 par trésor légendaire + 5 par lien + 5 par crâne barré + 20 par zéro parfait non modifié + 10 par objet non utilisé. Victoire : +50, plus 5 par jour restant."),
]

RESX_HEADER = '''<?xml version="1.0" encoding="utf-8"?>
<root>
  <resheader name="resmimetype"><value>text/microsoft-resx</value></resheader>
  <resheader name="version"><value>2.0</value></resheader>
  <resheader name="reader"><value>System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value></resheader>
  <resheader name="writer"><value>System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value></resheader>
'''

def resx(pairs):
    out = io.StringIO()
    out.write(RESX_HEADER)
    for key, val in pairs:
        out.write(f'  <data name="{key}" xml:space="preserve">\n    <value>{html.escape(val)}</value>\n  </data>\n')
    out.write('</root>\n')
    return out.getvalue()

import pathlib
dst = str(pathlib.Path(__file__).parent)
os.makedirs(dst, exist_ok=True)

with open(os.path.join(dst, "Strings.resx"), "w", encoding="utf-8") as f:
    f.write(resx([(k, en) for k, en, fr in S]))
with open(os.path.join(dst, "Strings.fr.resx"), "w", encoding="utf-8") as f:
    f.write(resx([(k, fr) for k, en, fr in S]))

lcs = io.StringIO()
lcs.write('''// Généré par gen_strings.py (même dossier) — ne pas éditer à la main : modifier le script et regénérer.
using System.Resources;

namespace UE.UI.Localization;

/// <summary>Chaînes UI localisées (Strings.resx = anglais neutre, Strings.fr.resx = français, culture du thread).</summary>
public static class L
{
    private static readonly ResourceManager RM = new("UE.UI.Localization.Strings", typeof(L).Assembly);

    private static string T(string key) => RM.GetString(key) ?? key;

''')
for k, en, fr in S:
    lcs.write(f'    public static string {k} => T(nameof({k}));\n\n')
lcs.write('}\n')
with open(os.path.join(dst, "L.cs"), "w", encoding="utf-8") as f:
    f.write(lcs.getvalue())

print(f"{len(S)} clés générées")
