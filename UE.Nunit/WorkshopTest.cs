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
            ClassicAssert.IsNull(ar.CurrentColumnValue);
            ar = GameEngine.WorkToActivate(cs, 2, 5);
            ClassicAssert.IsNull(ar.CurrentColumnValue);
            ar = GameEngine.WorkToActivate(cs, 5, 1);
            ClassicAssert.IsNotNull(ar.CurrentColumnValue);
            ClassicAssert.AreEqual(2, ar.CurrentColumnValue);
            ClassicAssert.AreEqual(2, ar.EnergyPoints);
            ar = GameEngine.WorkToActivate(cs, 6, 1);
            ClassicAssert.IsNotNull(ar.CurrentColumnValue);
            ClassicAssert.AreEqual(1, ar.CurrentColumnValue);
            ClassicAssert.AreEqual(3, ar.EnergyPoints);
            ar = GameEngine.WorkToActivate(cs, 3, 1);
            ClassicAssert.IsNull(ar.CurrentColumnValue);
            ClassicAssert.IsFalse(cs.CurrentActivationTable.Columns[2].IsEmpty);
            ar = GameEngine.WorkToActivate(cs, 7, 1);
            ClassicAssert.IsNotNull(ar.CurrentColumnValue);
            ClassicAssert.AreEqual(0, ar.CurrentColumnValue);
            ClassicAssert.IsTrue(cs.CurrentActivationTable.Columns[2].IsEmpty);
            ar = GameEngine.WorkToActivate(cs, 3, 1);
            ClassicAssert.IsNull(ar.CurrentColumnValue);
            ClassicAssert.AreEqual(GameState.CurrentHitPoint, CurrentHP);
            ar = GameEngine.WorkToActivate(cs, 7, 2);
            ClassicAssert.AreEqual(3, ar.EnergyPoints);
            ClassicAssert.IsNotNull(ar.CurrentColumnValue);
            ClassicAssert.AreEqual(-1, ar.CurrentColumnValue);
            ClassicAssert.AreEqual(GameState.CurrentHitPoint, CurrentHP - 1);
            ar = GameEngine.WorkToActivate(cs, 4, 5);
            ar = GameEngine.WorkToActivate(cs, 8, 1);
            ClassicAssert.AreEqual(4, ar.EnergyPoints);
            ClassicAssert.AreEqual(0, ar.DaysPassed);
            ClassicAssert.AreEqual(true, ar.IsFieldFilled);
            ClassicAssert.AreEqual(true, ar.IsConstructActivated);
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
            ClassicAssert.IsFalse(ar.IsConstructActivated);
            ClassicAssert.IsTrue(ar.IsFieldFilled);
            ClassicAssert.AreEqual(1, ar.DaysPassed);
            for (int i = 1; i <= 8; i++)
            {
                ar = GameEngine.WorkToActivate(cs, i, die.Dequeue());
            }
            ClassicAssert.AreEqual(4, ar.EnergyPoints);
            ClassicAssert.IsTrue(ar.IsConstructActivated);
            ClassicAssert.IsTrue(ar.IsFieldFilled);
            ClassicAssert.AreEqual(0, ar.DaysPassed);
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
            ClassicAssert.AreEqual(1, ar.DaysPassed);
            for (int i = 1; i <= 8; i++)
            {
                ar = GameEngine.WorkToActivate(cs, i, die.Dequeue());
            }
            ClassicAssert.AreEqual(3, ar.EnergyPoints);
            ClassicAssert.IsTrue(ar.IsConstructActivated);
            ClassicAssert.IsTrue(ar.IsFieldFilled);
            ClassicAssert.AreEqual(1, ar.DaysPassed);
        }

        [Test]
        public void ActivateConstructHelpsGodsHand()
        {
            AddResults(2, 2, 2, 2);
            ClassicAssert.AreEqual(0, GameState.GodsHandEnergy);
            RegionState rs = GameEngine.GetRegionStateFor(1);
            ConstructState cs = GameEngine.FindConstruct(rs, false);
            Queue<int> die = new Queue<int>(new List<int> { 6, 6, 5, 5, 1, 1, 1, 1 });
            ActivationResult ar = null;
            for (int i = 1; i <= 8; i++)
            {
                ar = GameEngine.WorkToActivate(cs, i, die.Dequeue());
            }
            ClassicAssert.AreEqual(0, ar.DaysPassed);
            ClassicAssert.AreEqual(6, ar.EnergyPoints);
            ClassicAssert.IsTrue(ar.IsConstructActivated);
            ClassicAssert.AreEqual(2, GameState.GodsHandEnergy);
        }

        [Test]
        public void FocusCharmHelpsActivation()
        {
            RegionState rs = GameEngine.GetRegionStateFor(1);
            ConstructState cs = GameEngine.FindConstruct(rs, false);
            GameEngine.UseFocusCharm(1);
            ActivationResult ar = GameEngine.WorkToActivate(cs, 1, 1);
            ClassicAssert.AreEqual(2, ar.EnergyPoints);

        }

        [Test]
        public void CanLinkIfActivatedAndComponentAvailable()
        {
            ActivateTwoConstructs();
            GameEngine.AddComponent(-1, 4);
            ClassicAssert.AreEqual(0, GameState.PossibleLinks.Count());
            ClassicAssert.AreEqual(0, GameState.ConnectedLinks.Count());
        }

        [Test]
        public void CanLinkSuccesfully()
        {
            LinkState ls = ActivateTwoConstructs();
            CreateConnection(ls);
            ClassicAssert.AreEqual(0, GameState.NumberOfComponents(ls.Link.ComponentID));
            LinkResult lr = GameEngine.WorkToLink(ls.ID, 6, 1,false);
            ClassicAssert.AreEqual(1, lr.LinkBox);
            ClassicAssert.AreEqual(false, lr.HasFailed);
            ClassicAssert.AreEqual(true, lr.IsLinkFinished);
            ClassicAssert.AreEqual(0, lr.ComponentLost);
            ClassicAssert.AreEqual(0, lr.HitPointLost);
            ClassicAssert.AreEqual(0, GameState.NumberOfComponents(ls.Link.ComponentID));
            ClassicAssert.AreEqual(true, lr.Connected);
            ClassicAssert.AreEqual(1,GameState.ConnectedLinks.Count());
        }

        [Test]
        public void LinkButFailedOneColumn()
        {
            LinkState ls = ActivateTwoConstructs();
            CreateConnection(ls);
            GameEngine.AddComponent(1, ls.Link.ComponentID);
            LinkResult lr = GameEngine.WorkToLink(ls.ID, 6, 3,false);
            ClassicAssert.AreEqual(2, lr.LinkBox,"Invalid LinkBox result");
            ClassicAssert.AreEqual(false, lr.HasFailed);
            ClassicAssert.AreEqual(true, lr.IsLinkFinished);
            ClassicAssert.AreEqual(1, lr.ComponentLost);
            ClassicAssert.AreEqual(1, lr.HitPointLost);
            ClassicAssert.AreEqual(5, GameState.CurrentHitPoint);
            ClassicAssert.AreEqual(0, GameState.NumberOfComponents(ls.Link.ComponentID));
            ClassicAssert.AreEqual(true,lr.Connected);
            ClassicAssert.AreEqual(1, GameState.ConnectedLinks.Count());
        }

        [Test]
        public void LinkFailed()
        {
            LinkState ls = ActivateTwoConstructs();
            CreateConnection(ls);
            LinkResult lr = GameEngine.WorkToLink(ls.ID, 6, 3,false);
            ClassicAssert.AreEqual(0, lr.LinkBox, "Invalid LinkBox result");
            ClassicAssert.AreEqual(true, lr.HasFailed);
            ClassicAssert.AreEqual(true, lr.IsLinkFinished);
            ClassicAssert.AreEqual(0, lr.ComponentLost);
            ClassicAssert.AreEqual(1, lr.HitPointLost);
            ClassicAssert.AreEqual(5, GameState.CurrentHitPoint);
            ClassicAssert.AreEqual(0, GameState.NumberOfComponents(ls.Link.ComponentID));
            ClassicAssert.AreEqual(false, lr.Connected);
            ClassicAssert.AreEqual(0, GameState.ConnectedLinks.Count());
            ClassicAssert.IsTrue(ls.Connection.IsEmpty);
        }

        [Test]
        public void WinTheGame()
        {
            ActivateAllConstructs();
            foreach (LinkState ls in GameState.LinkStates)
            {
                CreateConnection(ls,6);
            }
            ClassicAssert.AreEqual(6,GameState.ConnectedLinks.Count());
            ClassicAssert.IsTrue(GameEngine.IsFinalActivationPossible);
            AddResults(1,2);
            FinalActivationResult t = GameEngine.WorkForfinalActivation();
            ClassicAssert.IsFalse(t.GameLost);
            ClassicAssert.IsTrue(t.GameWon);
            ClassicAssert.IsTrue(GameEngine.IsGameWon);
        }

        [Test]
        public void ReduceFinalActivationDifficulty()
        {

            CreateImpossibleFinalActivation();
            GameEngine.SpendHPToReduceActivationDifficulty(6);
            ClassicAssert.AreEqual(30, GameState.FinalActivationDifficulty);
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
            ClassicAssert.AreEqual(36, GameState.FinalActivationDifficulty);
        }

        [Test]
        public void DieWhileActivating()
        {
           CreateImpossibleFinalActivation();
            GameState.CurrentHitPoint = 0;
            AddResults(5,5);
            FinalActivationResult far = GameEngine.WorkForfinalActivation();
            ClassicAssert.IsTrue(far.GameLost);
            ClassicAssert.IsFalse(far.GameWon);
            ClassicAssert.IsTrue(GameEngine.IsFinished);
            ClassicAssert.IsTrue(GameEngine.IsGameLost);
            ClassicAssert.AreEqual(-1,GameState.CurrentHitPoint);
        }

        [Test]
        public void LoseBecauseNotEnoughTimeToActivate()
        {
            CreateImpossibleFinalActivation();
            GameState.CurrentDay = GameEngine.GameDefinition.FirstTheoricalLosingDay - 2;
            AddResults(5, 5,5,5,1,2,3,4);
            FinalActivationResult far = GameEngine.WorkForfinalActivation();
            ClassicAssert.IsFalse(far.GameLost);
            ClassicAssert.IsFalse(far.GameWon);
            far = GameEngine.WorkForfinalActivation();
            ClassicAssert.IsTrue(GameEngine.IsGameLost);
            ClassicAssert.IsTrue(far.GameLost);
            ClassicAssert.IsTrue(GameEngine.IsFinished);
            ClassicAssert.IsFalse(far.GameWon);
        }

    }

}
