
using System.Xml.Serialization;
using UE.Core.Architecture;

namespace UE.Core.Entities
{
    public class RegionEvent
    {
        [XmlAttribute]
        public int ID { get; set; }

        [XmlAttribute]
        public Ability Ability { get; set; }

        public LocalizedTexts Name { get; set; }

        public override string ToString()
        {
            return string.Format("{0}({1})",Name.Text,ID);
        }
    }
}
