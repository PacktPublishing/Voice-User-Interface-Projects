using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Media.Browse;
using Android.Media.Session;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace MyPodCast
{
	public class MediaBrowsesFragment : Fragment
	{
		private const string ArgMediaId = "media_id";
		private string _mediaId;
        private MediaBrowser _mediaBrowser;
        private BrowseAdapter _browserAdapter;
        private SubscriptionCallback _subscriptionCallback;
        private ConnectionCallback _connectionCallback;

		public MediaBrowsesFragment(string mediaId)
		{
            _subscriptionCallback = new SubscriptionCallback();
            _connectionCallback = new ConnectionCallback();
            var args = new Bundle();
			args.PutString(ArgMediaId, mediaId);
			this.Arguments = args;
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			View rootView = inflater.Inflate(Resource.Layout.media_player, container, false);

			_browserAdapter = new BrowseAdapter(Activity);

			View controls = rootView.FindViewById(Resource.Id.media_player);
			controls.Visibility = ViewStates.Gone;

			var listView = rootView.FindViewById<ListView>(Resource.Id.list_view);
			listView.Adapter = _browserAdapter;

			listView.ItemClick += (sender, e) => {
				MediaBrowser.MediaItem item = _browserAdapter.GetItem(e.Position);
                var listener = (IMediaSelected)Activity;
                listener.OnMediaItemSelected(item);
            };

			Bundle args = Arguments;
			_mediaId = args.GetString(ArgMediaId, null);
			_mediaBrowser = new MediaBrowser(Activity,
				new ComponentName(Activity, Java.Lang.Class.FromType(typeof(MusicService))),
				_connectionCallback, null);
			
			_subscriptionCallback.OnChildrenLoadedImpl = (parentId, children) => {
				_browserAdapter.Clear();
				_browserAdapter.NotifyDataSetInvalidated();
				foreach(MediaBrowser.MediaItem item in children) {
					_browserAdapter.Add(item);
				}
				_browserAdapter.NotifyDataSetChanged();
			};

			_subscriptionCallback.OnErrorImpl = (id) => Toast.MakeText(Activity, "Error Loading Media", ToastLength.Long).Show();
			_connectionCallback.OnConnectedImpl = () => {
				if(string.IsNullOrEmpty(_mediaId))
					_mediaId = _mediaBrowser.Root;
				_mediaBrowser.Subscribe(_mediaId, _subscriptionCallback);
				if(_mediaBrowser.SessionToken == null)
					throw new ArgumentNullException("No Session token");
				var mediaController = new Android.Media.Session.MediaController(Activity, _mediaBrowser.SessionToken);
				Activity.MediaController = mediaController;
			};
			_connectionCallback.OnConnectionFailedImpl = () => Logger.Debug("OnConnectionFailedImpl");
			_connectionCallback.OnConnectionSuspendedImpl = () => 
            {
				Activity.MediaController = null;
			};
			return rootView;
		}

		public override void OnStart()
		{
			base.OnStart();
			_mediaBrowser.Connect();
		}

		public override void OnStop()
		{
			base.OnStop();
			_mediaBrowser.Disconnect();
		}
	}
}

