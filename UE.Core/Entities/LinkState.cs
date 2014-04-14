
using System;
using System.Xml.Serialization;
using UE.Core.Architecture;

namespace UE.Core.Entities
{
    public class LinkState
    {
        public LinkState()
        {
            Reset();
        }

        public void Reset()
        {
            Connection = new Table(3);
        }

        [XmlAttribute]
        public int ID { get; set; }

        [XmlIgnore]
        public Link Link { get; set; }

        public Table Connection { get; set; }

        public bool IsLinkDone
        {
            get
            {
                return Connection.IsFull;
            }
        }
        public int LinkBox { get { return Connection.LinkResult; } }

        [XmlIgnore]
        public ConstructState Construct1 { get; set; }

        [XmlIgnore]
        public ConstructState Construct2 { get; set; }

        public string Name
        {
            get
            {
                string text= Construct1.Construct.Name.Text + " - " + Construct2.Construct.Name.Text;
                if (IsLinkDone)
                {
                    text += String.Format("({0})", LinkBox);
                }
                return text;
            }
        }

        /// <summary>
        /// Has the connection attempt started ? 
        /// (used to know if a component must be spent)
        /// </summary>
        [XmlIgnore]
        public bool IsStarted { get; set; }
    }
}
