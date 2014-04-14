using UE.Core.Architecture;
using UE.Core.Architecture.Messages;
using UE.Core.Entities;

namespace UE.Core.Interfaces
{
   public interface IGameEngine
    {
        void ActivateConstruct(int indexRegion);
        void AddComponent(int quantity, Component component);
        void AddComponent(int quantity, int componentID);
        TimePassed AddToCurrentDay(int numberOfDays);
        TimePassed AddToCurrentDay(int numberOfDays, TimePassed timePassed);
        void AddToWasteBasket();
        CombatResult ApplyCombatRoll(TwoDice dice, Encounter e, Region r);
        SearchResult ApplySearch(int result, Region region, bool modified);
        CanSearchResult CanModifySearchResult(int result, int idRegion);
		CanSearchResult CanModifySearchResultInCurrentRegion(int result);
        bool CanRest { get; }
        TimePassed CrossRegionTracker(Region r);
		Table CurrentSearchBox { get; }
        Table CurrentSearchBoxForRegion(int indexRegion);
        IDiceRoller DiceGenerator { get; set; }
        void EnterRegionForSearch(Region r);
        ConstructState FindConstruct(RegionState rs, bool activated);
        GameDefinition GameDefinition { get; }
        GameState GameState { get; }
        ConstructState GetConstructStateFor(int idConstruct);
        Region GetRegion(int indexRegion);
        RegionState GetRegionStateFor(Region r);
        RegionState GetRegionStateFor(int indexRegion);
        bool HasAbility(Ability ability, int indexRegion = -1);
        void Init(string definitionData,string quoteFile="Data\\Quotes.xml");
        bool IsEventDay { get; }
        bool IsFinished { get; }
        bool IsGameLost { get; }
        bool IsGameWon { get; }
        bool IsGodsHandUsable { get; }
        void LoadGameState(string file);
        CombatResult MakeCombatTurn(Encounter e, Region r);
        int NumberOfAvailableSearchesBoxesFor(int indexRegion);
        void PlaceSearchNumberOnRegion(int region, int indexWhere, int value);
        TimePassed RecoverFromUnconsciousness();
        void ResetGameState();
        TimePassed Rest(int nbDays);
        void RollEvents();
        void SaveGameState(string file);
        int Score { get; }
        void SpendHPToReduceActivationDifficulty(int hitPoint);
        void StartFight();
        string ToString();
        bool TreasureIsFound(Region r);
        void UseAutomaticConnect(LinkState ls);
        int UseDowsingRod(int result, int number);
        void UseFocusCharm(int constructID);
        void UseGodsHand();
        void UseParalysisWand();
        FinalActivationResult WorkForfinalActivation();
        ActivationResult WorkToActivate(ConstructState cs, int position, int number);
        LinkResult WorkToLink(int ID, int pos, int value, bool isFirstDie);

        bool CanLink { get;  }

        bool CanActivate { get;  }

       bool IsFinalActivationPossible { get; }

       void LoadAutoSave();

       int DaysRemaining { get;  }
       void EnterRegionForSearch(int index);
    }
}
