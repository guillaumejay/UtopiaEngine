using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using UE.Core.Architecture;

namespace UE.Core.Entities
{
    public class Region
    {
        public LocalizedTexts Name { get; set; }

        public Construct Construct { get; set; }

        [XmlIgnore]
        public Component Component { get; set; }

        [XmlAttribute]
        public int Index { get; set; }
        public List<Encounter> Encounters {get;set;}

        public string TimeTrack { get; set; }

        /// <summary>
        /// Penalty (as a positive number) for a specific step
        /// </summary>
        /// <param name="step">starts from 0</param>
        /// <returns>a positive number (or 0)</returns>
        public int DayPenaltyFor(int step)
        {
            return Convert.ToInt32(TimeTrack.Split(',')[step]);
        }

        public Encounter GetEncounter(int encounterLevel)
        {
            return Encounters.Single(x => x.Level == encounterLevel);
        }


        [XmlIgnore]
        public LegendaryTreasure LegendaryTreasure { get; set; }
    }
}
