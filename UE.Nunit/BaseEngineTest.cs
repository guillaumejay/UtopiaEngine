using System.Linq;
using NUnit.Framework;
using UE.Core;
using UE.Core.Architecture.Messages;
using UE.Core.Entities;
using UE.Core.Interfaces;

namespace UE.NUnit
{
    [TestFixture]
    public class BaseEngineTest
    {
        protected GameState GameState
        {
            get { return GameEngine.GameState; }
        }

        protected virtual IDiceRoller GetDiceRoller()
        {
            return new RandomDice();
        }
        protected readonly GameEngine GameEngine;
        protected XmlRepository _xmlRepository;

        public BaseEngineTest()
        {
// ReSharper disable DoNotCallOverridableMethodsInConstructor
            _xmlRepository = new XmlRepository();
            GameEngine = new GameEngine(_xmlRepository, GetDiceRoller());
// ReSharper restore DoNotCallOverridableMethodsInConstructor
            GameEngine.Init("Data\\DefinitionStandard.xml");
        }

        protected Region GetRegion(int index)
        {
            return GameEngine.GameDefinition.Regions.Single(x => x.Index == index);
        }

        protected void AssertGameStateIsCorrectlyHydrated()
        {
            Assert.IsTrue(GameState.RegionStates.All(x => x.SearchBoxes.Count == 6));
            Assert.IsNotNull(GameState.Inventory);
            Assert.IsTrue(GameState.RegionStates.All(x => x.Region != null));
            Assert.IsTrue(GameState.RegionStates.All(x => x.LegendaryTreasure != null));
            Assert.IsTrue(GameState.Constructs.All(x => x.Construct != null));
            Assert.IsTrue(GameState.LinkStates.All(x => x.Link != null));
            Assert.IsTrue(GameState.LinkStates.All(x => x.Construct1 != null));
            Assert.IsTrue(GameState.LinkStates.All(x => x.Construct2 != null));
            Assert.IsTrue(GameState.Inventory.Components != null );
            Assert.AreEqual(GameEngine.GameDefinition.Components.Count, GameState.Inventory.Stores.Count());
        }

        protected void CreateConnection(LinkState ls, int nbLinks = 5)
        {
            LinkResult lr;
            for (int i = 1; i <= nbLinks; i++)
            {
                lr = GameEngine.WorkToLink(ls.ID, i, 2,false);
                Assert.AreEqual(false, lr.HasFailed);
                Assert.AreEqual((i == 6) ? true : false, lr.IsLinkFinished);
                Assert.AreEqual((i == 1) ? 1 : 0, lr.ComponentLost);
                Assert.AreEqual(0, lr.HitPointLost);
            }

        }
        protected void ActivateAllConstructs()
        {
            for (int i = 1; i <= 6; i++)
            {
                GameEngine.AddComponent(1, i);
            }
            foreach (RegionState rs in GameState.RegionStates)
            {
                GameEngine.FindConstruct(rs, true);
            }
            Assert.AreEqual(6, GameEngine.GameState.ConstructsActivated.Count());
            Assert.AreEqual(6, GameEngine.GameState.PossibleLinks.Count());
        }

        protected LinkState ActivateTwoConstructs()
        {
            GameEngine.AddComponent(1, 4);
            Assert.AreEqual(0, GameState.PossibleLinks.Count());
            RegionState rs = GameEngine.GetRegionStateFor(1);
            ConstructState cs = GameEngine.FindConstruct(rs, true);
            rs = GameEngine.GetRegionStateFor(2);
            ConstructState cs2 = GameEngine.FindConstruct(rs, true);
            Assert.AreEqual(1, GameState.PossibleLinks.Count());
            LinkState l = GameState.PossibleLinks.Single();
            Assert.AreEqual(cs.ID, l.Construct1.ID);
            Assert.AreEqual(cs2.ID, l.Construct2.ID);
            return l;
        }
    }
}
