using NUnit.Framework;
using UE.Core;
using UE.Core.Architecture;

namespace UE.NUnit
{
    [TestFixture]
    public class MechanicsTest
    {
        [Test]
        public void TableAndColumnEmpty()
        {
            Table t=new Table(3);
            ClassicAssert.AreEqual(3,t.Columns.Count);
            ClassicAssert.IsTrue(t.IsEmpty);
            ClassicAssert.AreEqual(6, t.UnfilledCellCount);
            t.Columns[0].Top = 5;
            ClassicAssert.IsFalse(t.IsEmpty);
            ClassicAssert.IsFalse(t.Columns[0].IsEmpty);
            ClassicAssert.IsTrue(t.Columns[1].IsEmpty);
           ClassicAssert.AreEqual(5,t.UnfilledCellCount);
            ClassicAssert.AreEqual("5",t[0]);
           t.Clear();
           ClassicAssert.IsTrue(t.IsEmpty);
        }
        [Test]
        public void ColumnFull()
        {
            Table t = new Table(3);
            ClassicAssert.AreEqual(3, t.Columns.Count);
            ClassicAssert.IsTrue(t.IsEmpty);
            t.Columns[0].Top = 5;
            ClassicAssert.IsFalse(t.IsEmpty);
            ClassicAssert.IsFalse(t.Columns[0].IsFull);
            t.Columns[0].Bottom = 5;
            ClassicAssert.IsTrue(t.Columns[0].IsFull);
        }

        [Test]
        public void ColumnWithForcedValue()
        {
            Column c = new Column(3, 2);
            ClassicAssert.AreEqual(0, c.ForcedValue);
            ClassicAssert.AreEqual(1, c.ActivationValue);
            c.ForcedValue = 2;
            ClassicAssert.AreEqual(2, c.ForcedValue);
        }
        [Test]
        public void EncounterLevelTest()
        {
            ClassicAssert.AreEqual(1,GameEngine.EncounterLevel(100));
            ClassicAssert.AreEqual(1, GameEngine.EncounterLevel(-100));
            ClassicAssert.AreEqual(2, GameEngine.EncounterLevel(200));
            ClassicAssert.AreEqual(2, GameEngine.EncounterLevel(-101));
            ClassicAssert.AreEqual(3, GameEngine.EncounterLevel(300));
            ClassicAssert.AreEqual(3, GameEngine.EncounterLevel(-201));
            ClassicAssert.AreEqual(4, GameEngine.EncounterLevel(400));
            ClassicAssert.AreEqual(4, GameEngine.EncounterLevel(429));
            ClassicAssert.AreEqual(4, GameEngine.EncounterLevel(-301));
            ClassicAssert.AreEqual(5, GameEngine.EncounterLevel(500));
            ClassicAssert.AreEqual(5, GameEngine.EncounterLevel(-401));
        }

        [Test]
        public void PlaceNumberOnTable()
        {
            Table t = new Table(3);
            t.PlaceNumber(1, 6);
            ClassicAssert.IsFalse(t.IsEmpty);
            ClassicAssert.IsTrue(t.IsStarted);
            ClassicAssert.IsFalse(t.IsFull);
            ClassicAssert.AreEqual(5, t.UnfilledCellCount);
            ClassicAssert.AreEqual(6,t.Columns[0].Top);
            t.PlaceNumber(2, 5);
            ClassicAssert.AreEqual(5, t.Columns[1].Top);
            t.PlaceNumber(3, 4);
            ClassicAssert.AreEqual(4, t.Columns[2].Top);
            ClassicAssert.AreEqual("4", t[2]);
            t.PlaceNumber(4, 3);
            ClassicAssert.IsTrue(t.IsStarted);
            ClassicAssert.IsFalse(t.IsFull);
            ClassicAssert.AreEqual(3, t.Columns[0].Bottom);
            ClassicAssert.AreEqual("3", t[3]);
            t.PlaceNumber(5, 2);
            ClassicAssert.AreEqual(2, t.Columns[1].Bottom);
            t.PlaceNumber(6, 1);
            ClassicAssert.AreEqual(1, t.Columns[2].Bottom);
            ClassicAssert.IsFalse(t.IsEmpty);
            ClassicAssert.IsTrue(t.IsStarted);
            ClassicAssert.IsTrue(t.IsFull);
        }

        [Test]
        public void ActivationTable()
        {
            Table state = new Table(4);
            state.PlaceNumber(1, 6);
            state[4] = "1";
            ClassicAssert.AreEqual(2, state.Columns[0].EnergyPointValue);
            state.PlaceNumber(1, 5);
            state.PlaceNumber(5, 1);
            ClassicAssert.AreEqual(1, state.Columns[0].EnergyPointValue);
            state.PlaceNumber(1, 6);
            state.PlaceNumber(5, 6);
            ClassicAssert.AreEqual(0, state.Columns[0].EnergyPointValue);
            state.PlaceNumber(1, 6);
            state.PlaceNumber(5,5 );
            ClassicAssert.AreEqual(-1, state.Columns[0].EnergyPointValue);

        }
    }
}
