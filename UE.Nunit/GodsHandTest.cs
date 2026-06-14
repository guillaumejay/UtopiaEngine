using System;

namespace UE.NUnit
{
    [TestFixture]
    public class GodsHandTest : BaseEngineTest
    {
        [SetUp]
        public void Initialize()
        {
            GameState.ResetState(GameEngine.GameDefinition);
        }

        [Test]
        public void NotUsableWithoutEnoughEnergy()
        {
            ClassicAssert.AreEqual(0, GameState.GodsHandEnergy);
            ClassicAssert.IsFalse(GameEngine.IsGodsHandUsable);
            Assert.Throws<InvalidOperationException>(() => GameEngine.UseGodsHand());
        }

        [Test]
        public void UsingSpendsThreeEnergyAndCrossesASkull()
        {
            GameState.GodsHandEnergy = 3;
            ClassicAssert.IsTrue(GameEngine.IsGodsHandUsable);

            GameEngine.UseGodsHand();

            ClassicAssert.AreEqual(0, GameState.GodsHandEnergy);
            ClassicAssert.AreEqual(1, GameState.NumberOfSkullsCrossed);
        }

        [Test]
        public void NotUsableWhenAllSkullsAlreadyCrossed()
        {
            GameState.GodsHandEnergy = 6;
            GameState.NumberOfSkullsCrossed = GameEngine.GameDefinition.NumberOfSkulls;

            ClassicAssert.IsFalse(GameEngine.IsGodsHandUsable);
            Assert.Throws<InvalidOperationException>(() => GameEngine.UseGodsHand());
        }
    }
}
