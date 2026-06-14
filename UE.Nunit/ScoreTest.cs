using System.Linq;
using UE.Core.Architecture.Messages;
using UE.Core.Entities;

namespace UE.NUnit
{
    [TestFixture]
    public class ScoreTest : BaseFixedDiceTest
    {
        [SetUp]
        public void Initialize()
        {
            fixedDice.FixedDie.Clear();
            GameState.ResetState(GameEngine.GameDefinition);
        }

        [Test]
        public void ScoreFromScalarStateValues()
        {
            // Fresh game: only current HP (6) + the 3 charged tools (10 each) count.
            ClassicAssert.AreEqual(GameState.CurrentHitPoint + 30, GameEngine.Score);
            int baseline = GameEngine.Score;

            GameState.NumberOfSkullsCrossed = 3;          // 5 pts each
            ClassicAssert.AreEqual(baseline + 15, GameEngine.Score);

            GameState.UnmodifiedPerfectZeroSearch = 2;    // 20 pts each
            ClassicAssert.AreEqual(baseline + 15 + 40, GameEngine.Score);
        }

        [Test]
        public void DeadPlayerHitPointsDoNotCount()
        {
            int withHp = GameEngine.Score;
            GameState.CurrentHitPoint = -1;               // negative HP excluded from score
            ClassicAssert.AreEqual(withHp - 6, GameEngine.Score);
        }

        [Test]
        public void FoundConstructAndTreasureEachScoreTen()
        {
            int before = GameEngine.Score;

            RegionState rs = GameEngine.GetRegionStateFor(1);
            GameEngine.FindConstruct(rs, false);          // found, not activated -> +10
            ClassicAssert.AreEqual(before + 10, GameEngine.Score);

            ClassicAssert.IsTrue(GameEngine.TreasureIsFound(GetRegion(2)));  // +10
            ClassicAssert.AreEqual(before + 20, GameEngine.Score);
        }

        [Test]
        public void WinningAddsBonusAndRemainingDays()
        {
            ActivateAllConstructs();
            foreach (LinkState ls in GameState.LinkStates)
            {
                CreateConnection(ls, 6);
            }
            ClassicAssert.IsTrue(GameEngine.IsFinalActivationPossible);

            int beforeWin = GameEngine.Score;
            int day = GameState.CurrentDay;

            AddResults(1, 2);
            FinalActivationResult far = GameEngine.WorkForfinalActivation();
            ClassicAssert.IsTrue(far.GameWon);

            // Winning grants 50 + 5 per day left (CurrentDay is unchanged on a successful activation).
            int expectedBonus = 50 + (GameEngine.GameDefinition.MaximumNumberOfDays - day) * 5;
            ClassicAssert.AreEqual(beforeWin + expectedBonus, GameEngine.Score);
        }
    }
}
