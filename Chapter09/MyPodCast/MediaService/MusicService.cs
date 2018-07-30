using System;
using System.Collections.Generic;
using Android.Media.Session;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Media;
using Android.Media.Browse;
using Android.Runtime;

namespace MyPodCast
{
	[Service(Exported = true, Label = "Henry Podcast Service", Name = "com.henry.mypodcast.service")]
	[IntentFilter(new[] { "android.media.browse.MediaBrowserService" })]
	public class MusicService : Android.Service.Media.MediaBrowserService
	{        
        private const string CustomActionFavorite = "com.henry.mypodcast.FAVORITE";        
        private List<MediaSession.QueueItem> _playingQueue;
        private int _currentIndexQueue;
		private bool _isStarted;
        private MediaSession _session;
        private MusicPlayer _musicPlayer;
        private MusicProvider _musicProvider;
        private PackageFinder _packageFinder;

        public MusicService()
		{
		}

		public override void OnCreate()
		{
			base.OnCreate();	
			_playingQueue = new List<MediaSession.QueueItem>();
			_musicProvider = new MusicProvider();
            _packageFinder = new PackageFinder();
            _musicPlayer = new MusicPlayer(this, _musicProvider);

            var mediaCallback = this.CreateMediaSessionCallback();

            _session = new MediaSession(this, "HenryPodcast");
            SessionToken = _session.SessionToken;
            _session.SetCallback(mediaCallback);
			_session.SetFlags(MediaSessionFlags.HandlesMediaButtons |
			    MediaSessionFlags.HandlesTransportControls);

            Context context = ApplicationContext;
			var intent = new Intent(context, typeof(MainActivity));
			var pendingIntent = PendingIntent.GetActivity(context, 99, intent, PendingIntentFlags.UpdateCurrent);
			_session.SetSessionActivity(pendingIntent);

            var extraBundle = new Bundle();
            extraBundle.PutBoolean("com.google.android.gms.car.media.ALWAYS_RESERVE_SPACE_FOR.ACTION_QUEUE", true);
            extraBundle.PutBoolean("com.google.android.gms.car.media.ALWAYS_RESERVE_SPACE_FOR.ACTION_SKIP_TO_PREVIOUS", true);
            extraBundle.PutBoolean("com.google.android.gms.car.media.ALWAYS_RESERVE_SPACE_FOR.ACTION_SKIP_TO_NEXT", true);
            extraBundle.PutBoolean("com.google.android.gms.car.media.ALWAYS_RESERVE_SPACE_FOR.ACTION_PLAY_PAUSE", true); 
            _session.SetExtras(extraBundle);
			UpdatePlaybackState(null);
		}

		public override void OnDestroy()
		{
			Logger.Debug("OnDestroy");
			OnStop(null);
			_session.Release();
		}

		public override BrowserRoot OnGetRoot(string clientPackageName, int clientUid, Bundle rootHints)
		{
			Logger.Debug($"OnGetRoot: clientPackageName={clientPackageName}");
            if(_packageFinder.Find(clientPackageName))
            {
                return new BrowserRoot(HierarchyHelper.PodcastRoot, null);
            }
            else
            {
                Logger.Warn($"OnGetRoot: clientPackageName={clientPackageName} ignored");
                return null;
            }
		}

		public override void OnLoadChildren(string parentId, Result result)
		{
			if(!_musicProvider.IsInitialized)
            {
				result.Detach();

				_musicProvider.RetrieveMedia(success => {
					if(success) {
						LoadChildrenImpl(parentId, result);
					} else {
						UpdatePlaybackState("Unable to get the data.");
						result.SendResult(new JavaList<MediaBrowser.MediaItem>());
					}
				});

			}
            else
            {
				LoadChildrenImpl(parentId, result);
			}
		}

        public void OnCompletion()
        {
            if (_playingQueue != null && _playingQueue.Count != 0)
            {
                _currentIndexQueue++;
                if (_currentIndexQueue >= _playingQueue.Count)
                {
                    _currentIndexQueue = 0;
                }
                HandlePlayRequest();
            }
            else
            {
                OnStop(null);
            }
        }
       
