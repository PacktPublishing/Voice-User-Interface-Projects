using System;
using Android.App;
using Android.Widget;
using Android.Media.Browse;
using Android.Media.Session;
using System.Collections.Generic;
using Android.Content;
using System.Text;
using Android.Views;
using Android.Runtime;
using System.Linq;

namespace MyPodCast
{
	public class MediaPlayerFragment : Fragment
	{
		private Button _btnNext;
        private Button _btnPrevious;
        private Button _btnPlay;

        private MediaBrowser _mediaBrowser;
        private Android.Media.Session.MediaController.TransportControls _transportControls;
        private Android.Media.Session.MediaController _mediaController;
        private PlaybackState _playbackState;
        private QueueAdapter _queueAdapter;
        private ConnectionCallback _connectionCallback;
        private SessionCallback _sessionCallback;

		public MediaPlayerFragment()
		{
            _connectionCallback = new ConnectionCallback();
            _sessionCallback = new SessionCallback();
			_connectionCallback.OnConnectedImpl = () => 
            {

				if(_mediaBrowser.SessionToken == null)
					throw new InvalidOperationException("No Session token");

				_mediaController = new Android.Media.Session.MediaController(Activity,
					_mediaBrowser.SessionToken);
				_transportControls = _mediaController.GetTransportControls();
				_mediaController.RegisterCallback(_sessionCallback);

				Activity.MediaController = _mediaController;
				_playbackState = _mediaController.PlaybackState;

				var queue = (JavaList)_mediaController.Queue;
				if(queue != null)
                {
					_queueAdapter.Clear();
					_queueAdapter.NotifyDataSetInvalidated();
					_queueAdapter.AddAll(queue.ToArray());
					_queueAdapter.NotifyDataSetChanged();
				}
				OnPlaybackStateChanged(_playbackState);
			};
			_connectionCallback.OnConnectionFailedImpl = () => Logger.Debug("OnConnectionFailedImpl");
			_connectionCallback.OnConnectionSuspendedImpl = () => 
            {
				_mediaController.UnregisterCallback(_sessionCallback);
				_transportControls = null;
				_mediaController = null;
				Activity.MediaController = null;
			};
			_sessionCallback.OnSessionDestroyedImpl = () => Logger.Debug("OnSessionDestroyedImpl");
			_sessionCallback.OnPlaybackStateChangedImpl = state => 
            {
				if(state == null) {
					return;
				}
				_playbackState = state;
				OnPlaybackStateChanged(state);
			};
			_sessionCallback.OnQueueChangedImpl = queue => 
            {
				if(queue != null) {
					_queueAdapter.Clear();
					_queueAdapter.NotifyDataSetInvalidated();
					_queueAdapter.AddAll(queue.ToArray());
					_queueAdapter.NotifyDataSetChanged();
				}
			};
		}

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Android.OS.Bundle savedInstanceState)
		{
			View rootView = inflater.Inflate(Resource.Layout.media_player, container, false);

            _btnPrevious = rootView.FindViewById<Button>(Resource.Id.btnPrevious);
            _btnPrevious.Enabled = false;
            _btnPrevious.Click += OnClick;

            _btnNext = rootView.FindViewById<Button>(Resource.Id.btnNext);
            _btnNext.Enabled = false;
            _btnNext.Click += OnClick;

            _btnPlay = rootView.FindViewById<Button>(Resource.Id.btnPlay);
            _btnPlay.Enabled = true;
            _btnPlay.Click += OnClick;

			_queueAdapter = new QueueAdapter(Activity);

			var listView = rootView.FindViewById<ListView>(Resource.Id.list_view);
			listView.Adapter = _queueAdapter;
			listView.Focusable = true;

			listView.ItemClick += (sender, e) => 
            {
				var item = _queueAdapter.GetItem(e.Position);
				_transportControls.SkipToQueueItem(item.QueueId);
			};

			_mediaBrowser = new MediaBrowser(Activity,
				new ComponentName(Activity, Java.Lang.Class.FromType(typeof(MusicService))),
				_connectionCallback, null);

			return rootView;
		}

		public override void OnResume()
		{
			base.OnResume();
			if(_mediaBrowser != null)
				_mediaBrowser.Connect();
		}

		public override void OnPause()
		{
			base.OnPause();
			if(_mediaController != null)
				_mediaController.UnregisterCallback(_sessionCallback);
			if(_mediaBrowser != null)
				_mediaBrowser.Disconnect();
		}

        private void OnPlaybackStateChanged(PlaybackState state)
		{
			if(state == null) return;
			_queueAdapter.ActiveQueueItemId = state.ActiveQueueItemId;
			_queueAdapter.NotifyDataSetChanged();
			var enablePlay = false;
			var statusBuilder = new StringBuilder();

			switch(state.State)
            {
			    case PlaybackStateCode.Playing:
				    statusBuilder.Append("playing");
				    enablePlay = false;
				    break;
			    case PlaybackStateCode.Paused:
				    statusBuilder.Append("paused");
				    enablePlay = true;
				    break;
			    case PlaybackStateCode.Stopped:
				    statusBuilder.Append("ended");
				    enablePlay = true;
				    break;
			    case PlaybackStateCode.Error:
				    statusBuilder.Append("error: ").Append(state.ErrorMessage);
				    break;
			    case PlaybackStateCode.Buffering:
				    statusBuilder.Append("buffering");
				    break;
			    case PlaybackStateCode.None:
				    statusBuilder.Append("none");
				    enablePlay = false;
				    break;
			    case PlaybackStateCode.Connecting:
				    statusBuilder.Append("connecting");
				    break;
			    default:
				    statusBuilder.Append(_playbackState);
				    break;
			}

			statusBuilder.Append(" -- At position: ").Append(state.Position);

			if(enablePlay)
                _btnPlay.Text = "Play";
			else
                _btnPlay.Text = "Pause";

			_btnPrevious.Enabled = (state.Actions & PlaybackState.ActionSkipToPrevious) != 0;
            _btnNext.Enabled = (state.Actions & PlaybackState.ActionSkipToNext) != 0;
		}

		public void OnClick(object sender, EventArgs e)
		{
			var v = (View)sender;
			PlaybackStateCode state = _playbackState == null ?
				PlaybackStateCode.None : _playbackState.State;
			
			switch(v.Id)
            {
			    case Resource.Id.btnPlay:
				    if(state == PlaybackStateCode.Paused
					    || state == PlaybackStateCode.Stopped
					    || state == PlaybackStateCode.None)
                    {
					    PlayMedia();
				    }
                    else if(state == PlaybackStateCode.Playing)
                    {
					    PauseMedia();
				    }
				    break;
			    case Resource.Id.btnPrevious:
				    SkipToPrevious();
				    break;
			    case Resource.Id.btnNext:
				    SkipToNext();
				    break;
			}
		}

        private void PlayMedia()
		{
			if(_transportControls != null)
				_transportControls.Play();
		}

        private void PauseMedia()
		{
			if(_transportControls != null)
				_transportControls.Pause();
		}

        private void SkipToPrevious()
		{
			if(_transportControls != null)
				_transportControls.SkipToPrevious();
		}

		private void SkipToNext()
		{
			if(_transportControls != null)
				_transportControls.SkipToNext();
		}
	}
}

