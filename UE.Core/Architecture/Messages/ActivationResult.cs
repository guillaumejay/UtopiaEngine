namespace UE.Core.Architecture.Messages
{
    public class ActivationResult:TimePassed
    {
        /// <summary>
        /// null if column is not filled, or the activation result (-1 = backlash) 
        /// </summary>
        public int? CurrentColumnValue{get;set;}

        public bool IsFieldFilled { get; set; }

        public bool IsConstructActivated { get; set; }

        public int EnergyPoints { get; set; }
    }
}
