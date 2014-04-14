using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UE.Core;
using UE.Core.Architecture;
using UE.Core.Architecture.Messages;
using UE.Core.Entities;

namespace UE.NUnit
{

    [TestFixture]
    public class WorkshopTest : BaseFixedDiceTest
    {
        public WorkshopTest()
        {

        }

        [SetUp]
        public void Initialize()
        {
            (fixedDice as FixedDice).FixedDie.Clear();
            GameState.ResetState(GameEngine.GameDefinition);
        }

        [Test]
        public void ActivateConstruct()
        {
            int CurrentHP = GameEngine.GameState.CurrentHitPoint;
            RegionState rs = GameEngine.GetRegionStateFor(1);
            ConstructState cs = GameEngine.FindConstruct(rs, false);
            ActivationResult ar = GameEngine.WorkToActivate(cs, 1, 6);
            Assert.IsNull(ar.CurrentColumnValue);
            ar = GameEngine.WorkToActivate(cs, 2, 5);
            Assert.IsNull(ar.CurrentColumnValue);
            ar = GameEngine.WorkToActivate(cs, 5, 1);
            Assert.IsNotNull(ar.CurrentColumnValue);
            Assert.AreEqual(2, ar.CurrentColumnValue);
            Assert.AreEqual(2, ar.EnergyPoints);
            ar = GameEngine.WorkToActivate(cs, 6, 1);
            Assert.IsNotNull(ar.CurrentColumnValue);
            Assert.AreEqual(1, ar.CurrentColumnValue);
            Assert.AreEqual(3, ar.EnergyPoints);
            ar = GameEngine.WorkToActivate(cs, 3, 1);
            Assert.IsNull(ar.CurrentColumnValue);
            Assert.IsFalse(cs.CurrentActivationTable.Columns[2].IsEmpty);
            ar = GameEngine.WorkToActivate(cs, 7, 1);
            Assert.IsNotNull(ar.CurrentColumnValue);
            Assert.AreEqual(0, ar.CurrentColumnValue);
            Assert.IsTrue(cs.CurrentActivationTable.Columns[2].IsEmpty);
            ar = GameEngine.WorkToActivate(cs, 3, 1);
            Assert.IsNull(ar.CurrentColumnValue);
            Assert.AreEqual(GameState.CurrentHitPoint, CurrentHP);
            ar = GameEngine.WorkToActivate(cs, 7, 2);
            Assert.AreEqual(3, ar.EnergyPoints);
            Assert.IsNotNull(ar.CurrentColumnValue);
            Assert.AreEqual(-1, ar.CurrentColumnValue);
            Assert.AreEqual(GameState.CurrentHitPoint, CurrentHP - 1);
            ar = GameEngine.WorkToActivate(cs, 4, 5);
            ar = GameEngine.WorkToActivate(cs, 8, 1);
            Assert.AreEqual(4, ar.EnergyPoints);
            Assert.AreEqual(0, ar.DaysPassed);
            Assert.AreEqual(true, ar.IsFieldFilled);
            Assert.AreEqual(true, ar.IsConstructActivated);
        }

        [Test]
        public void ActivateConstructOneDays()
        {
            RegionState rs = GameEngine.GetRegionStateFor(1);
            ConstructState cs = GameEngine.FindConstruct(rs, false);
            Queue<int> die = new Queue<int>(new List<int> { 3, 5, 5, 5, 1, 1, 1, 1, 5, 5, 5, 5, 1, 1, 1, 1 });
            ActivationResult ar = null;
            for (int i = 1; i <= 8; i++)
            {
                ar = GameEngine.WorkToActivate(cs, i, die.Dequeue());
            }
            Assert.IsFalse(ar.IsConstructActivated);
            Assert.IsTrue(ar.IsFieldFilled);
            Assert.AreEqual(1, ar.DaysPassed);
            for (int i = 1; i <= 8; i++)
            {
                ar = GameEngine.WorkToActivate(cs, i, die.Dequeue());
            }
            Assert.AreEqual(4, ar.EnergyPoints);
            Assert.IsTrue(ar.IsConstructActivated);
            Assert.IsTrue(ar.IsFieldFilled);
            Assert.AreEqual(0, ar.DaysPassed);
        }