        public bool isIndexPlayable(int index, List<MediaSession.QueueItem> queue)
        {
            return (queue != null && index >= 0 && index < queue.Count);
        }

        public void OnPlaybackStatusChanged(PlaybackStateCode state)
		{
			UpdatePlaybackState(null);
		}

		public void OnError(string error)
		{
			UpdatePlaybackState(error);
		}

        private void LoadChildrenImpl(string parentId, Result result)
        {
            var mediaItems = new JavaList<MediaBrowser.MediaItem>();

            if (HierarchyHelper.PodcastRoot == parentId)
            {
                Logger.Debug("Load ROOT");
                mediaItems.Add(new MediaBrowser.MediaItem(
                    new MediaDescription.Builder()
                        .SetMediaId(HierarchyHelper.PodcastsByMonth)
                        .SetTitle("All Podcasts")
                        .SetIconUri(Android.Net.Uri.Parse("android.resource://com.henry.mypodcast/drawable/ic_by_genre"))
                        .SetSubtitle("Podcasts By Month")
                        .Build(), MediaItemFlags.Browsable));

            }
            else if (HierarchyHelper.PodcastsByMonth == parentId)
            {
                Logger.Debug("Load BYMONTH List");
                foreach (var month in _musicProvider.Months)
                {
                    var item = new MediaBrowser.MediaItem(
                        new MediaDescription.Builder()
                            .SetMediaId(HierarchyHelper.PodcastsByMonth + HierarchyHelper.CategorySeparator + month)
                            .SetTitle(month)
                            .SetSubtitle($"{month} Podcasts")
                            .Build(), MediaItemFlags.Browsable);
                    mediaItems.Add(item);
                }
            }
            else if (parentId.StartsWith(HierarchyHelper.PodcastsByMonth))
            {
                var month = HierarchyHelper.GetHierarchy(parentId)[1];
                Logger.Debug("Load List of Podcasts for Month");
                foreach (var track in _musicProvider.GetMusicsByMonth(month))
                {
                    var hierarchyAwareMediaID = HierarchyHelper.EncodeMediaID(
                                                    track.Description.MediaId, HierarchyHelper.PodcastsByMonth, month);
                    var trackCopy = new MediaMetadata.Builder(track)
                        .PutString(MediaMetadata.MetadataKeyMediaId, hierarchyAwareMediaID)
                        .Build();
                    var bItem = new MediaBrowser.MediaItem(
                                    trackCopy.Description, MediaItemFlags.Playable);
                    mediaItems.Add(bItem);
                }
            }
            result.SendResult(mediaItems);
        }

        private void HandlePlayRequest()
        {
            if (!_isStarted)
            {
                Logger.Verbose("Starting podcast service");
                StartService(new Intent(ApplicationContext, typeof(MusicService)));
                _isStarted = true;
            }

            if (!_session.Active)
                _session.Active = true;

            if (this.isIndexPlayable(_currentIndexQueue, _playingQueue))
            {
                UpdateMetadata();
                _musicPlayer.Play(_playingQueue[_currentIndexQueue]);
            }
        }

        private void OnPause()
        {
            _musicPlayer.Pause();
        }

        private void OnStop(String message)
        {
            _musicPlayer.Stop(true);
            UpdatePlaybackState(message);

            // service is no longer necessary. Will be started again if needed.
            StopSelf();
            _isStarted = false;
        }

        private void UpdateMetadata()
        {
            if (!this.isIndexPlayable(_currentIndexQueue, _playingQueue))
            {
                Logger.Error("Can't retrieve current metadata.");
                UpdatePlaybackState("Error no data.");
                return;
            }
            MediaSession.QueueItem queueItem = _playingQueue[_currentIndexQueue];
            string musicId = HierarchyHelper.ExtractMusicIDFromMediaID(queueItem.Description.MediaId);
            MediaMetadata track = _musicProvider.GetMusic(musicId);
            string trackId = track.GetString(MediaMetadata.MetadataKeyMediaId);
            if (musicId != trackId)
            {
                var e = new InvalidOperationException("track ID should match musicId.");
                throw e;
            }
            Logger.Debug($"Updating metadata for MusicID= {musicId}");
            _session.SetMetadata(track);
        }

