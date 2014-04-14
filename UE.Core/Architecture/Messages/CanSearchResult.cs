using System;

namespace UE.Core
{
	public class CanSearchResult
	{
		public bool CanModify {
			get {
				return CanUseDowsingRod || CanUseGoodFortune || CanUseConstructPower;
			}
		}
	
			public bool CanUseDowsingRod {get ;set;}

		public bool CanUseConstructPower { get; set;}

		public string ConstructPowerText { get; set;}

		public bool CanUseGoodFortune {get;set;}

		public string GoodFortuneText {get;set;}
	}
}

