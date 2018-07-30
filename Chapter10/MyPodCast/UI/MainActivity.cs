using Android.App;

namespace MyPodCast
{
	[Activity(Label = "Podcast By Henry", MainLauncher = true)]
	public class MainActivity : Activity, IMediaSelected
    {
		protected override void OnCreate(Android.OS.Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			SetContentView(Resource.Layout.main_activity);
			if(savedInstanceState == null)
            {
				FragmentManager.BeginTransaction()
					.Add(Resource.Id.containerMain, new MediaBrowsesFragment(string.Empty))
					.Commit();
			}
        }

		public void OnMediaItemSelected(Android.Media.Browse.MediaBrowser.MediaItem item)
		{
			if(item.IsPlayable)
            {
				MediaController.GetTransportControls().PlayFromMediaId(item.MediaId, null);
				FragmentManager.BeginTransaction()
					.Replace(Resource.Id.containerMain, new MediaPlayerFragment())
					.AddToBackStack(null)
					.Commit();
			}
            else if(item.IsBrowsable)
            {
				FragmentManager.BeginTransaction()
					.Replace(Resource.Id.containerMain, new MediaBrowsesFragment(item.MediaId))
					.AddToBackStack(null)
					.Commit();
			}
		}
	}
}

