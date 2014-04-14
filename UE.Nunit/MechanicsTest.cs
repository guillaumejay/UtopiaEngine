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
            Assert.AreEqual(3,t.Columns.Count);
            Assert.IsTrue(t.IsEmpty);
            Assert.AreEqual(6, t.UnfilledCellCount);
            t.Columns[0].Top = 5;
            Assert.IsFalse(t.IsEmpty);
            Assert.IsFalse(t.Columns[0].IsEmpty);
            Assert.IsTrue(t.Columns[1].IsEmpty);
           Assert.AreEqual(5,t.UnfilledCellCount);
            Assert.AreEqual("5",t[0]);
           t.Clear();
           Assert.IsTrue(t.IsEmpty);
        }
        [Test]
        public void ColumnFull()
        {
            Table t = new Table(3);
            Assert.AreEqual(3, t.Columns.Count);
            Assert.IsTrue(t.IsEmpty);
            t.Columns[0].Top = 5;
            Assert.IsFalse(t.IsEmpty);
            Assert.IsFalse(t.Columns[0].IsFull);
            t.Columns[0].Bottom = 5;
            Assert.IsTrue(t.Columns[0].IsFull);
        }

        [Test]
        public void ColumnWithForcedValue()
        {
            Column c = new Column(3, 2);
            Assert.AreEqual(0, c.ForcedValue);
            Assert.AreEqual(1, c.ActivationValue);
            c.ForcedValue = 2;
            Assert.AreEqual(2, c.ForcedValue);
        }
        [Test]
        public void EncounterLevelTest()
        {
            Assert.AreEqual(1,GameEngine.EncounterLevel(100));
            Assert.AreEqual(1, GameEngine.EncounterLevel(-100));
            Assert.AreEqual(2, GameEngine.EncounterLevel(200));
            Assert.AreEqual(2, GameEngine.EncounterLevel(-101));
            Assert.AreEqual(3, GameEngine.EncounterLevel(300));
            Assert.AreEqual(3, GameEngine.EncounterLevel(-201));
            Assert.AreEqual(4, GameEngine.EncounterLevel(400));
            Assert.AreEqual(4, GameEngine.EncounterLevel(429));
            Assert.AreEqual(4, GameEngine.EncounterLevel(-301));
            Assert.AreEqual(5, GameEngine.EncounterLevel(500));
            Assert.AreEqual(5, GameEngine.EncounterLevel(-401));
        }

        [Test]
        public void PlaceNumberOnTable()
        {
            Table t = new Table(3);
            t.PlaceNumber(1, 6);
            Assert.IsFalse(t.IsEmpty);
            Assert.IsTrue(t.IsStarted);
            Assert.IsFalse(t.IsFull);
            Assert.AreEqual(5, t.UnfilledCellCount);
            Assert.AreEqual(6,t.Columns[0].Top);
            t.PlaceNumber(2, 5);
            Assert.AreEqual(5, t.Columns[1].Top);
            t.PlaceNumber(3, 4);
            Assert.AreEqual(4, t.Columns[2].Top);
            Assert.AreEqual("4", t[2]);
            t.PlaceNumber(4, 3);
            Assert.IsTrue(t.IsStarted);
            Assert.IsFalse(t.IsFull);
            Assert.AreEqual(3, t.Columns[0].Bottom);
            Assert.AreEqual("3", t[3]);
            t.PlaceNumber(5, 2);
            Assert.AreEqual(2, t.Columns[1].Bottom);
            t.PlaceNumber(6, 1);
            Assert.AreEqual(1, t.Columns[2].Bottom);
            Assert.IsFalse(t.IsEmpty);
            Assert.IsTrue(t.IsStarted);
            Assert.IsTrue(t.IsFull);
        }

        [Test]
        public void ActivationTable()
        {
            Table state = new Table(4);
            state.PlaceNumber(1, 6);
            state[4] = "1";
            Assert.AreEqual(2, state.Columns[0].EnergyPointValue);
            state.PlaceNumber(1, 5);
            state.PlaceNumber(5, 1);
            Assert.AreEqual(1, state.Columns[0].EnergyPointValue);
            state.PlaceNumber(1, 6);
            state.PlaceNumber(5, 6);
            Assert.AreEqual(0, state.Columns[0].EnergyPointValue);
            state.PlaceNumber(1, 6);
            state.PlaceNumber(5,5 );
            Assert.AreEqual(-1, state.Columns[0].EnergyPointValue);

        }
    }
}
