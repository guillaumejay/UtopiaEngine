using System.Xml.Serialization;
using UE.Core.Architecture;

namespace UE.Core.Entities
{
    public class ConstructState
    {
      /// <summary>
        /// For Serialization
        /// </summary>
        public ConstructState()
        {
            Reset();
        }
        public ConstructState(Construct c)
        {
            ID = c.ID;
            Construct = c;
            Reset();
        }

        private void Reset()
        {
            ActivationTable1 = new Table(4);
            ActivationTable2 = new Table(4);
            HasBeenFound = false;
            HasBeenActivated = false;
            Used = false;
        }

        [XmlAttribute]
        public int ID { get; set; }

        [XmlIgnore]
        public Construct Construct { get; set; }

        [XmlAttribute]
        public bool Used { get; set; }

        public bool AbilityUsable
        {
            get { return HasBeenActivated && (!Construct.OnceAGame || !Used); }
        }

        public Table CurrentActivationTable { get { return (ActivationTable1.IsFull) ? ActivationTable2 : ActivationTable1; } }

        public Table ActivationTable1 { get; set; }
        public Table ActivationTable2 { get; set; }


        public bool HasBeenFound { get; set; }

        public bool HasBeenActivated { get; set; }

        public bool HasUsableAbility(Ability ability)
        {
            return (Construct.Ability == ability && AbilityUsable);
        }
    }
}
