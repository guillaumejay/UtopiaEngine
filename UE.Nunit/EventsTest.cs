using NUnit.Framework;
using UE.Core.Architecture;
using UE.Core.Architecture.Messages;
using UE.Core.Entities;

namespace UE.NUnit
{
    [TestFixture]
    public class EventsTest:BaseFixedDiceTest
    {

        [SetUp]
        public void Initialize()
        {
            fixedDice.FixedDie.Clear();
            GameState.ResetState(GameEngine.GameDefinition);
        }

        [Test]
        public void FoulWeatherOnEnteringRegion()
        {
            Assert.IsFalse(GameEngine.HasAbility(Ability.FoulWeather, -1));
            Region r=SetEventsInRegion(1);
            Assert.IsTrue(GameEngine.HasAbility(Ability.FoulWeather, 1));
            Assert.IsTrue(GameEngine.HasAbility(Ability.FoulWeather, -1));
            AddResults(2,2,2,2);
            TimePassed t = GameEngine.CrossRegionTracker(r);
            Assert.IsTrue(GameEngine.HasAbility(Ability.FoulWeather, 2));
            Assert.AreEqual(2,t.DaysPassed);
            Assert.AreEqual(0,fixedDice.FixedDie.Count);
            // check that the same event is not applied twice
            r = SetEventsInRegion(1);
            AddResults(1, 1, 1, 1);
             t = GameEngine.CrossRegionTracker(r);
            Assert.AreEqual(2, t.DaysPassed);
        }

        [Test]
        public void FoulWeatherDuringRegionStay()
        {
            GameEngine.Rest(1);
            Assert.IsFalse(GameEngine.HasAbility(Ability.FoulWeather,1));
            Region r = SetEventsInRegion(2);
            AddResults(1, 1, 1, 1);
            Assert.IsTrue(GameEngine.HasAbility(Ability.FoulWeather, 2));
            TimePassed t = GameEngine.CrossRegionTracker(r);
            Assert.IsTrue(GameEngine.HasAbility(Ability.FoulWeather, 1));
            Assert.AreEqual(2, t.DaysPassed);
            Assert.AreEqual(0, fixedDice.FixedDie.Count);
        }

        [Test]
        public void GoodFortuneEvent()
        {
            GameState.Inventory.DowsingRodCharged = false;
            Assert.IsFalse(GameEngine.CanModifySearchResult(1,1).CanModify,"Search can be modified");
            Region r = SetEventsInRegion(1);
            Assert.IsTrue(GameEngine.CanModifySearchResult(1, 1).CanModify,"Search can not be modified");
        }

        [Test]
        public void EncounterLevelWithActiveMonsters()
        {
            int indexRegion = 1;
            Region r = GameEngine.GetRegion(indexRegion);
            Assert.AreEqual(6, GameEngine.NumberOfAvailableSearchesBoxesFor(indexRegion));
            GameEngine.PlaceSearchNumberOnRegion(indexRegion, 1, 2);
            GameEngine.PlaceSearchNumberOnRegion(indexRegion, 4, 1);
            Table search = GameEngine.CurrentSearchBoxForRegion(indexRegion);
            Assert.AreEqual(100,search.SearchResult);
            SearchResult sr= GameEngine.ApplySearch(search.SearchResult, r, false);
            Assert.AreEqual(1,sr.MonsterLevel);
            SetEventsInRegion(indexRegion);
            Assert.IsTrue(GameEngine.HasAbility(Ability.ActiveMonsters, indexRegion));            
            sr = GameEngine.ApplySearch(search.SearchResult, r, false);
            Assert.AreEqual(3,sr.MonsterLevel);
        }

        [Test]
        public void FleetingVisionHelpsActivating()
        {
            SetEventsInRegion(1);
            Assert.AreEqual(0, GameState.GodsHandEnergy);
            RegionState rs = GameEngine.GetRegionStateFor(1);
            ConstructState cs = GameEngine.FindConstruct(rs, false);
            ActivationResult ar= GameEngine.WorkToActivate(cs, 1, 1);
            Assert.AreEqual(1,ar.EnergyPoints);
        }

        private Region SetEventsInRegion(int indexRegion)
        {
            AddResults(indexRegion, indexRegion, indexRegion, indexRegion);
            GameEngine.RollEvents();
            return GameEngine.GetRegion(indexRegion);
        }
    }
}
