using System;
using System.Linq;
using System.Collections.Generic;
using UE.Core.Architecture;
using UE.Core.Architecture.Messages;
using UE.Core.Entities;
using UE.Core.Interfaces;

namespace UE.Core
{
    public class GameEngine:IGameEngine
    {

        private readonly IRepository _repository;

        public GameEngine(IRepository repository, IDiceRoller diceGenerator)
        {
            _repository = repository;
            DiceGenerator = diceGenerator;
        }

        public int DaysRemaining
        {
            get { return (GameDefinition.FirstTheoricalLosingDay + GameState.NumberOfSkullsCrossed) - GameState.CurrentDay; }
        }

        public bool CanRest { get { return GameState.CurrentHitPoint < GameDefinition.MaxHitPoint; } }

        public IDiceRoller DiceGenerator { get; set; }

        public GameDefinition GameDefinition { get; private set; }
        public GameState GameState { get; private set; }
        public bool IsEventDay { get { return GameDefinition.IsEventDay(GameState.CurrentDay - 1); } }

        public bool IsFinished
        {
            get { return IsGameLost || IsGameWon; }
        }

        public bool IsGameLost
        {
            get
            {
                return (GameState.CurrentHitPoint < 0)
                       ||
                       (GameState.CurrentDay >=
                        ((GameDefinition.MaximumNumberOfDays - GameDefinition.NumberOfSkulls) + 1) +
                        GameState.NumberOfSkullsCrossed)
                       || (GameState.CurrentDay == GameDefinition.MaximumNumberOfDays);
            }
        }

        public bool IsGameWon
        {
            get { return GameState.IsGameWon; }
        }

        public bool IsGodsHandUsable
        {
            get { return (GameState.GodsHandEnergy >= 3) && (GameState.NumberOfSkullsCrossed < GameDefinition.NumberOfSkulls); }
        }

        public int Score
        {
            get
            {
                int score = GameState.NumberOfSkullsCrossed * 5;
                score += GameState.UnmodifiedPerfectZeroSearch * 20;
                if (GameState.CurrentHitPoint>0)
                    score += GameState.CurrentHitPoint;
                score += GameState.ConstructsFound.Count() * 10;
                score += GameState.TreasuresFound.Count() * 10;
                score += GameState.ConstructsActivated.Count() * 5;
                score += GameState.Inventory.Score();
                score += GameState.ConnectedLinks.Count()*5;
                if (IsGameWon)
                {
                    score += 50;
                    score += (GameDefinition.MaximumNumberOfDays - GameState.CurrentDay) * 5;
                }
                return score;
            }
        }

        public static int EncounterLevel(int result)
        {
            if (result >= 500 || result <= -401)
                return 5;
            if (result >= 400 || result <= -301)
                return 4;
            if (result >= 300 || result <= -201)
                return 3;
            if (result >= 200 || result <= -101)
                return 2;
            return 1;
        }

        public void ActivateConstruct(int indexRegion)
        {
            ActivateConstruct(GameDefinition.GetRegion(indexRegion));
        }

        public void AddComponent(int quantity, Component component)
        {
            AddComponent(quantity, component.ID);
        }

        public void AddComponent(int quantity, int componentID)
        {
            GameState.Inventory.AddToStore(componentID, quantity, GameDefinition.MaxComponentInInventory);
        }

        public TimePassed AddToCurrentDay(int numberOfDays)
        {
            TimePassed t = new TimePassed();
            AddToCurrentDay(numberOfDays, t);
            return t;
        }

        public TimePassed AddToCurrentDay(int numberOfDays, TimePassed timePassed)
        {
            AddDays(numberOfDays, timePassed);
            if (timePassed.eventOccured)
            {
                RollEvents();
            }
            return timePassed;
        }

        public CombatResult ApplyCombatRoll(TwoDice dice, Encounter e, Region r)
        {
            CombatResult cr = new CombatResult();
            ApplyCombatDie(dice.First, e, cr);
            ApplyCombatDie(dice.Second, e, cr);
            if (cr.EncounterDead)
            {
                GetLoot(e, r, cr);
            }
            cr.DiceRoll = dice;
            return cr;
        }

