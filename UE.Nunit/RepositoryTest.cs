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
            Assert.IsTrue(File.Exists(file));
        }

        [Test]
        public void BasicLoadGameState()
        {
            int id=GameEngine.GameDefinition.ID;
            SaveGS();
            GameEngine.LoadGameState(file);
            AssertGameStateIsCorrectlyHydrated();
            Assert.AreEqual(id, GameEngine.GameState.DefinitionID);
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
            Assert.AreEqual(4,GameState.CurrentHitPoint);
            Assert.AreEqual(5, GameState.CurrentDay);
            Assert.AreEqual(2, GameState.GodsHandEnergy);
            Assert.AreEqual(1, GameState.NumberOfSkullsCrossed);
        }

        [Test]
        public void SaveConstructFound()
        {
            RegionState rs = GameEngine.GetRegionStateFor(1);
            Assert.IsFalse(rs.ConstructFound);
            GameEngine.FindConstruct(rs, false);
            Assert.IsTrue(rs.ConstructFound);
            GameEngine.SaveGameState(file);
            GameEngine.ResetGameState();
            GameEngine.LoadGameState(file);
            rs = GameEngine.GetRegionStateFor(1);
            Assert.IsTrue(rs.ConstructFound);
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
            Assert.AreEqual(events,eventsLoaded);
        }
    }
}
