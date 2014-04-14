namespace UE.Core.Architecture.Messages
{
    public class FinalActivationResult:TimePassed
    {
        public bool GameWon { get; set; }

        public TwoDice Roll { get; set; }

    }
}
