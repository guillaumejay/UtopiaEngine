using System.Linq;
using Engine_Test;
using NUnit.Framework;
using UE.Core.Architecture;
using UE.Core.Architecture.Messages;
using UE.Core.Entities;

namespace UE.NUnit
{
    [TestFixture]
    public class SearchTest:BaseFixedDiceTest
    {

        [SetUp]
        public void Initialize()
        {
            GameState.ResetState(GameEngine.GameDefinition);
        }

        
        [Test]
        public void PlaceNumbersOnSearchRegion()
        {
            int indexRegion = 1;
            Assert.AreEqual(6, GameEngine.NumberOfAvailableSearchesBoxesFor(indexRegion));
            GameEngine.PlaceSearchNumberOnRegion(indexRegion,1,1);
            Assert.AreEqual(6, GameEngine.NumberOfAvailableSearchesBoxesFor(indexRegion));
            Table search = GameEngine.CurrentSearchBoxForRegion(indexRegion);
            Assert.IsTrue(search.IsStarted);
            Assert.AreEqual(1,search.Columns[0].Top);
            Assert.AreEqual(1, GameEngine.GetRegionStateFor(indexRegion).SearchBoxes.Count(x => x.IsStarted));
            for (int i = 2; i <= 6; i++)
            {
                GameEngine.PlaceSearchNumberOnRegion(indexRegion, i, i);
                int actual = GameEngine.GetRegionStateFor(indexRegion).SearchBoxes.Count(x => x.IsStarted);
                Assert.AreEqual(1, actual,"Is Started = " + actual + " on " + i);
            }
            Assert.IsTrue(search.IsFull);
            Assert.AreEqual(5, GameEngine.NumberOfAvailableSearchesBoxesFor(indexRegion));
            Assert.AreEqual(1, GameEngine.GetRegionStateFor(indexRegion).SearchBoxes.Count(x => x.IsStarted));
        }
        [Test]
        public void UseDowsingRod()
        {

            Assert.IsFalse(GameEngine.CanModifySearchResult(1,1).CanUseDowsingRod);
			Assert.IsTrue(GameEngine.CanModifySearchResult(-1,1).CanUseDowsingRod);
			Assert.IsTrue(GameEngine.CanModifySearchResult(50,1).CanUseDowsingRod);
		    Assert.AreEqual(30, GameEngine.UseDowsingRod(50, 20));
			Assert.IsFalse(GameEngine.CanModifySearchResult(50,1).CanUseDowsingRod);
            Assert.IsFalse(GameEngine.GameState.Inventory.DowsingRodCharged);
        }

        [Test]
        public void SearchShouldCrossDay()
        {
            AddResults(6,6,6,6,6,6,6,6,6,6);
            Region r = GameEngine.GameDefinition.GetRegion(1);
            TimePassed t = GameEngine.CrossRegionTracker(r);
            Assert.AreEqual(1,t.DaysPassed);
            Assert.AreEqual(false, t.eventOccured);
            FillCurrentSearchBox(1);
            t = GameEngine.CrossRegionTracker(r);
            Assert.AreEqual(1, t.DaysPassed);
            Assert.AreEqual(true, t.eventOccured);
            FillCurrentSearchBox(1);
            t = GameEngine.CrossRegionTracker(r);
            Assert.AreEqual(0, t.DaysPassed);
            Assert.AreEqual(false, t.eventOccured);
        }

        private void FillCurrentSearchBox(int indexRegion)
        {
            for (int i = 1; i <= 6; i++)
            {
                GameEngine.PlaceSearchNumberOnRegion(indexRegion, i, i);
            }
        }

        [Test]
        public void SearchResultFilled()
        {
            Region r = GameEngine.GameDefinition.GetRegion(1);
            SearchResult sr = GameEngine.ApplySearch(80, r, false);
            Assert.AreEqual(0,sr.MonsterLevel);
            Assert.AreEqual(1,sr.NumberOfComposantFound);
            Assert.IsFalse(sr.ConstructFound);
            sr = GameEngine.ApplySearch(8, r, false);
            Assert.AreEqual(0,sr.MonsterLevel);
            Assert.AreEqual(0,sr.NumberOfComposantFound);
            Assert.IsTrue(sr.ConstructFound);
            Assert.AreEqual(1,GameState.ConstructsFound.Count() );
            Assert.AreEqual(1, GameState.ConstructsUnactivated.Count());
            Assert.AreEqual(0, GameState.ConstructsActivated.Count());
            sr = GameEngine.ApplySearch(8, r, false);
            Assert.AreEqual(0, sr.MonsterLevel);
            Assert.AreEqual(2, sr.NumberOfComposantFound);
            Assert.IsFalse(sr.ConstructFound);
            sr = GameEngine.ApplySearch(208,r, false);
            Assert.AreEqual(2, sr.MonsterLevel);
            Assert.AreEqual(0, sr.NumberOfComposantFound);
            Assert.IsFalse(sr.ConstructFound);
        }

        [Test]
        public void SearchResult0GivesActivatedConstruct()
        {
            Assert.AreEqual(0, GameState.NumberOfSkullsCrossed);
            Region r = GameEngine.GameDefinition.GetRegion(1);
            SearchResult sr = GameEngine.ApplySearch(0, r, false);
            Assert.AreEqual(1, GameState.ConstructsFound.Count());
            Assert.AreEqual(0, GameState.ConstructsUnactivated.Count());
            Assert.AreEqual(1, GameState.ConstructsActivated.Count());
            Assert.AreEqual(2, GameState.GodsHandEnergy);
            Assert.AreEqual(1,GameState.NumberOfSkullsCrossed);
            Assert.AreEqual(true, sr.ConstructFound);
            Assert.AreEqual(0, sr.NumberOfComposantFound);
        }

        [Test]
        public void FoundaConstructASecondTimeGivesTwoComponents()
        {
            RegionState rs = GameEngine.GetRegionStateFor(1);
            GameEngine.FindConstruct(rs, false);
            Assert.IsTrue(rs.ConstructFound);
            Assert.AreEqual(1, GameState.ConstructsFound.Count());
            SearchResult sr = GameEngine.ApplySearch(0, rs.Region, false);
            Assert.AreEqual(1, GameState.ConstructsFound.Count());
          Assert.AreEqual(false,sr.ConstructFound);
            Assert.AreEqual(2,sr.NumberOfComposantFound);
        }
    }
}
