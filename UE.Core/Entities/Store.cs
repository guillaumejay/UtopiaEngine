using System.Xml.Serialization;

namespace UE.Core.Entities
{
    public class Store
    {
        [XmlAttribute]
        public int ComponentId { get; set; }
        [XmlAttribute]
        public int Quantity { get; set; }
    }
}
