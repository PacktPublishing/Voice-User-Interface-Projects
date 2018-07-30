using System;
using System.IO;
using Android.Content;
using Android.Media;
using Android.Media.Session;
using Android.Net;
using Android.Net.Wifi;

namespace MyPodCast
{
    public enum AudioFocusState
    {
        NoFocusAndNoHide,
        NoFocusAndCanHide,
        Focused
    }

	public class MusicPlayer :  Java.Lang.Object
        , AudioManager.IOnAudioFocusChangeListener
        , MediaPlayer.IOnCompletionListener
        , MediaPlayer.IOnErrorListener, MediaPlayer.IOnPreparedListener
        , MediaPlayer.IOnSeekCompleteListener
	{
		private MusicService _musicService;
		private WifiManager.WifiLock _wifiLock;
        private MusicProvider _musicProvider;
        private MediaPlayer _mediaPlayer;
        private bool _playOnFocusGain;		
        private volatile int _currentPosition;
        private volatile string _currentMediaId;
        private AudioFocusState _audioFocusState;
        private AudioManager _audioManager;
        public PlaybackStateCode MusicPlayerState { get; set; }
        public bool IsPlaying
        {
            get
            {
                return _playOnFocusGain ||(_mediaPlayer != null && _mediaPlayer.IsPlaying);
            }
        }
        public int CurrentStreamPosition
        {
            get
            {
                return _mediaPlayer != null ? _mediaPlayer.CurrentPosition : _currentPosition;
            }
        }

        public MusicPlayer(MusicService service, MusicProvider musicProvider)
		{
            MusicPlayerState = PlaybackStateCode.None;
            _audioFocusState = AudioFocusState.NoFocusAndNoHide;
            _musicService = service;
			_musicProvider = musicProvider;
			_audioManager = (AudioManager) service.GetSystemService(Context.AudioService);
			_wifiLock = ((WifiManager) service.GetSystemService(Context.WifiService))
				.CreateWifiLock(WifiMode.Full, "mywifilock");
            if(_mediaPlayer == null)
            {
                _mediaPlayer = new MediaPlayer();

                _mediaPlayer.SetWakeMode(_musicService.ApplicationContext,
                    Android.OS.WakeLockFlags.Partial);
                _mediaPlayer.SetOnPreparedListener(this);
                _mediaPlayer.SetOnCompletionListener(this);
                _mediaPlayer.SetOnErrorListener(this);
                _mediaPlayer.SetOnSeekCompleteListener(this);
            }
        }

		public void Stop(bool notifyListeners)
		{
			MusicPlayerState = PlaybackStateCode.Stopped;

			if(notifyListeners && _musicService != null) {
                _musicService.OnPlaybackStatusChanged(MusicPlayerState);
			}

			_currentPosition = CurrentStreamPosition;
			GiveUpAudioFocus();
			CleanUp(true);
		}			

		public void Play(MediaSession.QueueItem item) 
		{
            var mediaHasChanged = InitPlayerStates(item.Description.MediaId);

            if (MusicPlayerState == PlaybackStateCode.Paused && !mediaHasChanged && _mediaPlayer != null) {
				ConfigMediaPlayerState();
			} else {
				MusicPlayerState = PlaybackStateCode.Stopped;
				CleanUp(false);
				MediaMetadata track = _musicProvider.GetMusic(
					HierarchyHelper.ExtractMusicIDFromMediaID(item.Description.MediaId));

				string source = track.GetString(MusicProvider.PodcastSource);

				try
                {
                    _mediaPlayer.Reset();
                    MusicPlayerState = PlaybackStateCode.Buffering;
					_mediaPlayer.SetAudioStreamType(Android.Media.Stream.Music);
					_mediaPlayer.SetDataSource(source);
					_mediaPlayer.PrepareAsync();
					_wifiLock.Acquire();
                    _musicService.OnPlaybackStatusChanged(MusicPlayerState);
                }
                catch(Exception ex)
                {
					Logger.Error(ex, "Error playing song");
                    _musicService.OnError(ex.Message);
                }
			}
		}

		public void Pause()
		{
			if(MusicPlayerState == PlaybackStateCode.Playing) {
				if(_mediaPlayer != null && _mediaPlayer.IsPlaying) {
					_mediaPlayer.Pause();
					_currentPosition = _mediaPlayer.CurrentPosition;
				}
				CleanUp(false);
				GiveUpAudioFocus();
			}
			MusicPlayerState = PlaybackStateCode.Paused;
			if(_musicService != null)
                _musicService.OnPlaybackStatusChanged(MusicPlayerState);
		}

		public void SeekTo(int position)
		{
			Logger.Debug("SeekTo");

			if(_mediaPlayer == null) {
				_currentPosition = position;
			} else {
				if(_mediaPlayer.IsPlaying) {
					MusicPlayerState = PlaybackStateCode.Buffering;
				}
				_mediaPlayer.SeekTo(position);
				if(_musicService != null)
                    _musicService.OnPlaybackStatusChanged(MusicPlayerState);
			}
		}
			
		private bool InitPlayerStates(string mediaId)
		{
			Logger.Debug("GetAudioFocus");
            _playOnFocusGain = true;
           
            if (_audioFocusState != AudioFocusState.Focused) {
				var result = _audioManager.RequestAudioFocus(this, Android.Media.Stream.Music,
					AudioFocus.Gain);
				if(result == AudioFocusRequest.Granted) {
					_audioFocusState = AudioFocusState.Focused;
				}
			}

            bool mediaHasChanged = mediaId != _currentMediaId;
            if (mediaHasChanged)
            {
                _currentPosition = 0;
                _currentMediaId = mediaId;
            }

            return mediaHasChanged;
        }

		private void GiveUpAudioFocus()
		{
			Logger.Debug("GiveUpAudioFocus");
			if(_audioFocusState == AudioFocusState.Focused) {
				if(_audioManager.AbandonAudioFocus(this) == AudioFocusRequest.Granted) {
					_audioFocusState = AudioFocusState.NoFocusAndNoHide;
				}
			}
		}

        private void ConfigMediaPlayerState()
		{
			Logger.Debug("ConfigMediaPlayerState");
			if(_audioFocusState == AudioFocusState.NoFocusAndNoHide)
            {
				if(MusicPlayerState == PlaybackStateCode.Playing)
					Pause();
			}
            else
            {
				if(_audioFocusState == AudioFocusState.NoFocusAndCanHide)
					_mediaPlayer.SetVolume(0.2f, 0.2f);
				else
                    _mediaPlayer.SetVolume(1.0f, 1.0f);
                
                if(_playOnFocusGain) {
					if(_mediaPlayer != null && !_mediaPlayer.IsPlaying) {
						if(_currentPosition == _mediaPlayer.CurrentPosition) {
							_mediaPlayer.Start();
							MusicPlayerState = PlaybackStateCode.Playing;
						} else {
							_mediaPlayer.SeekTo(_currentPosition);
							MusicPlayerState = PlaybackStateCode.Buffering;
						}
					}
					_playOnFocusGain = false;
				}
			}
			if(_musicService != null)
                _musicService.OnPlaybackStatusChanged(MusicPlayerState);
		}

#region IOnAudioFocusChangeListener
        public void OnAudioFocusChange(AudioFocus focusChange)
		{
            Logger.Debug($"OnAudioFocusChange. focusChange={focusChange}");
            if (focusChange == AudioFocus.Gain)
            {
                _audioFocusState = AudioFocusState.Focused;

            }
            else if (focusChange == AudioFocus.Loss ||
              focusChange == AudioFocus.LossTransient ||
              focusChange == AudioFocus.LossTransientCanDuck)
            {
                bool canDuck = focusChange == AudioFocus.LossTransientCanDuck;
                _audioFocusState = canDuck ? AudioFocusState.NoFocusAndCanHide : AudioFocusState.NoFocusAndNoHide;

                _playOnFocusGain |= MusicPlayerState == PlaybackStateCode.Playing && !canDuck;
            }
            ConfigMediaPlayerState();
		}
#endregion
#region IOnCompletionListener
        public void OnCompletion(MediaPlayer mp)
		{
			Logger.Debug("OnCompletion");
			if(_musicService != null)
                _musicService.OnCompletion();
		}
#endregion
#region IOnErrorListener
        public bool OnError(MediaPlayer mp, MediaError what, int extra)
		{
			Logger.Error("OnError: what=" + what + ", extra=" + extra);
			if(_musicService != null)
                _musicService.OnError("MediaPlayer error " + what + "(" + extra + ")");
			return true;
		}
#endregion
#region IOnPreparedListener
        public void OnPrepared(MediaPlayer mp)
		{
			Logger.Debug("OnPrepared");
			ConfigMediaPlayerState();
		}
#endregion
#region IOnSeekCompleteListener
        public void OnSeekComplete(MediaPlayer mp)
		{
			Logger.Debug("OnSeekComplete");
			_currentPosition = mp.CurrentPosition;
			if(MusicPlayerState == PlaybackStateCode.Buffering) {
				_mediaPlayer.Start();
				MusicPlayerState = PlaybackStateCode.Playing;
			}
			if(_musicService != null)
                _musicService.OnPlaybackStatusChanged(MusicPlayerState);
		}
#endregion

		private void CleanUp(bool releaseMediaPlayer)
		{
			Logger.Debug("CleanUp");

			_musicService.StopForeground(true);

			if(releaseMediaPlayer && _mediaPlayer != null) {
				_mediaPlayer.Reset();
				_mediaPlayer.Release();
				_mediaPlayer = null;
			}

			if(_wifiLock.IsHeld) {
				_wifiLock.Release();
			}
		}
	}
}