        public SearchResult ApplySearch(int result, Region region, bool modified)
        {
            SearchResult sr = new SearchResult { FinalResult = result };
            RegionState rs = GetRegionStateFor(region);
            if (result >= 0 && result <= 10)
            {
                if (rs.ConstructFound)
                {
                    AddComponent(2, region.Component);
                    sr.NumberOfComposantFound = 2;
                }
                else
                {
                    FindConstruct(rs, result == 0);
                    sr.ConstructFound = true;
                    if (result == 0)
                    {
                        if (!modified)
                        {
                            GameState.UnmodifiedPerfectZeroSearch++;
                        }
                        AddToGodsHand(5);
                    }
                }
            }
            if (result > 10 && result < 100)
            {
                AddComponent(1, region.Component);
                sr.NumberOfComposantFound = 1;
            }
            if (result < 0 || result >= 100)
            {
                sr.MonsterLevel = EncounterLevel(result);
                if (HasAbility(Ability.ActiveMonsters, region.Index))
                {
                    sr.MonsterLevel += 2;
                    if (sr.MonsterLevel > 5)
                        sr.MonsterLevel = 5;
                }
            }
            GameState.LastPlayed = DateTime.Now;
            return sr;
        }

		public CanSearchResult CanModifySearchResult(int result, int idRegion)
		{ return GameState.CanModifySearchResult(result, idRegion); }

		public CanSearchResult CanModifySearchResultInCurrentRegion(int result)
		{ return GameState.CanModifySearchResult(result, GameState.CurrentSearchRegionIndex); }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public TimePassed CrossRegionTracker(Region r)
        {
            RegionState rs = GetRegionStateFor(r);
            int penalty = r.DayPenaltyFor(rs.SearchBoxes.Count() - rs.RemainingSearchBoxes);
            bool foulWeatherApplied = false;
            if (HasAbility(Ability.FoulWeather, r.Index) && penalty > 0)
            {
                foulWeatherApplied = true;
                penalty++;
            }
            if (penalty > 0)
            {
                TimePassed t = AddDays(penalty, null);
                if (t.eventOccured)
                {
                    RollEvents();
                }
                if (!foulWeatherApplied && HasAbility(Ability.FoulWeather, r.Index))
                {
                    TimePassed t2 = AddDays(1, null);
                    t.GameLost = t2.GameLost;
                    t.DaysPassed++;
                }
                return t;
            }
            return new TimePassed { GameLost = IsGameLost };
        }
		 
		public Table CurrentSearchBox
		{
			get{
				return CurrentSearchBoxForRegion (GameState.CurrentSearchRegionIndex);
			}
		}

        public Table CurrentSearchBoxForRegion(int indexRegion)
        {
            return GetRegionStateFor(indexRegion).SearchBoxes.First(x => x.IsFull == false);
        }
        public void EnterRegionForSearch(int index)
        {
            GameState.EventCanceledInCurrentRegion = false;
            GameState.ClearSearchboxes(index);
            GameState.CurrentSearchRegionIndex = index;
        }

        public void EnterRegionForSearch(Region r)
        {
            EnterRegionForSearch(r.Index);
        }

        public ConstructState FindConstruct(RegionState rs, bool activated)
        {
            ConstructState constructState = GetConstructStateFor(rs.Region.Construct.ID);
            constructState.HasBeenFound = true;
            rs.ConstructFound = true;
            if (activated)
                ActivateConstruct(rs.Region);
            return constructState;
        }

        public ConstructState GetConstructStateFor(int idConstruct)
        {
            return GameState.Constructs.Single(x => x.ID == idConstruct);
        }

        public Region GetRegion(int indexRegion)
        {
            return GameDefinition.GetRegion(indexRegion);
        }

        public RegionState GetRegionStateFor(int indexRegion)
        {
            return GameState.RegionStates.Single(x => x.Index == indexRegion);
        }

        public RegionState GetRegionStateFor(Region r)
        {
            return GetRegionStateFor(r.Index);
        }

        public bool HasAbility(Ability ability, int indexRegion = -1)
        {
            bool ret = GameState.TreasuresFound.Any(x => x.HasAbility(ability));
            ret |= GameState.HasActivatedContructAbility(ability);
            if (indexRegion == -1)
            {
                ret |= GameState.RegionStates.Any(x => x.HasAbility(ability));
            }
            else
            {
                ret |= GetRegionStateFor(indexRegion).HasAbility(ability);
            }
            return ret;
        }

        public void Init(string definitionData,string quoteFile="Data\\Quotes.xml")
        {
            GameDefinition = _repository.LoadDefinition(definitionData,quoteFile);
            foreach (Region r in GameDefinition.Regions)
            {
                r.LegendaryTreasure = GameDefinition.LegendaryTreasures.SingleOrDefault(x => x.IndexRegion == r.Index);
                r.Component = GameDefinition.Components.SingleOrDefault(x => x.IndexRegion == r.Index);
            }
            GameState = new GameState(GameDefinition);
        }

