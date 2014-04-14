using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace UE.Core.Entities
{
    public class GameDefinition
    {
        [XmlIgnore]
        public int MaxComponentInInventory
        {
            get { return 4; }
        }

        [XmlIgnore]
        public int GodsHandCapacity { get { return 6; } }

        public bool EventsActivated { get; set; }

        private string _timeTrack;

        [XmlAttribute]
        public int ID { get; set; }
        public int MaxHitPoint { get; set; }
        public List<RegionEvent> Events { get; set; }
        public List<Region> Regions { get; set; }
        public List<Component> Components { get; set; }
        public List<LegendaryTreasure> LegendaryTreasures { get; set; }

        public List<Link> Links { get; set; }

        [XmlIgnore]
        public string[] DetailledTimeTrack { get; private set; }

        public string TimeTrack
        {
            get { return _timeTrack; }
            set
            {
                _timeTrack = value;
                DetailledTimeTrack = value.Split(',');
            }
        }

        public int NumberOfSkulls { get; set; }

        [XmlIgnore]
        public int MaximumNumberOfDays
        {
            get { return DetailledTimeTrack.Count(); }
        }

        public bool IsEventDay(int day)
        {
            return DetailledTimeTrack[day] == "E";
        }

        [XmlIgnore]
        public int NumberOfRegions { get { return Regions.Count(); } }

        public Construct GetConstruct(int contructId)
        {
            return Regions.Select(x => x.Construct).Single(y => y.ID == contructId);
        }

        public Region GetRegion(int indexRegion)
        {
            return Regions.Single(x => x.Index == indexRegion);
        }
        
        [XmlIgnore]
        public List<Quote> Quotes { get; set; }
        
        /// <summary>
        /// Base losing day
        /// </summary>
        public int FirstTheoricalLosingDay { get { return MaximumNumberOfDays - (NumberOfSkulls - 1); } }

        public Quote RandomQuote
        {
            get
            {
                RandomDice rd = new RandomDice();
                return Quotes.ElementAt(rd.GetVariableDice(Quotes.Count));
            }
        }
    }
}
