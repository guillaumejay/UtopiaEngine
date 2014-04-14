using System.Xml.Serialization;
using UE.Core.Architecture;

namespace UE.Core.Entities
{
    public class Construct
    {
        [XmlAttribute]
        public int ID { get; set; }
        public LocalizedTexts Name { get; set; }

        public Ability Ability { get; set; }

        [XmlAttribute]
        public bool OnceAGame { get; set; }
    }
}
