using System.Linq;
using System.Collections.Generic;
using Engine_Test;
using NUnit.Framework;
using UE.Core;
using UE.Core.Interfaces;

namespace UE.NUnit
{
    [TestFixture]
    public class BaseFixedDiceTest:BaseEngineTest
    {
        protected FixedDice fixedDice;

        protected override IDiceRoller GetDiceRoller()
        {
            fixedDice = new FixedDice(new Queue<int>());
            return fixedDice;
        }

        /// <summary>
        /// automatically clear queue before adding
        /// </summary>
        /// <param name="list"></param>
        protected void AddResults(params int[] list)
        {
            fixedDice.FixedDie.Clear();
            for (int i = 0; i < list.Count(); i++)
            {
                (fixedDice as FixedDice).FixedDie.Enqueue(list[i]);
            }
        }
    }
}
