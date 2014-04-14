using System;

namespace UE.Core.Architecture.Messages
{
    public class CombatResult
    {
        public int HitpointLost { get; set; }

        public bool EncounterDead { get; set; }

        public bool ComponentFound { get; set; }
        
        public bool LegendaryTreasureFound { get; set; }

        public TwoDice DiceRoll { get; set; }

        public override string ToString()
        {
            return String.Format("HP lost {0}, EncounterDead:{1},ComponentFound:{2},LegendaryTreasureFound{3}",HitpointLost,EncounterDead,ComponentFound,LegendaryTreasureFound);
        }
    }
}
