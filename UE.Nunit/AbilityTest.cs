using NUnit.Framework;
using System.Linq;
using UE.Core.Architecture.Messages;
using UE.Core.Entities;
using UE.NUnit;
using UE.Core;

namespace Engine_Test
{
    [TestFixture]
    public class AbilityTest : BaseEngineTest
    {

        [SetUp]
        public void Initialize()
        {
              GameState.ResetState(GameEngine.GameDefinition);
        }
        /// <summary>
        /// The rulebook was false : the bracelet of Ios acts when contructs are activated, not when days are crossed
        /// </summary>
        [Test]
        public void AddToGodsHandAbilityTest()
        {
            GameEngine.Rest(1);
            Assert.AreEqual(0, GameEngine.GameState.GodsHandEnergy);
            Assert.IsTrue(GameEngine.TreasureIsFound(GetRegion(2)));
            GameEngine.Rest(1); // Nothing should change
            Assert.AreEqual(0, GameEngine.GameState.GodsHandEnergy);
        }

		/// <summary>
		/// Test checking that the new rule is correctly implemented
		/// </summary>
        [Test]
        public void AddToGodsHandAbilityRightRuleTest()
        {
         
            Assert.AreEqual(0, GameEngine.GameState.GodsHandEnergy);
            Assert.IsTrue(GameEngine.TreasureIsFound(GetRegion(2)));
            GameEngine.FindConstruct(GameEngine.GetRegionStateFor(2),true);
			// Construct is actiaved, bracelet of Ios should work
            Assert.AreEqual(1, GameEngine.GameState.GodsHandEnergy);
        }
        [Test]
        public void AutomaticallyConnect()
        {
            ActivateAllConstructs();
            Assert.IsTrue(GameEngine.TreasureIsFound(GetRegion(5)));
            Assert.IsTrue(GameEngine.HasAbility(Ability.AutomaticallyConnect));
            LinkState ls = GameEngine.GameState.PossibleLinks.First();
            GameEngine.UseAutomaticConnect(ls);
            Assert.IsTrue(ls.IsLinkDone);
            Assert.AreEqual(2, ls.LinkBox);
            Assert.AreEqual(1, GameState.ConnectedLinks.Count());
        }

        [Test]
        public void RecoverFaster()
        {
            GameEngine.GameState.CurrentHitPoint = 0;
            TimePassed t = GameEngine.RecoverFromUnconsciousness();
            Assert.AreEqual(6, t.DaysPassed);
            GameEngine.GameState.CurrentHitPoint = 0;
            GameEngine.FindConstruct(GameEngine.GetRegionStateFor(3), true);
            t = GameEngine.RecoverFromUnconsciousness();
            Assert.AreEqual(4, t.DaysPassed);
        }

        [Test]
        public void RecoverHPTest()
        {
            GameEngine.GameState.CurrentHitPoint = 5;
            GameEngine.CrossRegionTracker(GameEngine.GetRegion(4));
            Assert.AreEqual(5, GameEngine.GameState.CurrentHitPoint);
            Assert.IsTrue(GameEngine.TreasureIsFound(GetRegion(4)));
            GameEngine.CrossRegionTracker(GameEngine.GetRegion(4));
            Assert.AreEqual(6, GameEngine.GameState.CurrentHitPoint);
        }

		[Test]
		public void ActivateConstructSearchPower()
		{
		    GameEngine.UseDowsingRod(10, 50);
			Assert.IsFalse (GameEngine.CanModifySearchResult (5, 1).CanModify);
			GameEngine.FindConstruct(GameEngine.GetRegionStateFor(2),true);
			CanSearchResult csr = GameEngine.CanModifySearchResult (5, 1);
			Assert.IsTrue (csr.CanModify);
			Assert.IsTrue (csr.CanUseConstructPower);
			Assert.IsFalse (csr.CanUseDowsingRod);
			Assert.IsFalse (csr.CanUseGoodFortune);
			Assert.IsFalse (GameEngine.CanModifySearchResult (5, 3).CanModify);
			GameEngine.FindConstruct(GameEngine.GetRegionStateFor(5),true);
			 csr = GameEngine.CanModifySearchResult (5, 3);
			Assert.IsTrue (csr.CanModify);
			Assert.IsTrue (csr.CanUseConstructPower);
			Assert.IsFalse (csr.CanUseDowsingRod);
			Assert.IsFalse (csr.CanUseGoodFortune);
		}
    }
}