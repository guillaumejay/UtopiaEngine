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
            ClassicAssert.AreEqual(6, GameEngine.NumberOfAvailableSearchesBoxesFor(indexRegion));
            GameEngine.PlaceSearchNumberOnRegion(indexRegion,1,1);
            ClassicAssert.AreEqual(6, GameEngine.NumberOfAvailableSearchesBoxesFor(indexRegion));
            Table search = GameEngine.CurrentSearchBoxForRegion(indexRegion);
            ClassicAssert.IsTrue(search.IsStarted);
            ClassicAssert.AreEqual(1,search.Columns[0].Top);
            ClassicAssert.AreEqual(1, GameEngine.GetRegionStateFor(indexRegion).SearchBoxes.Count(x => x.IsStarted));
            for (int i = 2; i <= 6; i++)
            {
                GameEngine.PlaceSearchNumberOnRegion(indexRegion, i, i);
                int actual = GameEngine.GetRegionStateFor(indexRegion).SearchBoxes.Count(x => x.IsStarted);
                ClassicAssert.AreEqual(1, actual,"Is Started = " + actual + " on " + i);
            }
            ClassicAssert.IsTrue(search.IsFull);
            ClassicAssert.AreEqual(5, GameEngine.NumberOfAvailableSearchesBoxesFor(indexRegion));
            ClassicAssert.AreEqual(1, GameEngine.GetRegionStateFor(indexRegion).SearchBoxes.Count(x => x.IsStarted));
        }
        [Test]
        public void UseDowsingRod()
        {

            ClassicAssert.IsFalse(GameEngine.CanModifySearchResult(1,1).CanUseDowsingRod);
			ClassicAssert.IsTrue(GameEngine.CanModifySearchResult(-1,1).CanUseDowsingRod);
			ClassicAssert.IsTrue(GameEngine.CanModifySearchResult(50,1).CanUseDowsingRod);
		    ClassicAssert.AreEqual(30, GameEngine.UseDowsingRod(50, 20));
			ClassicAssert.IsFalse(GameEngine.CanModifySearchResult(50,1).CanUseDowsingRod);
            ClassicAssert.IsFalse(GameEngine.GameState.Inventory.DowsingRodCharged);
        }

        [Test]
        public void SearchShouldCrossDay()
        {
            AddResults(6,6,6,6,6,6,6,6,6,6);
            Region r = GameEngine.GameDefinition.GetRegion(1);
            TimePassed t = GameEngine.CrossRegionTracker(r);
            ClassicAssert.AreEqual(1,t.DaysPassed);
            ClassicAssert.AreEqual(false, t.eventOccured);
            FillCurrentSearchBox(1);
            t = GameEngine.CrossRegionTracker(r);
            ClassicAssert.AreEqual(1, t.DaysPassed);
            ClassicAssert.AreEqual(true, t.eventOccured);
            FillCurrentSearchBox(1);
            t = GameEngine.CrossRegionTracker(r);
            ClassicAssert.AreEqual(0, t.DaysPassed);
            ClassicAssert.AreEqual(false, t.eventOccured);
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
            ClassicAssert.AreEqual(0,sr.MonsterLevel);
            ClassicAssert.AreEqual(1,sr.NumberOfComposantFound);
            ClassicAssert.IsFalse(sr.ConstructFound);
            sr = GameEngine.ApplySearch(8, r, false);
            ClassicAssert.AreEqual(0,sr.MonsterLevel);
            ClassicAssert.AreEqual(0,sr.NumberOfComposantFound);
            ClassicAssert.IsTrue(sr.ConstructFound);
            ClassicAssert.AreEqual(1,GameState.ConstructsFound.Count() );
            ClassicAssert.AreEqual(1, GameState.ConstructsUnactivated.Count());
            ClassicAssert.AreEqual(0, GameState.ConstructsActivated.Count());
            sr = GameEngine.ApplySearch(8, r, false);
            ClassicAssert.AreEqual(0, sr.MonsterLevel);
            ClassicAssert.AreEqual(2, sr.NumberOfComposantFound);
            ClassicAssert.IsFalse(sr.ConstructFound);
            sr = GameEngine.ApplySearch(208,r, false);
            ClassicAssert.AreEqual(2, sr.MonsterLevel);
            ClassicAssert.AreEqual(0, sr.NumberOfComposantFound);
            ClassicAssert.IsFalse(sr.ConstructFound);
        }

        [Test]
        public void SearchResult0GivesActivatedConstruct()
        {
            ClassicAssert.AreEqual(0, GameState.NumberOfSkullsCrossed);
            Region r = GameEngine.GameDefinition.GetRegion(1);
            SearchResult sr = GameEngine.ApplySearch(0, r, false);
            ClassicAssert.AreEqual(1, GameState.ConstructsFound.Count());
            ClassicAssert.AreEqual(0, GameState.ConstructsUnactivated.Count());
            ClassicAssert.AreEqual(1, GameState.ConstructsActivated.Count());
            ClassicAssert.AreEqual(2, GameState.GodsHandEnergy);
            ClassicAssert.AreEqual(1,GameState.NumberOfSkullsCrossed);
            ClassicAssert.AreEqual(true, sr.ConstructFound);
            ClassicAssert.AreEqual(0, sr.NumberOfComposantFound);
        }

        [Test]
        public void FoundaConstructASecondTimeGivesTwoComponents()
        {
            RegionState rs = GameEngine.GetRegionStateFor(1);
            GameEngine.FindConstruct(rs, false);
            ClassicAssert.IsTrue(rs.ConstructFound);
            ClassicAssert.AreEqual(1, GameState.ConstructsFound.Count());
            SearchResult sr = GameEngine.ApplySearch(0, rs.Region, false);
            ClassicAssert.AreEqual(1, GameState.ConstructsFound.Count());
          ClassicAssert.AreEqual(false,sr.ConstructFound);
            ClassicAssert.AreEqual(2,sr.NumberOfComposantFound);
        }
    }
}
