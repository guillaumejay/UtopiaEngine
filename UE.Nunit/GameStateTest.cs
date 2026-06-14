using System;
using System.Linq;
using NUnit.Framework;
using UE.NUnit;


namespace Engine_Test
{
    [TestFixture]
    public class GameStateTest:BaseEngineTest
    {

        [SetUp]
        public void Initialize()
        {
               GameState.ResetState(GameEngine.GameDefinition);
        }
        [Test]
        public void StartOfGame()
        {

            ClassicAssert.IsFalse(GameEngine.IsGameLost);

            ClassicAssert.AreEqual(GameEngine.GameDefinition.MaxHitPoint, GameState.CurrentHitPoint);
            ClassicAssert.AreEqual(0, GameState.CurrentDay);
            ClassicAssert.AreEqual(0, GameState.NumberOfSkullsCrossed);
            ClassicAssert.AreEqual(6, GameState.RegionStates.Count);
            ClassicAssert.AreEqual(6, GameState.Constructs.Count);
            ClassicAssert.IsTrue(GameState.Inventory.DowsingRodCharged);
            ClassicAssert.IsTrue(GameState.Inventory.FocusCharmCharged);
            ClassicAssert.IsTrue(GameState.Inventory.ParalysisWandCharged);
            ClassicAssert.IsTrue(GameState.RegionStates.All(x => x.RemainingSearchBoxes == 6));
            ClassicAssert.IsNull(GameState.LastPlayed);
            AssertGameStateIsCorrectlyHydrated();
          ClassicAssert.AreEqual(15,GameEngine.DaysRemaining);
            ClassicAssert.IsTrue(GameState.Inventory.Stores.All(x=>x.Quantity==0));
        }

        [Test]
        public void LostIfHitPointTooLow()
        {
            GameState.CurrentHitPoint = 0;
            ClassicAssert.IsFalse(GameEngine.IsGameLost);
            ClassicAssert.IsFalse(GameEngine.IsFinished);
            GameState.CurrentHitPoint = -1;
            ClassicAssert.IsTrue(GameEngine.IsGameLost);
        }

        [Test]
        public void LostBecauseOfTime()
        {
            GameState.CurrentDay = 14;
            ClassicAssert.IsFalse(GameEngine.IsGameLost);
            GameState.CurrentDay = 1000;
            ClassicAssert.IsTrue(GameEngine.IsGameLost);
        }

        [Test]
        public void MaximumTimeAvailable()
        {
            GameState.CurrentDay = GameEngine.GameDefinition.FirstTheoricalLosingDay;
            ClassicAssert.AreEqual(15, GameState.CurrentDay);
            ClassicAssert.IsTrue(GameEngine.IsGameLost);
            GameState.NumberOfSkullsCrossed = 1;
            ClassicAssert.IsFalse(GameEngine.IsGameLost);
        }

        [Test]
        public void LostBecauseOfTimeWithSkullCrossed()
        {         
            GameState.NumberOfSkullsCrossed = GameEngine.GameDefinition.NumberOfSkulls;
            GameState.CurrentDay = GameEngine.GameDefinition.MaximumNumberOfDays;
            ClassicAssert.IsTrue(GameEngine.IsGameLost);
            GameState.CurrentDay = GameEngine.GameDefinition.MaximumNumberOfDays - 1;
            ClassicAssert.IsFalse(GameEngine.IsGameLost);
            GameState.NumberOfSkullsCrossed = GameEngine.GameDefinition.NumberOfSkulls - 1;
            ClassicAssert.IsFalse(GameEngine.IsGameLost);
        }

        [Test]
        public void LostBecauseOfTimeSpecificDays()
        {
            GameState.CurrentDay = 15;
            ClassicAssert.IsTrue(GameEngine.IsGameLost);
            GameState.NumberOfSkullsCrossed = 1;
            ClassicAssert.IsFalse(GameEngine.IsGameLost);
            GameState.CurrentDay = 21;
            ClassicAssert.IsTrue(GameEngine.IsGameLost);
            GameState.NumberOfSkullsCrossed = 7;
            ClassicAssert.IsFalse(GameEngine.IsGameLost);

        }


        [Test]
        public void CanUseDowsingRod()
        {
            int result = 100;
            ClassicAssert.IsTrue(GameEngine.CanModifySearchResult(result,1).CanModify);
            GameState.Inventory.DowsingRodCharged = false;
			ClassicAssert.IsFalse(GameEngine.CanModifySearchResult(result,1).CanModify);
            GameState.Inventory.DowsingRodCharged = true;
            result = 1;
			ClassicAssert.IsFalse(GameEngine.CanModifySearchResult(result,1).CanModify);
        }
    }
}
