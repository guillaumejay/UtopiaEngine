using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace UE.Core.Entities
{
    public class Inventory
    {
        /// <summary>
        /// For serialization
        /// </summary>
        public Inventory()
        {
        }

        public Inventory(IEnumerable<Component> components)
        {
            Stores = new List<Store>();
            foreach (Component c in components)
            {
                Stores.Add(new Store { ComponentId = c.ID, Quantity = 0 });
            }
            Components = components;
            Reset();
        }

        [XmlIgnore]
        public IEnumerable<Component> Components { get; set; }

        public List<Store> Stores { get; set; }

        public bool DowsingRodCharged { get; set; }

        public bool ParalysisWandCharged { get; set; }

        public bool FocusCharmCharged { get; set; }

        internal void Reset()
        {
            DowsingRodCharged = true;
            ParalysisWandCharged = true;
            FocusCharmCharged = true;
            foreach (Store store in Stores)
            {
                store.Quantity = 0;
            }
        }



        public override string ToString()
        {
            string tmp = String.Format("DowsingRod:{0},ParalysisWand:{1},FocusCharm:{2}" + Environment.NewLine, DowsingRodCharged,
                                       ParalysisWandCharged, FocusCharmCharged);
            foreach (Component c in Components)
            {
                tmp += String.Format("{0}:{1} ", c.Name.Text, Stores.Single(x => x.ComponentId == c.ID).Quantity);

            }
            return tmp.Trim();
        }

        internal void AddToStore(int componentId, int quantity, int maxQuantity)
        {
            Store st = Stores.Single(x => x.ComponentId == componentId);
            if (quantity <0 && quantity > st.Quantity)
            {
                throw new InvalidOperationException(String.Format("Trying to substract {0} from {1} for component {2}", quantity, st.Quantity, componentId));
            }
            st.Quantity += quantity;
            if (st.Quantity > maxQuantity)
                st.Quantity = maxQuantity;
            if (st.Quantity < 0)
            {
                st.Quantity = 0;
            }
        }

        internal int Score()
        {
            int score = 0;
            if (DowsingRodCharged)
                score += 10;
            if (ParalysisWandCharged)
                score += 10;
            if (FocusCharmCharged)
                score += 10;
            return score;
        }

        internal int GetComponentQuantityFor(int id)
        {
                return Stores.Single(x=>x.ComponentId==id).Quantity ;
        }
    }
}