        [Test]
        public void ActivateConstructTwoDays()
        {
            AddResults(2, 2, 2, 2);
            RegionState rs = GameEngine.GetRegionStateFor(1);
            ConstructState cs = GameEngine.FindConstruct(rs, false);
            Queue<int> die = new Queue<int>(new List<int> { 3, 5, 5, 5, 1, 1, 1, 1, 5, 3, 5, 5, 1, 1, 1, 1 });
            ActivationResult ar = null;
            for (int i = 1; i <= 8; i++)
            {
                ar = GameEngine.WorkToActivate(cs, i, die.Dequeue());
            }
            Assert.AreEqual(1, ar.DaysPassed);
            for (int i = 1; i <= 8; i++)
            {
                ar = GameEngine.WorkToActivate(cs, i, die.Dequeue());
            }
            Assert.AreEqual(3, ar.EnergyPoints);
            Assert.IsTrue(ar.IsConstructActivated);
            Assert.IsTrue(ar.IsFieldFilled);
            Assert.AreEqual(1, ar.DaysPassed);
        }

        [Test]
        public void ActivateConstructHelpsGodsHand()
        {
            AddResults(2, 2, 2, 2);
            Assert.AreEqual(0, GameState.GodsHandEnergy);
            RegionState rs = GameEngine.GetRegionStateFor(1);
            ConstructState cs = GameEngine.FindConstruct(rs, false);
            Queue<int> die = new Queue<int>(new List<int> { 6, 6, 5, 5, 1, 1, 1, 1 });
            ActivationResult ar = null;
            for (int i = 1; i <= 8; i++)
            {
                ar = GameEngine.WorkToActivate(cs, i, die.Dequeue());
            }
            Assert.AreEqual(0, ar.DaysPassed);
            Assert.AreEqual(6, ar.EnergyPoints);
            Assert.IsTrue(ar.IsConstructActivated);
            Assert.AreEqual(2, GameState.GodsHandEnergy);
        }

        [Test]
        public void FocusCharmHelpsActivation()
        {
            RegionState rs = GameEngine.GetRegionStateFor(1);
            ConstructState cs = GameEngine.FindConstruct(rs, false);
            GameEngine.UseFocusCharm(1);
            ActivationResult ar = GameEngine.WorkToActivate(cs, 1, 1);
            Assert.AreEqual(2, ar.EnergyPoints);

        }

        [Test]
        public void CanLinkIfActivatedAndComponentAvailable()
        {
            ActivateTwoConstructs();
            GameEngine.AddComponent(-1, 4);
            Assert.AreEqual(0, GameState.PossibleLinks.Count());
            Assert.AreEqual(0, GameState.ConnectedLinks.Count());
        }

        [Test]
        public void CanLinkSuccesfully()
        {
            LinkState ls = ActivateTwoConstructs();
            CreateConnection(ls);
            Assert.AreEqual(0, GameState.NumberOfComponents(ls.Link.ComponentID));
            LinkResult lr = GameEngine.WorkToLink(ls.ID, 6, 1,false);
            Assert.AreEqual(1, lr.LinkBox);
            Assert.AreEqual(false, lr.HasFailed);
            Assert.AreEqual(true, lr.IsLinkFinished);
            Assert.AreEqual(0, lr.ComponentLost);
            Assert.AreEqual(0, lr.HitPointLost);
            Assert.AreEqual(0, GameState.NumberOfComponents(ls.Link.ComponentID));
            Assert.AreEqual(true, lr.Connected);
            Assert.AreEqual(1,GameState.ConnectedLinks.Count());
        }

