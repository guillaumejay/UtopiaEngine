using System.Collections.Generic;

using UE.Core.Interfaces;

namespace UE.Core
{
    public class FixedDice:IDiceRoller
    {
        public Queue<int>  FixedDie { get; set; }

        public FixedDice(Queue<int> fixedDie)
        {
            FixedDie = fixedDie;
        }

        public int Get1d6()
        {
            return FixedDie.Dequeue();
        }

        public TwoDice Get2d6()
        {
            return new TwoDice(FixedDie.Dequeue(),FixedDie.Dequeue());
        }


        public int GetVariableDice(int nbSide)
        {
            return FixedDie.Dequeue();
        }
    }
}
