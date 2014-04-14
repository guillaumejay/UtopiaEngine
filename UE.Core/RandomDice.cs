using System;
using UE.Core.Interfaces;

namespace UE.Core
{
    public class RandomDice : IDiceRoller
    {
        private Random _rnd;
        public RandomDice()
        {
            _rnd = new Random();
        }

        public RandomDice(int seed)
        {
            _rnd = new Random(seed);
        }

        public int Get1d6()
        {
            return _rnd.Next(1, 7);
        }

        public TwoDice Get2d6()
        {
            TwoDice dice = new TwoDice(Get1d6(), Get1d6());
            return dice;
        }

        public int GetVariableDice(int nbSide)
        {
            return _rnd.Next(1, nbSide + 1);
        }
    }
}