        private void UpdatePlaybackState(String error)
        {
            var position = PlaybackState.PlaybackPositionUnknown;
            if (_musicPlayer != null)
            {
                position = _musicPlayer.CurrentStreamPosition;
            }

            var stateBuilder = new PlaybackState.Builder()
                .SetActions(GetAvailableActions());

            SetCustomAction(stateBuilder);

            if (error != null)
            {
                stateBuilder.SetErrorMessage(error);
                stateBuilder.SetState(PlaybackStateCode.Error, position, 1.0f, SystemClock.ElapsedRealtime());
            }
            else
                stateBuilder.SetState(_musicPlayer.MusicPlayerState, position, 1.0f, SystemClock.ElapsedRealtime());

            if (this.isIndexPlayable(_currentIndexQueue, _playingQueue))
            {
                var item = _playingQueue[_currentIndexQueue];
                stateBuilder.SetActiveQueueItemId(item.QueueId);
            }

            _session.SetPlaybackState(stateBuilder.Build());
        }

        private void SetCustomAction(PlaybackState.Builder stateBuilder)
        {
            MediaMetadata currentMusic = GetCurrentPlayingMusic();
            if (currentMusic != null)
            {
                // Set appropriate "Favorite" icon on Custom action:
                var musicId = currentMusic.GetString(MediaMetadata.MetadataKeyMediaId);
                var favoriteIcon = Resource.Drawable.ic_star_off;
                if (_musicProvider.IsFavorite(musicId))
                {
                    favoriteIcon = Resource.Drawable.ic_star_on;
                }
                stateBuilder.AddCustomAction(CustomActionFavorite, "Favorite", favoriteIcon);
            }
        }

        private long GetAvailableActions()
        {
            long actions = PlaybackState.ActionPlay | PlaybackState.ActionPlayFromMediaId |
                           PlaybackState.ActionPlayFromSearch;
            if (_playingQueue == null || _playingQueue.Count == 0)
            {
                return actions;
            }
            if (_musicPlayer.IsPlaying)
            {
                actions |= PlaybackState.ActionPause;
            }
            if (_currentIndexQueue > 0)
            {
                actions |= PlaybackState.ActionSkipToPrevious;
            }
            if (_currentIndexQueue < _playingQueue.Count - 1)
            {
                actions |= PlaybackState.ActionSkipToNext;
            }
            return actions;
        }

        private MediaMetadata GetCurrentPlayingMusic()
        {
            if (this.isIndexPlayable(_currentIndexQueue, _playingQueue))
            {
                var item = _playingQueue[_currentIndexQueue];
                if (item != null)
                {
                    Logger.Debug("GetCurrentPlayingMusic");
                    return _musicProvider.GetMusic(
                        HierarchyHelper.ExtractMusicIDFromMediaID(item.Description.MediaId));
                }
            }
            return null;
        }

