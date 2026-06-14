using NUnit.Framework;
using System.Linq;
using System.IO;
using UE.Core.Entities;
using UE.NUnit;

namespace Engine_Test
{
    [TestFixture]
    public class RepositoryTest:BaseEngineTest
    {
        string file = "saveTest.xml";
        
        [Test]
        public void BasicSaveGameState()
        {
            SaveGS();
        }

        private void SaveGS()
        {
          
            GameEngine.SaveGameState(file);
            ClassicAssert.IsTrue(File.Exists(file));
        }

        [Test]
        public void BasicLoadGameState()
        {
            int id=GameEngine.GameDefinition.ID;
            SaveGS();
            GameEngine.LoadGameState(file);
            AssertGameStateIsCorrectlyHydrated();
            ClassicAssert.AreEqual(id, GameEngine.GameState.DefinitionID);
        }
        [Test]
        public void SaveGameStateBasicValues()
        {
            GameState.CurrentHitPoint = 4;
            GameState.CurrentDay = 5;
            GameState.GodsHandEnergy = 2;
            GameState.NumberOfSkullsCrossed = 1;
         
            GameEngine.SaveGameState(file);
            GameEngine.ResetGameState();
            GameEngine.LoadGameState(file);
            ClassicAssert.AreEqual(4,GameState.CurrentHitPoint);
            ClassicAssert.AreEqual(5, GameState.CurrentDay);
            ClassicAssert.AreEqual(2, GameState.GodsHandEnergy);
            ClassicAssert.AreEqual(1, GameState.NumberOfSkullsCrossed);
        }

        [Test]
        public void SaveConstructFound()
        {
            RegionState rs = GameEngine.GetRegionStateFor(1);
            ClassicAssert.IsFalse(rs.ConstructFound);
            GameEngine.FindConstruct(rs, false);
            ClassicAssert.IsTrue(rs.ConstructFound);
            GameEngine.SaveGameState(file);
            GameEngine.ResetGameState();
            GameEngine.LoadGameState(file);
            rs = GameEngine.GetRegionStateFor(1);
            ClassicAssert.IsTrue(rs.ConstructFound);
        }

        [Test]
        public void SaveEvents()
        {
            GameEngine.RollEvents();
            string events = GameState.RegionStates.Aggregate(string.Empty, (current, rs) => current + string.Join("-", rs.Events.Select(x => x.ID.ToString() + rs.Index.ToString())));
            GameEngine.SaveGameState(file);
            GameEngine.ResetGameState();
            GameEngine.LoadGameState(file);
            string eventsLoaded = GameState.RegionStates.Aggregate(string.Empty, (current, rs) => current + string.Join("-", rs.Events.Select(x => x.ID.ToString() + rs.Index.ToString())));
            ClassicAssert.AreEqual(events,eventsLoaded);
        }
    }
}
