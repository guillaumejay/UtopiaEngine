using System.Xml.Serialization;
using UE.Core.Architecture;

namespace UE.Core.Entities
{
    public class LegendaryTreasure

    {
        [XmlAttribute]
        public int ID { get; set; }

        [XmlAttribute]
        public int IndexRegion { get; set; }

        public LocalizedTexts Name { get; set; }

        public Ability Ability { get; set; }


        internal bool HasAbility(Ability ability)
        {
            return Ability == ability;
        }
    }
}