        private MediaSessionCallback CreateMediaSessionCallback()
        {
            var mediaCallback = new MediaSessionCallback();
            mediaCallback.OnPlayImpl = () => {
                Logger.Debug("OnPlayImpl");

                if (_playingQueue == null || _playingQueue.Count == 0)
                {
                    _playingQueue = new List<MediaSession.QueueItem>(_musicProvider.GetRandomQueue());
                    _session.SetQueue(_playingQueue);
                    _session.SetQueueTitle("Random music");
                    _currentIndexQueue = 0;
                }

                if (_playingQueue != null && _playingQueue.Count != 0)
                {
                    HandlePlayRequest();
                }
            };
            mediaCallback.OnSkipToQueueItemImpl = (id) => {
                Logger.Debug("OnSkipToQueueItem");

                if (_playingQueue != null && _playingQueue.Count != 0)
                {
                    _currentIndexQueue = -1;
                    int index = 0;
                    foreach (var item in _playingQueue)
                    {
                        if (id == item.QueueId)
                            _currentIndexQueue = index;
                        index++;
                    }
                    HandlePlayRequest();
                }
            };
            mediaCallback.OnSeekToImpl = (pos) => {
                Logger.Debug("OnSeekToImpl:");
                _musicPlayer.SeekTo((int)pos);
            };
            mediaCallback.OnPlayFromMediaIdImpl = (mediaId, extras) => {
                Logger.Debug($"OnPlayFromMediaIdImpl mediaId: {mediaId}");

                _playingQueue = _musicProvider.GetPlayingQueue(mediaId);
                _session.SetQueue(_playingQueue);

                string[] hierarchies = HierarchyHelper.GetHierarchy(mediaId);
                string month = hierarchies != null && hierarchies.Length == 2 ? hierarchies[1] : string.Empty;
                var queueTitle = $"{month} Podcasts";

                _session.SetQueueTitle(queueTitle);

                if (_playingQueue != null && _playingQueue.Count != 0)
                {
                    _currentIndexQueue = -1;
                    int index = 0;
                    foreach (var item in _playingQueue)
                    {
                        if (mediaId == item.Description.MediaId)
                            _currentIndexQueue = index;
                        index++;
                    }

                    if (_currentIndexQueue < 0)
                        Logger.Error($"OnPlayFromMediaIdImpl: media ID {mediaId} not be found.");
                    else
                        HandlePlayRequest();
                }
            };
            mediaCallback.OnPauseImpl = () => {
                OnPause();
            };
            mediaCallback.OnStopImpl = () => {
                OnStop(null);
            };
            mediaCallback.OnSkipToNextImpl = () => {
                Logger.Debug("OnSkipToNextImpl");
                _currentIndexQueue++;
                if (_playingQueue != null && _currentIndexQueue >= _playingQueue.Count)
                {
                    _currentIndexQueue = 0;
                }
                if (this.isIndexPlayable(_currentIndexQueue, _playingQueue))
                {
                    HandlePlayRequest();
                }
                else
                {
                    OnStop("Cannot skip");
                }
            };
            mediaCallback.OnSkipToPreviousImpl = () => {
                Logger.Debug("OnSkipToPreviousImpl");
                _currentIndexQueue--;
                if (_playingQueue != null && _currentIndexQueue < 0)
                {
                    _currentIndexQueue = 0;
                }
                if (this.isIndexPlayable(_currentIndexQueue, _playingQueue))
                {
                    HandlePlayRequest();
                }
                else
                {
                    OnStop("Cannot skip");
                }
            };
            mediaCallback.OnCustomActionImpl = (action, extras) => {
                if (CustomActionFavorite == action)
                {
                    Logger.Info("OnCustomActionImpl");
                    var track = GetCurrentPlayingMusic();
                    if (track != null)
                    {
                        var musicId = track.GetString(MediaMetadata.MetadataKeyMediaId);
                        _musicProvider.SetFavorite(musicId, !_musicProvider.IsFavorite(musicId));
                    }
                    UpdatePlaybackState(null);
                }
                else
                {
                    Logger.Error($"Unsupported action: {action}");
                }
            };
            mediaCallback.OnPlayFromSearchImpl = (query, extras) => {
                Logger.Debug($"OnPlayFromSearchImpl  query={query}");

                if (string.IsNullOrEmpty(query))
                {
                    _playingQueue = new List<MediaSession.QueueItem>(_musicProvider.GetRandomQueue());
                }
                else
                {
                    _playingQueue = new List<MediaSession.QueueItem>(_musicProvider.GetPlayingQueueFromSearch(query));
                }

                _session.SetQueue(_playingQueue);

                if (_playingQueue != null && _playingQueue.Count != 0)
                {
                    _currentIndexQueue = 0;

                    HandlePlayRequest();
                }
                else
                {
                    OnStop("0 Found.");
                }
            };
            return mediaCallback;
        }
    }
}

