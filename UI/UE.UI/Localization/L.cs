// Généré par gen_strings.py (même dossier) — ne pas éditer à la main : modifier le script et regénérer.
using System.Resources;

namespace UE.UI.Localization;

/// <summary>Chaînes UI localisées (Strings.resx = anglais neutre, Strings.fr.resx = français, culture du thread).</summary>
public static class L
{
    private static readonly ResourceManager RM = new("UE.UI.Localization.Strings", typeof(L).Assembly);

    private static string T(string key) => RM.GetString(key) ?? key;

    public static string Help => T(nameof(Help));

    public static string StatusHp => T(nameof(StatusHp));

    public static string StatusDay => T(nameof(StatusDay));

    public static string StatusDaysRemaining => T(nameof(StatusDaysRemaining));

    public static string StatusScore => T(nameof(StatusScore));

    public static string StatusGodsHand => T(nameof(StatusGodsHand));

    public static string HomeTagline => T(nameof(HomeTagline));

    public static string NewGame => T(nameof(NewGame));

    public static string ContinueSaved => T(nameof(ContinueSaved));

    public static string Camp => T(nameof(Camp));

    public static string SearchAction => T(nameof(SearchAction));

    public static string ConstructsToActivate => T(nameof(ConstructsToActivate));

    public static string ConstructsFoundLabel => T(nameof(ConstructsFoundLabel));

    public static string LinksPossibleLabel => T(nameof(LinksPossibleLabel));

    public static string LinksConnectedLabel => T(nameof(LinksConnectedLabel));

    public static string ComponentInfo => T(nameof(ComponentInfo));

    public static string ConstructInfo => T(nameof(ConstructInfo));

    public static string TreasureInfo => T(nameof(TreasureInfo));

    public static string FoundState => T(nameof(FoundState));

    public static string ToFindState => T(nameof(ToFindState));

    public static string EventsPrefix => T(nameof(EventsPrefix));

    public static string SearchingFor => T(nameof(SearchingFor));

    public static string SearchCostsDays => T(nameof(SearchCostsDays));

    public static string RegionExhausted => T(nameof(RegionExhausted));

    public static string SearchResultLabel => T(nameof(SearchResultLabel));

    public static string SearchResultModified => T(nameof(SearchResultModified));

    public static string UseDowsingRod => T(nameof(UseDowsingRod));

    public static string ApplySearch => T(nameof(ApplySearch));

    public static string SearchAgain => T(nameof(SearchAgain));

    public static string BackToRegions => T(nameof(BackToRegions));

    public static string NothingFound => T(nameof(NothingFound));

    public static string ConstructFoundPerfect => T(nameof(ConstructFoundPerfect));

    public static string ConstructFoundMsg => T(nameof(ConstructFoundMsg));

    public static string ComponentsFoundMsg => T(nameof(ComponentsFoundMsg));

    public static string DiceLabel => T(nameof(DiceLabel));

    public static string DiceHint => T(nameof(DiceHint));

    public static string MustFight => T(nameof(MustFight));

    public static string SpiritSuffix => T(nameof(SpiritSuffix));

    public static string YouRoll => T(nameof(YouRoll));

    public static string RollCombatDice => T(nameof(RollCombatDice));

    public static string Flee => T(nameof(Flee));

    public static string YouFlee => T(nameof(YouFlee));

    public static string UseWandButton => T(nameof(UseWandButton));

    public static string SkipWand => T(nameof(SkipWand));

    public static string WandApplied => T(nameof(WandApplied));

    public static string YouLoseHp => T(nameof(YouLoseHp));

    public static string EncounterDefeated => T(nameof(EncounterDefeated));

    public static string LootComponent => T(nameof(LootComponent));

    public static string LootTreasure => T(nameof(LootTreasure));

    public static string DiedToWounds => T(nameof(DiedToWounds));

    public static string EventsRolled => T(nameof(EventsRolled));

    public static string GameLostTime => T(nameof(GameLostTime));

    public static string UnconsciousMsg => T(nameof(UnconsciousMsg));

    public static string GameLostFmt => T(nameof(GameLostFmt));

    public static string ActivationTitle => T(nameof(ActivationTitle));

    public static string Attempt1 => T(nameof(Attempt1));

    public static string Attempt2 => T(nameof(Attempt2));

    public static string EnergyLabel => T(nameof(EnergyLabel));

    public static string ColumnEnergy => T(nameof(ColumnEnergy));

    public static string ColumnZero => T(nameof(ColumnZero));

    public static string ColumnBacklash => T(nameof(ColumnBacklash));

    public static string FocusCharmUsed => T(nameof(FocusCharmUsed));

    public static string UseFocusCharm => T(nameof(UseFocusCharm));

    public static string ConstructActivated => T(nameof(ConstructActivated));

    public static string SurplusToGodsHand => T(nameof(SurplusToGodsHand));

    public static string ExhaustedActivation => T(nameof(ExhaustedActivation));

    public static string AttemptFailed => T(nameof(AttemptFailed));

    public static string DiedToBacklash => T(nameof(DiedToBacklash));

    public static string BackToConstructs => T(nameof(BackToConstructs));

