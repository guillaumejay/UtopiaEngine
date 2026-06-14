using System;

namespace UE.NUnit
{
    [TestFixture]
    public class GuardClausesTest : BaseEngineTest
    {
        [SetUp]
        public void Initialize()
        {
            GameState.ResetState(GameEngine.GameDefinition);
        }

        [Test]
        public void UseDowsingRodRejectsResultBelowTwo()
        {
            Assert.Throws<InvalidOperationException>(() => GameEngine.UseDowsingRod(1, 50));
        }

        [Test]
        public void UseDowsingRodRejectsNumberOutOfRange()
        {
            Assert.Throws<ArgumentException>(() => GameEngine.UseDowsingRod(50, 0));
            Assert.Throws<ArgumentException>(() => GameEngine.UseDowsingRod(50, 101));
        }

        [Test]
        public void UseParalysisWandTwiceThrows()
        {
            GameEngine.UseParalysisWand();
            Assert.Throws<InvalidOperationException>(() => GameEngine.UseParalysisWand());
        }

        [Test]
        public void UseFocusCharmTwiceThrows()
        {
            GameEngine.UseFocusCharm(1);
            Assert.Throws<InvalidOperationException>(() => GameEngine.UseFocusCharm(1));
        }

        [Test]
        public void SpendHPBeyondCurrentHitPointsThrows()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => GameEngine.SpendHPToReduceActivationDifficulty(GameState.CurrentHitPoint + 1));
        }

        [Test]
        public void SpendHPTwiceThrows()
        {
            GameEngine.SpendHPToReduceActivationDifficulty(1);
            Assert.Throws<InvalidOperationException>(
                () => GameEngine.SpendHPToReduceActivationDifficulty(1));
        }

        [Test]
        public void WasteBasketOverflowThrows()
        {
            for (int i = 0; i < 10; i++)
            {
                GameEngine.AddToWasteBasket();
            }
            ClassicAssert.IsTrue(GameState.IsWasteBasketFull);
            Assert.Throws<InvalidOperationException>(() => GameEngine.AddToWasteBasket());
        }
    }
}
