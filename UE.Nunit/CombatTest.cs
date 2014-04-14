using System.Linq;
using NUnit.Framework;
using UE.Core.Architecture.Messages;
using UE.Core.Entities;

namespace UE.NUnit
{

    [TestFixture]
    public class CombatTest : BaseFixedDiceTest
    {

        [SetUp]
        public void Initialize()
        {
            fixedDice.FixedDie.Clear();
            GameState.ResetState(GameEngine.GameDefinition);
        }

        [Test]
        public void Test1HitPointLost()
        {
            AddResults(1, 2);
            var cr = DoCombatTurn(1, 1);
            Assert.AreEqual(1, cr.DiceRoll.First);
            Assert.AreEqual(2, cr.DiceRoll.Second);
            Assert.AreEqual(1, cr.HitpointLost);
            Assert.IsFalse(cr.EncounterDead);
            Assert.IsFalse(cr.LegendaryTreasureFound);
            Assert.IsFalse(cr.ComponentFound);
            Assert.AreEqual(5, GameState.CurrentHitPoint);
        }
        [Test]
        public void Test2HitPointLost()
        {
            AddResults(1, 1);
            var cr = DoCombatTurn(1, 1);
            Assert.AreEqual(1, cr.DiceRoll.First);
            Assert.AreEqual(1, cr.DiceRoll.Second);
            Assert.AreEqual(2, cr.HitpointLost);
            Assert.IsFalse(cr.EncounterDead);
            Assert.IsFalse(cr.LegendaryTreasureFound);
            Assert.IsFalse(cr.ComponentFound);
            Assert.AreEqual(4, GameState.CurrentHitPoint);
        }

        [Test]
        public void TestHitPointLostAndEncounterDeadWithoutLoot()
        {
            AddResults(1, 5, 2);
            var cr = DoCombatTurn(1, 1);
            Assert.AreEqual(1, cr.DiceRoll.First);
            Assert.AreEqual(5, cr.DiceRoll.Second);
            Assert.AreEqual(1, cr.HitpointLost);
            Assert.IsTrue(cr.EncounterDead);
            Assert.IsFalse(cr.LegendaryTreasureFound);
            Assert.IsFalse(cr.ComponentFound);
            Assert.AreEqual(5, GameState.CurrentHitPoint);
        }

        [Test]
        public void TestHitPointLostAndEncounterDeadWithoutLootOnFirstDice()
        {
            AddResults(5, 1, 2);
            var cr = DoCombatTurn(1, 1);
            Assert.AreEqual(1, cr.HitpointLost);
            Assert.IsTrue(cr.EncounterDead);
            Assert.IsFalse(cr.LegendaryTreasureFound);
            Assert.IsFalse(cr.ComponentFound);
            Assert.AreEqual(5, GameState.CurrentHitPoint);
        }

        [Test]
        public void TestHitPointLostAndEncounterWithParalysis()
        {
            AddResults(1, 5, 6);
            GameEngine.UseParalysisWand();
            var cr = DoCombatTurn(1, 5);
            Assert.AreEqual(3, cr.DiceRoll.First);
            Assert.AreEqual(7, cr.DiceRoll.Second);
            Assert.AreEqual(1, cr.HitpointLost, "Lost != 1 HP");
            Assert.IsTrue(cr.EncounterDead);
            Assert.IsFalse(cr.LegendaryTreasureFound);
            Assert.IsFalse(cr.ComponentFound);
            Assert.AreEqual(5, GameState.CurrentHitPoint);
        }
        [Test]
        public void TestHncounterDeadWithLoot()
        {
            AddResults(1, 5, 1);
            Assert.AreEqual(0, GameState.Inventory.Stores.Single(x => x.ComponentId == GetRegion(1).Component.ID).Quantity);
            var cr = DoCombatTurn(1, 1);
            Assert.AreEqual(1, cr.DiceRoll.First, "First dice should be 1");
            Assert.AreEqual(5, cr.DiceRoll.Second);
            Assert.AreEqual(1, cr.HitpointLost, "Lost != 1 HP");
            Assert.IsTrue(cr.EncounterDead);
            Assert.IsFalse(cr.LegendaryTreasureFound);
            Assert.IsTrue(cr.ComponentFound);
            Assert.AreEqual(5, GameState.CurrentHitPoint);
            Assert.AreEqual(1, GameState.Inventory.Stores.Single(x => x.ComponentId == GetRegion(1).Component.ID).Quantity);
        }

        [Test]
        public void FindTreasureAddsAbility()
        {
            Assert.IsFalse(GameEngine.HasAbility(Ability.BetterDefense));
            FindLT(true);
            Assert.IsTrue(GameEngine.HasAbility(Ability.BetterDefense));
            FindLT(false);
            Assert.IsTrue(GameEngine.HasAbility(Ability.BetterDefense));
        }



        [Test]
        public void IcePlateHelpsDefense()
        {
            AddResults(4, 6, 2);
            int indexRegion = 1;
            Assert.IsTrue(GameEngine.TreasureIsFound(GetRegion(indexRegion)));
            var cr = DoCombatTurn(indexRegion, 5);

            Assert.AreEqual(0, cr.HitpointLost);
        }

        [Test]
        public void GoldChassisHelpsAgainstSpirit()
        {
            AddResults(5, 5);
            var cr = DoCombatTurn(4, 3);
            Assert.AreEqual(false, cr.EncounterDead);
            AddResults(5, 5, 4);
            GameEngine.FindConstruct(GameEngine.GetRegionStateFor(4), true);
            Assert.IsTrue(GameEngine.HasAbility(Ability.HelpAgainstSpirit));
            cr = DoCombatTurn(4, 3);
            Assert.AreEqual(true, cr.EncounterDead);
        }

        [Test]
        public void TreasureHelpsToAttack()
        {
            AddResults(5, 5, 2);
            int indexRegion = 1;
            Assert.IsTrue(GameEngine.TreasureIsFound(GetRegion(6)));
            var cr = DoCombatTurn(indexRegion, 5);
            Assert.AreEqual(true, cr.EncounterDead);
        }

        private void FindLT(bool expectToFindLT)
        {
            AddResults(3, 6, 2);
            int indexRegion = 1;
            var cr = DoCombatTurn(indexRegion, 5);
            Assert.AreEqual(3, cr.DiceRoll.First);
            Assert.AreEqual(6, cr.DiceRoll.Second);
            Assert.AreEqual(1, cr.HitpointLost);
            Assert.IsTrue(cr.EncounterDead);
            Assert.AreEqual(expectToFindLT, cr.LegendaryTreasureFound);
            Assert.IsFalse(cr.ComponentFound);
            RegionState rs = GameEngine.GetRegionStateFor(indexRegion);
            Assert.AreEqual(true, rs.LegendaryTreasureFound);

        }
        private CombatResult DoCombatTurn(int indexRegion, int encounterLevel)
        {
            Region r = GetRegion(indexRegion);
            Encounter e = r.GetEncounter(encounterLevel);
            CombatResult cr = GameEngine.MakeCombatTurn(e, r);
            return cr;
        }


    }


}