    public static string ConstructsTitle => T(nameof(ConstructsTitle));

    public static string NotFoundState => T(nameof(NotFoundState));

    public static string ActivatedState => T(nameof(ActivatedState));

    public static string FoundToActivateState => T(nameof(FoundToActivateState));

    public static string ActivateAction => T(nameof(ActivateAction));

    public static string LinksTitle => T(nameof(LinksTitle));

    public static string LinksSummary => T(nameof(LinksSummary));

    public static string LinkConnected => T(nameof(LinkConnected));

    public static string LinkNeedsActivated => T(nameof(LinkNeedsActivated));

    public static string LinkNoComponent => T(nameof(LinkNoComponent));

    public static string LinkReady => T(nameof(LinkReady));

    public static string ConnectAction => T(nameof(ConnectAction));

    public static string FinalActivationButton => T(nameof(FinalActivationButton));

    public static string ConnectTitle => T(nameof(ConnectTitle));

    public static string ConnectHint => T(nameof(ConnectHint));

    public static string BasketLabelFmt => T(nameof(BasketLabelFmt));

    public static string AutoConnectButton => T(nameof(AutoConnectButton));

    public static string AutoConnected => T(nameof(AutoConnected));

    public static string ComponentsSpent => T(nameof(ComponentsSpent));

    public static string NegativeColumns => T(nameof(NegativeColumns));

    public static string LinkFailedReset => T(nameof(LinkFailedReset));

    public static string LinkMade => T(nameof(LinkMade));

    public static string DiedToLinkBacklash => T(nameof(DiedToLinkBacklash));

    public static string BackToLinks => T(nameof(BackToLinks));

    public static string FinalTitle => T(nameof(FinalTitle));

    public static string FinalIntro => T(nameof(FinalIntro));

    public static string FinalDifficulty => T(nameof(FinalDifficulty));

    public static string SacrificeHpButton => T(nameof(SacrificeHpButton));

    public static string SacrificeDone => T(nameof(SacrificeDone));

    public static string FinalRollButton => T(nameof(FinalRollButton));

    public static string FinalRollLine => T(nameof(FinalRollLine));

    public static string FinalFail => T(nameof(FinalFail));

    public static string Victory => T(nameof(Victory));

    public static string FinalScore => T(nameof(FinalScore));

    public static string LifeExhausted => T(nameof(LifeExhausted));

    public static string SkullsFmt => T(nameof(SkullsFmt));

    public static string GodsHandFmt => T(nameof(GodsHandFmt));

    public static string BasketFmt => T(nameof(BasketFmt));

    public static string CrossSkullButton => T(nameof(CrossSkullButton));

    public static string SkullCrossed => T(nameof(SkullCrossed));

    public static string RestHint => T(nameof(RestHint));

    public static string RestButton => T(nameof(RestButton));

    public static string Rested => T(nameof(Rested));

    public static string ComponentsHeader => T(nameof(ComponentsHeader));

    public static string ItemsHeader => T(nameof(ItemsHeader));

    public static string TreasuresHeader => T(nameof(TreasuresHeader));

    public static string NoTreasures => T(nameof(NoTreasures));

    public static string ChargedState => T(nameof(ChargedState));

    public static string UsedState => T(nameof(UsedState));

    public static string DowsingRodName => T(nameof(DowsingRodName));

    public static string ParalysisWandName => T(nameof(ParalysisWandName));

    public static string FocusCharmName => T(nameof(FocusCharmName));

    public static string HelpTitle => T(nameof(HelpTitle));

    public static string HelpBack => T(nameof(HelpBack));

    public static string HelpForThisScreen => T(nameof(HelpForThisScreen));

    public static string HelpFullHelp => T(nameof(HelpFullHelp));

    public static string HelpGoalTitle => T(nameof(HelpGoalTitle));

    public static string HelpGoalBody => T(nameof(HelpGoalBody));

    public static string HelpTimeTitle => T(nameof(HelpTimeTitle));

    public static string HelpTimeBody => T(nameof(HelpTimeBody));

    public static string HelpSearchTitle => T(nameof(HelpSearchTitle));

    public static string HelpSearchBody => T(nameof(HelpSearchBody));

    public static string HelpResultsTitle => T(nameof(HelpResultsTitle));

    public static string HelpResultsBody => T(nameof(HelpResultsBody));

    public static string HelpCombatTitle => T(nameof(HelpCombatTitle));

    public static string HelpCombatBody => T(nameof(HelpCombatBody));

    public static string HelpItemsTitle => T(nameof(HelpItemsTitle));

    public static string HelpItemsBody => T(nameof(HelpItemsBody));

    public static string HelpGodshandTitle => T(nameof(HelpGodshandTitle));

    public static string HelpGodshandBody => T(nameof(HelpGodshandBody));

    public static string HelpConstructsTitle => T(nameof(HelpConstructsTitle));

    public static string HelpConstructsBody => T(nameof(HelpConstructsBody));

    public static string HelpScoreTitle => T(nameof(HelpScoreTitle));

    public static string HelpScoreBody => T(nameof(HelpScoreBody));

    public static string LanguageLabel => T(nameof(LanguageLabel));

}
