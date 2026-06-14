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
            ClassicAssert.AreEqual(0, GameEngine.GameState.GodsHandEnergy);
            ClassicAssert.IsTrue(GameEngine.TreasureIsFound(GetRegion(2)));
            GameEngine.Rest(1); // Nothing should change
            ClassicAssert.AreEqual(0, GameEngine.GameState.GodsHandEnergy);
        }

		/// <summary>
		/// Test checking that the new rule is correctly implemented
		/// </summary>
        [Test]
        public void AddToGodsHandAbilityRightRuleTest()
        {
         
            ClassicAssert.AreEqual(0, GameEngine.GameState.GodsHandEnergy);
            ClassicAssert.IsTrue(GameEngine.TreasureIsFound(GetRegion(2)));
            GameEngine.FindConstruct(GameEngine.GetRegionStateFor(2),true);
			// Construct is actiaved, bracelet of Ios should work
            ClassicAssert.AreEqual(1, GameEngine.GameState.GodsHandEnergy);
        }
        [Test]
        public void AutomaticallyConnect()
        {
            ActivateAllConstructs();
            ClassicAssert.IsTrue(GameEngine.TreasureIsFound(GetRegion(5)));
            ClassicAssert.IsTrue(GameEngine.HasAbility(Ability.AutomaticallyConnect));
            LinkState ls = GameEngine.GameState.PossibleLinks.First();
            GameEngine.UseAutomaticConnect(ls);
            ClassicAssert.IsTrue(ls.IsLinkDone);
            ClassicAssert.AreEqual(2, ls.LinkBox);
            ClassicAssert.AreEqual(1, GameState.ConnectedLinks.Count());
        }

        [Test]
        public void RecoverFaster()
        {
            GameEngine.GameState.CurrentHitPoint = 0;
            TimePassed t = GameEngine.RecoverFromUnconsciousness();
            ClassicAssert.AreEqual(6, t.DaysPassed);
            GameEngine.GameState.CurrentHitPoint = 0;
            GameEngine.FindConstruct(GameEngine.GetRegionStateFor(3), true);
            t = GameEngine.RecoverFromUnconsciousness();
            ClassicAssert.AreEqual(4, t.DaysPassed);
        }

        [Test]
        public void RecoverHPTest()
        {
            GameEngine.GameState.CurrentHitPoint = 5;
            GameEngine.CrossRegionTracker(GameEngine.GetRegion(4));
            ClassicAssert.AreEqual(5, GameEngine.GameState.CurrentHitPoint);
            ClassicAssert.IsTrue(GameEngine.TreasureIsFound(GetRegion(4)));
            GameEngine.CrossRegionTracker(GameEngine.GetRegion(4));
            ClassicAssert.AreEqual(6, GameEngine.GameState.CurrentHitPoint);
        }

		[Test]
		public void ActivateConstructSearchPower()
		{
		    GameEngine.UseDowsingRod(10, 50);
			ClassicAssert.IsFalse (GameEngine.CanModifySearchResult (5, 1).CanModify);
			GameEngine.FindConstruct(GameEngine.GetRegionStateFor(2),true);
			CanSearchResult csr = GameEngine.CanModifySearchResult (5, 1);
			ClassicAssert.IsTrue (csr.CanModify);
			ClassicAssert.IsTrue (csr.CanUseConstructPower);
			ClassicAssert.IsFalse (csr.CanUseDowsingRod);
			ClassicAssert.IsFalse (csr.CanUseGoodFortune);
			ClassicAssert.IsFalse (GameEngine.CanModifySearchResult (5, 3).CanModify);
			GameEngine.FindConstruct(GameEngine.GetRegionStateFor(5),true);
			 csr = GameEngine.CanModifySearchResult (5, 3);
			ClassicAssert.IsTrue (csr.CanModify);
			ClassicAssert.IsTrue (csr.CanUseConstructPower);
			ClassicAssert.IsFalse (csr.CanUseDowsingRod);
			ClassicAssert.IsFalse (csr.CanUseGoodFortune);
		}
    }
}