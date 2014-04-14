using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Input;
using Cirrious.MvvmCross.ViewModels;
using UE.Core.Entities;
using UE.Core.Interfaces;

namespace UE.Core.ViewModels
{
    public class RegionDataViewModel
    {
        public string RegionName { get; set; }

        public string ComponentText { get; set; }

        public int ComponentStore { get; set; }

		public string ComponentString 
		{ get { return String.Format ("{0} ({1})", ComponentText, ComponentStore); } }

        public string TreasureText { get; set; }

        public bool TreasureFound { get; set; }

		public string TreasureString 
		{ get { return String.Format ("{0} ({1})", TreasureText,(TreasureFound)?"OK":"Missing"); } }

        public string ConstructText { get; set; }

        public bool ConstructFound { get; set; }

		public string ConstructString 
		{ get { return String.Format ("{0} ({1})", ConstructText,(ConstructFound)?"OK":"Missing"); } }

		public bool HasEvent { get { return !String.IsNullOrEmpty (Events); } }

        public string Events { get; set; }

        public int Index { get; set; }

        public static RegionDataViewModel Create(RegionState rs, Inventory inventory)
        {
            RegionDataViewModel rdvm = new RegionDataViewModel();
            rdvm.RegionName = rs.Region.Name.Text;
            rdvm.Index = rs.Region.Index;
            rdvm.ComponentStore = inventory.GetComponentQuantityFor(rs.Region.Component.ID);
            rdvm.ComponentText = rs.Region.Component.Name.Text;
            rdvm.ConstructFound = rs.ConstructFound;
            rdvm.TreasureFound = rs.LegendaryTreasureFound;
            rdvm.ConstructText = rs.Region.Construct.Name.Text;
            rdvm.TreasureText = rs.Region.LegendaryTreasure.Name.Text;
            rdvm.Events = String.Join(", ", rs.Events.Select(x => x.Name));
            return rdvm;
        }

    }
    public class StartSearchViewModel : BaseViewModel
    {
        public List<RegionDataViewModel> RegionData { get; set; }


        public StartSearchViewModel(IGameEngine engine)
            : base(engine)
        {

        }


        protected override void FillViewModel()
        {
            RegionData = new List<RegionDataViewModel>();
            foreach (RegionState rs in _gameEngine.GameState.RegionStates)
            {
                var rdvm = RegionDataViewModel.Create(rs, _gameEngine.GameState.Inventory);
                RegionData.Add(rdvm);
            }
        }

        public ICommand RegionSelectedCommand
        {
            get
            {
                 MvxCommand<RegionDataViewModel> command =new MvxCommand<RegionDataViewModel>(SearchRegion);
                return command;
            }
        }

        private void SearchRegion(RegionDataViewModel item)
        {
            _gameEngine.EnterRegionForSearch(item.Index);
            ShowViewModel<SearchRegionViewModel>();
        }
        public ICommand ChooseRegionCommand { get { return new MvxCommand<int>(ChooseRegion); } }

        public ICommand BackCommand
        {
            get
            {
                return new MvxCommand(() => ShowViewModel<MainViewModel>());
            }
        }

        protected void ChooseRegion(int index)
        {
            _gameEngine.EnterRegionForSearch(index);
			ShowViewModel<SearchRegionViewModel> ();
        }


    }

}
