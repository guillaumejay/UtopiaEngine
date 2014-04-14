using System;

namespace UE.Core.Architecture.Messages
{
    public class SearchResult
    {
        public int MonsterLevel { get; set; }

        public int FinalResult { get; set; }

        public bool ConstructFound { get; set; }

        public int NumberOfComposantFound { get; set; }

        public override string ToString()
        {
            return String.Format("FinalResult {0},EncounterLevel {1}, ConstructFound {2}, NumberOfComposantFound {3}",
                                 FinalResult, MonsterLevel, ConstructFound, NumberOfComposantFound);
        }
    }
}