        public void LoadGameState(string file)
        {
            this.GameState = _repository.LoadGameState(file);
            GameState.Hydrate(GameDefinition);
        }

        public CombatResult MakeCombatTurn(Encounter e, Region r)
        {
            TwoDice dice = DiceGenerator.Get2d6();
            if (GameState.ParalysisWandInUse)
                dice.ModifyBothDie(2);
            if (e.IsSpirit && HasAbility(Ability.HelpAgainstSpirit))
                dice.ModifyBothDie(1);
            return ApplyCombatRoll(dice, e, r);
        }

        public int NumberOfAvailableSearchesBoxesFor(int indexRegion)
        {
            return GetRegionStateFor(indexRegion).RemainingSearchBoxes;
        }

        public void PlaceSearchNumberOnRegion(int region, int indexWhere, int value)
        {
            Table t = CurrentSearchBoxForRegion(region);
            t.PlaceNumber(indexWhere, value);
        }

        public TimePassed RecoverFromUnconsciousness()
        {
            int nbDays = 6;
            if (HasAbility(Ability.RecoverFaster))
            {
                nbDays = 4;
            }
            TimePassed t = AddDays(nbDays, new TimePassed());
            GameState.CurrentHitPoint = GameDefinition.MaxHitPoint;
            return t;
        }

