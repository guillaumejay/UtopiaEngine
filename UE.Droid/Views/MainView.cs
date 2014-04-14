// --------------------------------------------------------------------------------------------------------------------
// <summary>
//    Defines the MainView type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace UE.Droid.Views
{
    using Android.App;
    using Android.OS;

    /// <summary>
    /// Mainview type.
    /// </summary>
	[Activity(Label = "What do you want ?")]
	public class MainView : BaseView
    {
        /// <summary>
        /// Called when [create].
        /// </summary>
        /// <param name="bundle">The bundle.</param>
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
			this.SetContentView(Resource.Layout.MainView);
        }
    }
}