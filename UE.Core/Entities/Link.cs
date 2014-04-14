using System;
using System.Xml.Serialization;

namespace UE.Core.Entities
{
    public class Link
    {
        [XmlAttribute]
        public int ID { get; set; }

        [XmlAttribute]
        public int ComponentID { get; set; }

        [XmlAttribute]
        public string Constructs { get; set; }

        public int ConstructID1 { get { return Convert.ToInt32(Constructs.Split(',')[0]); } }

        public int ConstructID2 { get { return Convert.ToInt32(Constructs.Split(',')[1]); } }
    }
}
