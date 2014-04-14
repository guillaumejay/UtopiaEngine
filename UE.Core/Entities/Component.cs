using System.Xml.Serialization;
using UE.Core.Architecture;

namespace UE.Core.Entities
{
    public class Component
    {
        [XmlAttribute]
        public int ID { get; set; }

        [XmlAttribute]
        public int IndexRegion { get; set; }

        public LocalizedTexts Name { get; set; }


    }
}
