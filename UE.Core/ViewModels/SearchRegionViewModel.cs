using System;
using System.Collections.Generic;
using UE.Core.ViewModels;
using UE.Core.Architecture;
using UE.Core.Interfaces;
using System.Windows.Input;
using Cirrious.MvvmCross.ViewModels;

namespace UE.Core.ViewModels
{
	public class SearchRegionViewModel:BaseViewModel
	{
		public SearchRegionViewModel (IGameEngine engine)
			: base(engine)
		{

		}

		protected override void FillViewModel ()
		{
			ButtonVisibility = new List<bool>();
            ButtonVisibility.Add(true);
            ButtonVisibility.Add(false);
			SearchDice = _gameEngine.DiceGenerator.Get2d6 ();

		}

        public List<bool>  ButtonVisibility { get; set; }

		private TwoDice _searchDice;

		public TwoDice SearchDice {
			get {
				return _searchDice;
			}
			set {
				_searchDice = value;
				ButtonVisibility [0] = true;
				ButtonVisibility [1] = true;
				RaisePropertyChanged (() => SearchDice);
				RaisePropertyChanged (() => ButtonVisibility);
			}
		}

		public Table SearchBox {
			get
			{ return _gameEngine.CurrentSearchBox; }
			set
			{}
		}
		private bool _displayDowsingRod;
		public bool DisplayDowsingRod
		{
			get {
				return _displayDowsingRod;
			}

			set{
				_displayDowsingRod = value;
				RaisePropertyChanged(()=>DisplayDowsingRod);
			}
		}
		public string RegionName { get; set; }

		//public ICommand PlaceNumberCommand { get { return new MvxCommand<int,int> (PlaceNumber); } }

		public void PlaceNumber (int index, int value, int diceIndex)
		{
			
			SearchBox.PlaceNumber (index, value);
			if (SearchBox.IsFull) {
				CanSearchResult crs=_gameEngine.CanModifySearchResultInCurrentRegion(SearchBox.SearchResult);
				if (crs.CanModify) {
					DisplayDowsingRod =crs.CanUseDowsingRod;
				}
			} 
			else {
				if (SearchBox.UnfilledCellCount % 2 == 0) {
					SearchDice = _gameEngine.DiceGenerator.Get2d6 ();
				} else {
					ButtonVisibility [diceIndex] = false;
						RaisePropertyChanged (() => ButtonVisibility);
				}
			}

		}
	}
}