        [Test]
        public void LinkButFailedOneColumn()
        {
            LinkState ls = ActivateTwoConstructs();
            CreateConnection(ls);
            GameEngine.AddComponent(1, ls.Link.ComponentID);
            LinkResult lr = GameEngine.WorkToLink(ls.ID, 6, 3,false);
            Assert.AreEqual(2, lr.LinkBox,"Invalid LinkBox result");
            Assert.AreEqual(false, lr.HasFailed);
            Assert.AreEqual(true, lr.IsLinkFinished);
            Assert.AreEqual(1, lr.ComponentLost);
            Assert.AreEqual(1, lr.HitPointLost);
            Assert.AreEqual(5, GameState.CurrentHitPoint);
            Assert.AreEqual(0, GameState.NumberOfComponents(ls.Link.ComponentID));
            Assert.AreEqual(true,lr.Connected);
            Assert.AreEqual(1, GameState.ConnectedLinks.Count());
        }

        [Test]
        public void LinkFailed()
        {
            LinkState ls = ActivateTwoConstructs();
            CreateConnection(ls);
            LinkResult lr = GameEngine.WorkToLink(ls.ID, 6, 3,false);
            Assert.AreEqual(0, lr.LinkBox, "Invalid LinkBox result");
            Assert.AreEqual(true, lr.HasFailed);
            Assert.AreEqual(true, lr.IsLinkFinished);
            Assert.AreEqual(0, lr.ComponentLost);
            Assert.AreEqual(1, lr.HitPointLost);
            Assert.AreEqual(5, GameState.CurrentHitPoint);
            Assert.AreEqual(0, GameState.NumberOfComponents(ls.Link.ComponentID));
            Assert.AreEqual(false, lr.Connected);
            Assert.AreEqual(0, GameState.ConnectedLinks.Count());
            Assert.IsTrue(ls.Connection.IsEmpty);
        }

        [Test]
        public void WinTheGame()
        {
            ActivateAllConstructs();
            foreach (LinkState ls in GameState.LinkStates)
            {
                CreateConnection(ls,6);
            }
            Assert.AreEqual(6,GameState.ConnectedLinks.Count());
            Assert.IsTrue(GameEngine.IsFinalActivationPossible);
            AddResults(1,2);
            FinalActivationResult t = GameEngine.WorkForfinalActivation();
            Assert.IsFalse(t.GameLost);
            Assert.IsTrue(t.GameWon);
            Assert.IsTrue(GameEngine.IsGameWon);
        }

        [Test]
        public void ReduceFinalActivationDifficulty()
        {

            CreateImpossibleFinalActivation();
            GameEngine.SpendHPToReduceActivationDifficulty(6);
            Assert.AreEqual(30, GameState.FinalActivationDifficulty);
        }

        private void CreateImpossibleFinalActivation()
        {
            ActivateAllConstructs();
            foreach (LinkState ls in GameState.LinkStates)
            {
                CreateConnection(ls, 6);
                foreach (Column col in ls.Connection.Columns)
                {
                    col.ForcedValue = 2;
                }
            }
            Assert.AreEqual(36, GameState.FinalActivationDifficulty);
        }

        [Test]
        public void DieWhileActivating()
        {
           CreateImpossibleFinalActivation();
            GameState.CurrentHitPoint = 0;
            AddResults(5,5);
            FinalActivationResult far = GameEngine.WorkForfinalActivation();
            Assert.IsTrue(far.GameLost);
            Assert.IsFalse(far.GameWon);
            Assert.IsTrue(GameEngine.IsFinished);
            Assert.IsTrue(GameEngine.IsGameLost);
            Assert.AreEqual(-1,GameState.CurrentHitPoint);
        }

        [Test]
        public void LoseBecauseNotEnoughTimeToActivate()
        {
            CreateImpossibleFinalActivation();
            GameState.CurrentDay = GameEngine.GameDefinition.FirstTheoricalLosingDay - 2;
            AddResults(5, 5,5,5,1,2,3,4);
            FinalActivationResult far = GameEngine.WorkForfinalActivation();
            Assert.IsFalse(far.GameLost);
            Assert.IsFalse(far.GameWon);
            far = GameEngine.WorkForfinalActivation();
            Assert.IsTrue(GameEngine.IsGameLost);
            Assert.IsTrue(far.GameLost);
            Assert.IsTrue(GameEngine.IsFinished);
            Assert.IsFalse(far.GameWon);
        }

    }

}
