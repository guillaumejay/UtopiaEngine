using System;
using System.Xml.Serialization;
using UE.Core.Architecture;

namespace UE.Core.Entities
{
    public class Encounter
    {
        [XmlAttribute]
        public int Level { get; set; }

        public LocalizedTexts Name { get; set; }
        [XmlAttribute]
        public int Attack { get; set; }
        [XmlAttribute]
        public int Hit { get; set; }
        [XmlAttribute]
        public bool IsSpirit { get; set; }

        [XmlIgnore]
        public string AttackText
        {
            get
            {
                if (Attack == 1)
                    return "1";
                return "1-" + Attack;
            }
        }

        [XmlIgnore]
        public string HitText
        {
            get
            {
                if (Hit == 6)
                    return "6";
                return Hit + "-6";
            }
        }

        public override string ToString()
        {
            string tmp=String.Format("{0} {1}/{2}{3}",Name.TextFor(Tools.Language),AttackText,HitText,(IsSpirit)?" (S)":"");
            return tmp;
        }
    }
}
