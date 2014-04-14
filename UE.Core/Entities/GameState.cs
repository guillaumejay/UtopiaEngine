using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using UE.Core.Architecture;

namespace UE.Core.Entities
{
    public class GameState
    {
        /// <summary>
        /// For Serialization
        /// </summary>
        public GameState()
        {
        }

        public GameState(GameDefinition gameDefinition)
        {
            Inventory = new Inventory(gameDefinition.Components);

            ResetState(gameDefinition);
        }

        public List<LinkState> LinkStates { get; set; }

        public List<ConstructState> Constructs { get; set; }

        public IEnumerable<ConstructState> ConstructsActivated { get { return Constructs.Where(x => x.HasBeenActivated && x.HasBeenFound); } }

        public IEnumerable<ConstructState> ConstructsFound { get { return Constructs.Where(x => x.HasBeenFound); } }

        public IEnumerable<ConstructState> ConstructsUnactivated { get { return Constructs.Where(x => x.HasBeenActivated == false && x.HasBeenFound); } }

        public int CurrentDay { get; set; }

        public int CurrentHitPoint { get; set; }

        public bool EventCanceledInCurrentRegion { get; set; }

        /// <summary>
        /// 0 if not in use
        /// </summary>
        public int FocusCharmInUseForConstructID { get; set; }

        public int GodsHandEnergy { get; set; }

        public Inventory Inventory { get; set; }

        public bool IsWasteBasketFull { get { return WasteBasket >= 10; } }

        public DateTime? LastPlayed { get; set; }

        public int NumberOfComponents(int componentId)
        {
            return Inventory.Stores.Single(x => x.ComponentId == componentId).Quantity;
        }
        public int NumberOfSkullsCrossed { get; set; }

        public bool ParalysisWandInUse { get; set; }

        public List<RegionState> RegionStates { get; set; }

        public IEnumerable<LegendaryTreasure> TreasuresFound
        {
            get { return RegionStates.Where(x => x.LegendaryTreasureFound).Select(y => y.LegendaryTreasure); }
        }

        public int UnmodifiedPerfectZeroSearch { get; set; }

        public int WasteBasket { get; set; }

		internal CanSearchResult CanModifySearchResult(int result, int idRegion)
        {
			CanSearchResult crs = new CanSearchResult ();
			crs.CanUseDowsingRod = CanUseDowsingRod (result);
			crs.CanUseGoodFortune = RegionStates.Single (x => x.Index == idRegion).HasAbility (Ability.GoodFortune);
			switch (idRegion)
			{
			case 1:
			case 6:
				CanModifySearchByConstruct (crs, Ability.HelpSearch16);
				break;
			case 3:
				case 4:
				CanModifySearchByConstruct (crs, Ability.HelpSearch34);
				break;
			}
            return crs;
        }

		void CanModifySearchByConstruct (CanSearchResult crs, Ability helpSearch)
		{
			if (HasActivatedContructAbility(helpSearch))
			    {
				crs.CanUseConstructPower=true;
			}

		}

		internal bool HasActivatedContructAbility (Ability ability)
		{
			return  ConstructsActivated.Any (x => x.HasUsableAbility (ability));
		}

        private bool CanUseDowsingRod(int result)
        {
            return (result < 0 || result > 1) && this.Inventory.DowsingRodCharged;
        }

        public void ResetState(GameDefinition gameDefinition)
        {
            DefinitionID = gameDefinition.ID;
            CurrentHitPoint = gameDefinition.MaxHitPoint;
            GodsHandEnergy = 0;
            CurrentDay = 0;
            ParalysisWandInUse = false;
            NumberOfSkullsCrossed = 0;
            UnmodifiedPerfectZeroSearch = 0;
            WasteBasket = 0;
            RegionStates = new List<RegionState>();
            Constructs = new List<ConstructState>();
            LinkStates = new List<LinkState>();
            foreach (Region r in gameDefinition.Regions)
            {
                RegionState rs = new RegionState();
                rs.Index = r.Index;
                rs.SearchBoxes = new List<Table>();
                for (int i = 0; i < 6; i++)
                {
                    rs.SearchBoxes.Add(new Table(3));
                }

                RegionStates.Add(rs);
                ConstructState cs = new ConstructState(r.Construct);
                Constructs.Add(cs);
            }
            foreach (Link l in gameDefinition.Links)
            {
                LinkState ls = new LinkState { ID = l.ID };
                LinkStates.Add(ls);
            }
            Hydrate(gameDefinition);
            Inventory.Reset();
        }
        internal void ClearSearchboxes(int index)
        {
            RegionStates.Single(x => x.Index == index).ClearSearchBoxes();
        }

        internal void Hydrate(GameDefinition gameDefinition)
        {
            foreach (Region r in gameDefinition.Regions)
            {
                RegionState rs = RegionStates.Single(x => x.Index == r.Index);
                rs.Region = r;
                Constructs.Single(x => x.ID == r.Construct.ID).Construct = r.Construct;
                rs.ConstructFound = Constructs.Single(x => x.ID == r.Construct.ID).HasBeenFound;
                rs.LegendaryTreasure = r.LegendaryTreasure;
                foreach (int id in rs.EventsID)
                {
                    rs.Events.Add(gameDefinition.Events.Single(x => x.ID == id));
                }
            }
            foreach (Link l in gameDefinition.Links)
            {
                LinkState ls = LinkStates.Single(x => x.ID == l.ID);
                ls.Construct1 = Constructs.Single(x=>x.ID==l.ConstructID1);
                ls.Construct2 = Constructs.Single(x => x.ID == l.ConstructID2);
                ls.Link = l;
            }
            Inventory.Components = gameDefinition.Components;
        }

        /// <summary>
        /// return links which can be made, not already done
        /// </summary>
        public IEnumerable<LinkState> PossibleLinks
        {
            get
            {
                IEnumerable<LinkState> list = LinkStates.Where(x => x.IsLinkDone == false);
                list = list.Where(y => y.Construct1.HasBeenActivated && y.Construct2.HasBeenActivated);
                list=list.Where(y=>NumberOfComponents(y.Link.ComponentID)>0);
                return list;
            }
        }


        public IEnumerable<LinkState> ConnectedLinks
        {
            get { return LinkStates.Where(x => x.IsLinkDone); }
        }

      

        public int FinalActivationDifficulty { get { return LinkStates.Sum(x => x.LinkBox) + FinalActivationModifier; } }

        public int FinalActivationModifier { get; set; }

        [XmlIgnore]
        public  bool IsGameWon { get; internal set; }

        [XmlAttribute]
        public int DefinitionID { get; set; }

        [XmlIgnore]
        public int CurrentSearchRegionIndex { get; internal set; }
    }
}