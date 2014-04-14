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
    /// Defines the FirstView type.
    /// </summary>
	[Activity(Label = "Utopia Engine")]
	public class TitleView : BaseView
    {
        /// <summary>
        /// Called when [create].
        /// </summary>
        /// <param name="bundle">The bundle.</param>
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
			this.SetContentView(Resource.Layout.TitleView);
        }
    }
}