        public void ResetGameState()
        {
            GameState.ResetState(GameDefinition);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nbDays"></param>
        /// <returns></returns>
        public TimePassed Rest(int nbDays)
        {
            GameState.LastPlayed = DateTime.Now;
            return AddDays(nbDays, null, true);
        }

        public void RollEvents()
        {
            foreach (RegionState regionState in GameState.RegionStates)
            {
                regionState.ClearEvents();
            }

            foreach (RegionEvent gameEvent in GameDefinition.Events)
            {
                int indexRegion = DiceGenerator.Get1d6();
                GetRegionStateFor(indexRegion).AddEvent(gameEvent);
            }
        }

        public void SaveGameState(string file)
        {
            _repository.SaveGameState(file, GameState);
        }
        public void StartFight()
        {
            GameState.ParalysisWandInUse = false;
        }

        public override string ToString()
        {
            return String.Format("Hitpoint:{0}, CurrentDay:{1},GodsHandEnergy:{2},SkullsCrossed:{3}", GameState.CurrentHitPoint, GameState.CurrentDay, GameState.GodsHandEnergy, GameState.NumberOfSkullsCrossed);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="r"></param>
        /// <returns>True if treasure was found, false if already found</returns>
        public bool TreasureIsFound(Region r)
        {
            RegionState rs = GetRegionStateFor(r);
            if (!rs.LegendaryTreasureFound)
            {
                rs.LegendaryTreasureFound = true;
                return true;
            }
            return false;
        }

        public int UseDowsingRod(int result, int number)
        {
            if (result < 2)
            {
                throw new InvalidOperationException("Trying to use DowsingRod on Result = " + result);
            }
            if (number < 1 || number > 100)
            {
                throw new ArgumentException("Trying to use DowsingRod with number = " + number);
            }
            GameState.Inventory.DowsingRodCharged = false;
            return Math.Max(1, result - number);
        }

        public void UseGodsHand()
        {
            if (!IsGodsHandUsable)
            {
                throw new InvalidOperationException("Using GodsHand when not possible");
            }
            GameState.GodsHandEnergy -= 3;
            GameState.NumberOfSkullsCrossed++;
        }

        public void UseFocusCharm(int constructID)
        {
            if (!GameState.Inventory.FocusCharmCharged)
            {
                throw new InvalidOperationException("Trying to use already used FocusWand");
            }
            GameState.Inventory.FocusCharmCharged = false;
            GameState.FocusCharmInUseForConstructID = constructID;
        }

        public void UseParalysisWand()
        {
            if (GameState.Inventory.ParalysisWandCharged == false)
            {
                throw new InvalidOperationException("Trying to use already used ParalysisWand");
            }
            GameState.ParalysisWandInUse = true;
            GameState.Inventory.ParalysisWandCharged = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="?"></param>
        /// <param name="position">from 1 to 8</param>
        /// <param name="number"></param>
        /// <returns></returns>
        public ActivationResult WorkToActivate(ConstructState cs, int position, int number)
        {
            ActivationResult ar = new ActivationResult();
            Table at = cs.CurrentActivationTable;
            at.PlaceNumber(position, number);
            ar.EnergyPoints = at.ActivationResult;
            if (HasAbility(Ability.FleetingVisions, GetRegionForConstruct(cs).Index))
            {
                ar.EnergyPoints++;
            }
            if (cs.Construct.ID == GameState.FocusCharmInUseForConstructID)
            {
                ar.EnergyPoints += 2;
            }
            int col = at.ColumnFromPosition(position);
            if (at.Columns[col].IsFull)
            {
                ar.CurrentColumnValue = at.Columns[col].EnergyPointValue;
                if (ar.CurrentColumnValue == 0)
                {
                    at.Clear(col);
                }
                if (ar.CurrentColumnValue == -1)
                {
                    AddToHitPoint(-1);
                }
            }
            ar.IsFieldFilled = at.IsFull;
            if (ar.IsFieldFilled)
            {
                if (ar.EnergyPoints >= 4)
                {
                    ar.IsConstructActivated = true;
                    ActivateConstruct(cs);
                    AddToGodsHand(ar.EnergyPoints - 4);
                }
                else
                {
                    AddToCurrentDay(1, ar);
                    if (cs.ActivationTable2.IsFull)
                    { //Automatically Activate if failed on the two fields
                        ar.IsConstructActivated = true;
                        ActivateConstruct(cs);
                    }
                }
            }
            return ar;
        }

        private void ActivateConstruct(ConstructState cs)
        {
            cs.HasBeenActivated = true;
            if (HasAbility(Ability.ChargeGodsHandOnActivation))
            {
                AddToGodsHand(1);
            }
        }

        private void ActivateConstruct(Region r)
        {
            ConstructState constructState = GetConstructStateFor(r.Construct.ID);
            ActivateConstruct(constructState);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ID">Link Id</param>
        /// <param name="pos">0 for wastebasket</param>
        /// <param name="value"></param>
        /// <param name="isFirstDie"></param>
        /// <returns></returns>
        public LinkResult WorkToLink(int ID, int pos, int value, bool isFirstDie)
        {
            LinkResult result;
            if (pos == 0)
            {
                AddToWasteBasket();
                result = new LinkResult();
            }
            else
            {
                result = WorkToLink(ID, pos, value);
                if (result.IsLinkFinished)
                {
                    if (isFirstDie && !GameState.IsWasteBasketFull)
                    {
                        // discard second die
                        AddToWasteBasket();
                    }
                }
            }
            return result;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="IinkID"></param>
        /// <param name="position">0 for waste baskert</param>
        /// <param name="value"></param>
        /// <returns></returns>
        private LinkResult WorkToLink(int IinkID, int position, int value)
        {
            LinkResult lr = new LinkResult();
            LinkState ls = GameState.LinkStates.Single(x => x.ID == IinkID);
            if (!ls.IsStarted)
            {
                ls.IsStarted = true;
                AddComponent(-1, ls.Link.ComponentID);
                lr.ComponentLost = 1;
            }
            if (position == 0)
                AddToWasteBasket();
            ls.Connection.PlaceNumber(position, value);
            lr.IsLinkFinished = ls.IsLinkDone;
            if (lr.IsLinkFinished)
            {
                foreach (Column c in ls.Connection.Columns)
                {
                    if (c.ActivationValue < 0)
                    {
                        lr.HitPointLost++;
                        AddToHitPoint(-1);
                        if (GameState.NumberOfComponents(ls.Link.ComponentID) > 0)
                        {
                            lr.ComponentLost++;
                            AddComponent(-1, ls.Link.ComponentID);
                            c.ForcedValue = 2;
                        }
                        else
                        {
                            lr.HasFailed = true;
                        }
                    }
                }
            }
            if (lr.HasFailed)
            {
                ls.Reset();
            }
            else
            {
                lr.LinkBox = ls.LinkBox;
            }
            return lr;
        }

        public void SpendHPToReduceActivationDifficulty(int hitPoint)
        {
            if (hitPoint > GameState.CurrentHitPoint)
            {
                throw new ArgumentOutOfRangeException("Reducing Activation difficulty by too mnay hitpoints " + hitPoint);
            }
            if (GameState.FinalActivationModifier != 0)
            {
                throw new InvalidOperationException("You can't reduce twice the final activation difficulty");
            }
            GameState.CurrentHitPoint -= hitPoint;
            GameState.FinalActivationModifier = -hitPoint;
        }

        public FinalActivationResult WorkForfinalActivation()
        {
            FinalActivationResult result = new FinalActivationResult();
            TwoDice td = DiceGenerator.Get2d6();
            result.Roll = td;
            result.GameWon = td.Sum >= GameState.FinalActivationDifficulty;
            GameState.IsGameWon = result.GameWon;
            if (!result.GameWon)
            {
                AddToHitPoint(-1);
                AddToCurrentDay(1, result);
            }
            return result;
        }

        public void AddToWasteBasket()
        {
            if (GameState.IsWasteBasketFull)
            {
                throw new InvalidOperationException("Waste basket is already full");
            }
            GameState.WasteBasket++;
        }
        /// <summary>
        /// Should never be called directly (does not roll for event)
        /// </summary>
        /// <param name="numberOfDays"></param>
        /// <param name="isResting"></param>
        /// <returns></returns>
        private TimePassed AddDays(int numberOfDays, TimePassed t, bool isResting = false)
        {
            if (t == null)
                t = new TimePassed();
            for (int i = 1; i <= numberOfDays; i++)
            {
                GameState.CurrentDay++;
                t.DaysPassed++;
                t.GameLost = (IsGameLost);
                if (t.GameLost)
                    break;
                if (isResting)
                    AddToHitPoint(1);
                if (HasAbility(Ability.RecoverHP))
                    AddToHitPoint(1);


                if (IsEventDay)
                    t.eventOccured = true;
            }
            return t;
        }

        private void AddToGodsHand(int energyPoints)
        {
            GameState.GodsHandEnergy += energyPoints;
            if (GameState.GodsHandEnergy < 0)
                GameState.GodsHandEnergy = 0;
            if (GameState.GodsHandEnergy > GameDefinition.GodsHandCapacity)
                GameState.GodsHandEnergy = GameDefinition.GodsHandCapacity;
            if (GameState.GodsHandEnergy >= 3)
            {
                GameState.GodsHandEnergy -= 3;
                if (GameDefinition.NumberOfSkulls > GameState.NumberOfSkullsCrossed)
                    GameState.NumberOfSkullsCrossed++;
            }

        }
        private void AddToHitPoint(int value)
        {
            GameState.CurrentHitPoint += value;
            if (GameState.CurrentHitPoint >= GameDefinition.MaxHitPoint)
                GameState.CurrentHitPoint = GameDefinition.MaxHitPoint;
        }

        private void ApplyCombatDie(int die, Encounter e, CombatResult cr)
        {
            int attackValue = CorrectAttackValue(e.Attack);
            if (die <= attackValue)
            {
                AddToHitPoint(-1);
                cr.HitpointLost++;
            }
            int hitValue = CorrectHitValue(e.Hit);
            cr.EncounterDead |= (die >= hitValue);
        }

        private int CorrectAttackValue(int baseAttackValue)
        {
            int av = baseAttackValue;
            if (HasAbility(Ability.BetterDefense))
            {
                av--;
            }
            if (av < 1)
                av = 1;
            return av;
        }

        private int CorrectHitValue(int baseHitValue)
        {
            int hv = baseHitValue;
            if (HasAbility(Ability.BetterAttack))
            {
                hv--;
            }
            return hv;
        }

        private void GetLoot(Encounter e, Region r, CombatResult cr)
        {
            bool loot = DiceGenerator.Get1d6() <= e.Level;
            if (loot)
            {
                if (e.Level < 5)
                {
                    AddComponent(1, r.Component);
                    cr.ComponentFound = true;
                }
                else
                {
                    cr.LegendaryTreasureFound = TreasureIsFound(r);
                }
            }
        }

        private Region GetRegionForConstruct(ConstructState cs)
        {
            Region r = GameDefinition.Regions.Single(x => x.Construct.ID == cs.Construct.ID);
            return r;
        }

        public void UseAutomaticConnect(LinkState ls)
        {
            ls.Connection.Clear();
            ls.Connection.ForcedValue = 2;
        }


        public bool CanLink
        {
            get { return GameState.PossibleLinks.Any(); }
        }

        public bool CanActivate
        {
            get { return GameState.ConstructsUnactivated.Any(); }


        }

        public bool IsFinalActivationPossible { get { return GameState.ConnectedLinks.Count() == GameState.LinkStates.Count(); } }


        public void LoadAutoSave()
        {
            _repository.LoadAutoSave();
        }
    }
}
