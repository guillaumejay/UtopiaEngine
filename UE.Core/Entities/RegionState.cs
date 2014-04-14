using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using UE.Core.Architecture;

namespace UE.Core.Entities
{
    public class RegionState
    {
        public RegionState()
        {
            Events=new List<RegionEvent>();
            EventsID=new List<int>();
        }

        [XmlAttribute]
        public int Index { get; set; }

        public List<int> EventsID { get; set; }

        [XmlIgnore]
        public List<RegionEvent> Events { get; set; }

        public List<Table> SearchBoxes { get; set; }
        
        public bool LegendaryTreasureFound { get; set; }

        [XmlIgnore]
        public LegendaryTreasure LegendaryTreasure { get; set; }

        [XmlIgnore]
        public bool ConstructFound { get; set; }

        [XmlIgnore]
        public Region Region { get; set; }

        [XmlIgnore]
        public int RemainingSearchBoxes
        {
            get { return SearchBoxes.Count(x => x.IsFull == false); }
        }

        public Table CurrentSearchBox
        {
            get
            {
                Table retour = SearchBoxes.SingleOrDefault(x => x.IsStarted);
                if (retour == null)
                {
                    retour = SearchBoxes.FirstOrDefault(x => x.IsEmpty);
                }
                return retour;
            }
        }

        public bool HasAbility(Ability ability)
        {
            return Events.Any(x=>x.Ability==ability);
        }


        internal void ClearSearchBoxes()
        {
            foreach (Table searchBox in SearchBoxes)
            {
                searchBox.Clear();
            }
        }

        internal void ClearEvents()
        {
            Events.Clear();
            EventsID.Clear();
        }

        internal void AddEvent(RegionEvent gameEvent)
        {
           Events.Add(gameEvent);
            EventsID.Add(gameEvent.ID);
        }
    }
}
