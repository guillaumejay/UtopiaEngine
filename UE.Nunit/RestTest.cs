namespace UE.NUnit
{
    [TestFixture]
    public class RestTest : BaseEngineTest
    {
        [SetUp]
        public void Initialize()
        {
            GameState.ResetState(GameEngine.GameDefinition);
        }

        [Test]
        public void CannotRestAtFullHitPoints()
        {
            ClassicAssert.AreEqual(GameEngine.GameDefinition.MaxHitPoint, GameState.CurrentHitPoint);
            ClassicAssert.IsFalse(GameEngine.CanRest);
        }

        [Test]
        public void RestingRecoversOneHitPointPerDay()
        {
            GameState.CurrentHitPoint = 3;
            ClassicAssert.IsTrue(GameEngine.CanRest);

            GameEngine.Rest(2);

            ClassicAssert.AreEqual(5, GameState.CurrentHitPoint);
            ClassicAssert.AreEqual(2, GameState.CurrentDay);
        }
    }
}
