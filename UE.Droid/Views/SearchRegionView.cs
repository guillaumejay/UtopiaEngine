using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using UE.Core.ViewModels;


namespace UE.Droid.Views
{
    [Activity(Label = "Utopia Engine")]
	public class SearchRegionView : BaseView
    {
		private SearchRegionViewModel CurrentViewModel {
			get{ return ( SearchRegionViewModel)ViewModel;}
		}

		private Button _movingDice;
        public int currentNumberToPlace { get; set; }
        public Button Dice1, Dice2;
        /// <summary>
        /// Called when [create].
        /// </summary>
        /// <param name="bundle">The bundle.</param>
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
			this.SetContentView(Resource.Layout.SearchRegionView);
            TextView tv = FindViewById<TextView>(Resource.Id.searchbox1);
            tv.Drag += DragOnSearchBox;
            Dice1 = FindViewById<Button>(Resource.Id.dice1);
            Dice2 = FindViewById<Button>(Resource.Id.dice2);
            Dice1.Click += SelectBox;
            Dice2.Click += SelectBox;
			SetEventPlaceNumber(Resource.Id.searchbox1);
			SetEventPlaceNumber(Resource.Id.searchbox2);
			SetEventPlaceNumber(Resource.Id.searchbox3);
			SetEventPlaceNumber(Resource.Id.searchbox4);
			SetEventPlaceNumber(Resource.Id.searchbox5);
			SetEventPlaceNumber(Resource.Id.searchbox6);
        }



        private void SetEventPlaceNumber(int id)
        {
            TextView sel = FindViewById<TextView>(id);
            sel.Click += PlaceNumber;
        }

        void SelectBox(object sender, EventArgs e)
        {
			_movingDice = (sender as Button);
            //urrentNumberToPlace =Convert.ToInt32((sender as Button).Text);
            
        }

        void PlaceNumber(object sender, EventArgs e)
        {
			if (_movingDice==null)
				return;
			TextView target=sender as TextView;
			if (target.Text != String.Empty)
				return;
            target.Text = _movingDice.Text;
			CurrentViewModel.PlaceNumber(Convert.ToInt32 (target.Tag.ToString()),
			                             Convert.ToInt32(_movingDice.Text),
			                             Convert.ToInt32 (_movingDice.Tag.ToString()));
		//	_movingDice.Visibility = ViewStates.Invisible;
			_movingDice = null;
		}

        void DragOnSearchBox(object sender, Android.Views.View.DragEventArgs e)
        {
            var evt = e.Event;
            switch (evt.Action)
            {
                  case DragAction.Started:
                    e.Handled = true;
                    break;
                  case DragAction.Drop:
                    e.Handled = true;
                    var data = e.Event.ClipData.GetItemAt(0).Text;
                    break;
                  case DragAction.Ended:
 
                    e.Handled = true;
                    break;
            }

        } 
    }
}