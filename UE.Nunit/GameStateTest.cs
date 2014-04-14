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

            Assert.IsFalse(GameEngine.IsGameLost);

            Assert.AreEqual(GameEngine.GameDefinition.MaxHitPoint, GameState.CurrentHitPoint);
            Assert.AreEqual(0, GameState.CurrentDay);
            Assert.AreEqual(0, GameState.NumberOfSkullsCrossed);
            Assert.AreEqual(6, GameState.RegionStates.Count);
            Assert.AreEqual(6, GameState.Constructs.Count);
            Assert.IsTrue(GameState.Inventory.DowsingRodCharged);
            Assert.IsTrue(GameState.Inventory.FocusCharmCharged);
            Assert.IsTrue(GameState.Inventory.ParalysisWandCharged);
            Assert.IsTrue(GameState.RegionStates.All(x => x.RemainingSearchBoxes == 6));
            Assert.IsNull(GameState.LastPlayed);
            AssertGameStateIsCorrectlyHydrated();
          Assert.AreEqual(15,GameEngine.DaysRemaining);
            Assert.IsTrue(GameState.Inventory.Stores.All(x=>x.Quantity==0));
        }

        [Test]
        public void LostIfHitPointTooLow()
        {
            GameState.CurrentHitPoint = 0;
            Assert.IsFalse(GameEngine.IsGameLost);
            Assert.IsFalse(GameEngine.IsFinished);
            GameState.CurrentHitPoint = -1;
            Assert.IsTrue(GameEngine.IsGameLost);
        }

        [Test]
        public void LostBecauseOfTime()
        {
            GameState.CurrentDay = 14;
            Assert.IsFalse(GameEngine.IsGameLost);
            GameState.CurrentDay = 1000;
            Assert.IsTrue(GameEngine.IsGameLost);
        }

        [Test]
        public void MaximumTimeAvailable()
        {
            GameState.CurrentDay = GameEngine.GameDefinition.FirstTheoricalLosingDay;
            Assert.AreEqual(15, GameState.CurrentDay);
            Assert.IsTrue(GameEngine.IsGameLost);
            GameState.NumberOfSkullsCrossed = 1;
            Assert.IsFalse(GameEngine.IsGameLost);
        }

        [Test]
        public void LostBecauseOfTimeWithSkullCrossed()
        {         
            GameState.NumberOfSkullsCrossed = GameEngine.GameDefinition.NumberOfSkulls;
            GameState.CurrentDay = GameEngine.GameDefinition.MaximumNumberOfDays;
            Assert.IsTrue(GameEngine.IsGameLost);
            GameState.CurrentDay = GameEngine.GameDefinition.MaximumNumberOfDays - 1;
            Assert.IsFalse(GameEngine.IsGameLost);
            GameState.NumberOfSkullsCrossed = GameEngine.GameDefinition.NumberOfSkulls - 1;
            Assert.IsFalse(GameEngine.IsGameLost);
        }

        [Test]
        public void LostBecauseOfTimeSpecificDays()
        {
            GameState.CurrentDay = 15;
            Assert.IsTrue(GameEngine.IsGameLost);
            GameState.NumberOfSkullsCrossed = 1;
            Assert.IsFalse(GameEngine.IsGameLost);
            GameState.CurrentDay = 21;
            Assert.IsTrue(GameEngine.IsGameLost);
            GameState.NumberOfSkullsCrossed = 7;
            Assert.IsFalse(GameEngine.IsGameLost);

        }


        [Test]
        public void CanUseDowsingRod()
        {
            int result = 100;
            Assert.IsTrue(GameEngine.CanModifySearchResult(result,1).CanModify);
            GameState.Inventory.DowsingRodCharged = false;
			Assert.IsFalse(GameEngine.CanModifySearchResult(result,1).CanModify);
            GameState.Inventory.DowsingRodCharged = true;
            result = 1;
			Assert.IsFalse(GameEngine.CanModifySearchResult(result,1).CanModify);
        }
    }
}
