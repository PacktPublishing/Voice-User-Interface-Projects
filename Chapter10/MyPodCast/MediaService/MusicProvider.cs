using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Android.Graphics;
using Android.Media;
using Android.Media.Session;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using SkiaSharp;
using SkiaSharp.Views.Android;

namespace MyPodCast
{
    public enum State
    {
        NotInitialized,
        Initializing,
        Initialized
    };

    public class MusicProvider
	{
		public const string PodcastSource = "PODCASTS_SOURCE";
        private Dictionary<string, List<MediaMetadata>> _musicListByMonths;
        private Dictionary<string, MediaMetadata> _musicListById;
		private List<string> _favorites;
		private volatile State _currentState = State.NotInitialized;
        public List<string> Months
        {
            get
            {
                return _currentState != State.Initialized ? new List<string>() : new List<string>(_musicListByMonths.Keys);
            }
        }
        public bool IsInitialized
        {
            get
            {
                return _currentState == State.Initialized;
            }
        }


        public MusicProvider()
		{
			_musicListByMonths = new Dictionary<string, List<MediaMetadata>>();
			_musicListById = new Dictionary<string, MediaMetadata>();
			_favorites = new List<string>();
		}


		public IEnumerable<MediaMetadata> GetMusicsByMonth(string month)
		{
			if(_currentState != State.Initialized || !_musicListByMonths.ContainsKey(month))
				return new List<MediaMetadata>();

			return _musicListByMonths [month];
		}

		public IEnumerable<MediaMetadata> SearchMusic(string titleQuery)
		{
			if(_currentState != State.Initialized)
				return new List<MediaMetadata>();
			
			var result = new List<MediaMetadata>();
			titleQuery = titleQuery.ToLower();
			foreach(var track in _musicListById.Values)
            {
				if(track.GetString(MediaMetadata.MetadataKeyTitle).ToLower().Contains(titleQuery))
					result.Add(track);				
			}
			return result;
		}

		public MediaMetadata GetMusic(string mediaId)
		{
			return _musicListById.ContainsKey(mediaId) ? _musicListById[mediaId] : null;
		}

		public void SetFavorite(string mediaId, bool favorite)
		{
			if(favorite)
				_favorites.Add(mediaId);
			else
				_favorites.Remove(mediaId);
		}

		public bool IsFavorite(string mediaId)
		{
			return _favorites.Contains(mediaId);
		}

		public void RetrieveMedia(Action<bool> callback)
		{
			Logger.Debug("RetrieveMedia");
			if(_currentState == State.Initialized) {
				callback(true);
				return;
			}

            try
            {
                if (_currentState == State.NotInitialized)
                {
                    _currentState = State.Initializing;
                    GetSource();
                    BuildListsByMonths();
                    _currentState = State.Initialized;
                }
            }
            catch (Exception)
            {
                _currentState = State.NotInitialized;
            }
            callback(_currentState == State.Initialized);
		}

		private void BuildListsByMonths()
		{
            _musicListByMonths = new Dictionary<string, List<MediaMetadata>>();
			foreach(var m in _musicListById.Values)
            {
				var month = m.GetString(MediaMetadata.MetadataKeyGenre);
                if(_musicListByMonths.ContainsKey(month))
                    _musicListByMonths[month].Add(m);
                else
                    _musicListByMonths.Add(month, new List<MediaMetadata> { m });
			}
		}
        private void GetSource()
        {
            JsonConvert.DefaultSettings = () =>
            {
                var settings = new JsonSerializerSettings();
                settings.NullValueHandling = NullValueHandling.Ignore;
                settings.DefaultValueHandling = DefaultValueHandling.Ignore;
                settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                return settings;
            };
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("mysecret", "12345");
                var message = httpClient.GetAsync("https://myhenrytestapp.azurewebsites.net/podcasts").GetAwaiter().GetResult();
                var result = message.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var sources = JsonConvert.DeserializeObject<List<PodcastData>>(result);
                foreach (var data in sources)
                {
                    List<Bitmap> images = GetImage(800, 480, 128, 128, data.AlbumCoverSource);
                    _musicListById.Add(data.Id, new MediaMetadata.Builder()
                        .PutString(MediaMetadata.MetadataKeyMediaId, data.Id)
                        .PutString(PodcastSource, data.PodcastSource)
                        .PutString(MediaMetadata.MetadataKeyAlbum, data.AlbumName)
                        .PutString(MediaMetadata.MetadataKeyArtist, data.Artist)
                        .PutString(MediaMetadata.MetadataKeyGenre, data.Month)
                        .PutString(MediaMetadata.MetadataKeyAlbumArtUri, data.AlbumCoverSource)
                        .PutString(MediaMetadata.MetadataKeyTitle, data.Title)
                        .PutBitmap(MediaMetadata.MetadataKeyAlbumArt, images[0])
                        .PutBitmap(MediaMetadata.MetadataKeyDisplayIcon, images[1])
                        .Build());
                }
            }

        }

        private void GetSource_from_chapter_9()
        {
            List<PodcastData> sources = new List<PodcastData>()
            {
                new PodcastData() {
                    Id = "1",
                    PodcastSource = "http://storage.googleapis.com/automotive-media/Jazz_In_Paris.mp3",
                    AlbumName = "Xamarin Album",
                    Artist = "Henry Lee",
                    Month = "January",
                    AlbumCoverSource = "http://storage.googleapis.com/automotive-media/album_art.jpg",
                    Title = "Henry Test Title",
                },
                new PodcastData() {
                    Id = "2",
                    PodcastSource = "http://storage.googleapis.com/automotive-media/Jazz_In_Paris.mp3",
                    AlbumName = "Xamarin Album",
                    Artist = "Henry Lee",
                    Month = "January",
                    AlbumCoverSource = "http://storage.googleapis.com/automotive-media/album_art.jpg",
                    Title = "Henry2 Test Title",
                },
                new PodcastData() {
                    Id = "3",
                    PodcastSource = "http://storage.googleapis.com/automotive-media/Jazz_In_Paris.mp3",
                    AlbumName = "Xamarin Album",
                    Artist = "Henry Lee",
                    Month = "January",
                    AlbumCoverSource = "http://storage.googleapis.com/automotive-media/album_art.jpg",
                    Title = "Henry3 Test Title",
                },
                new PodcastData() {
                    Id = "4",
                    PodcastSource = "http://storage.googleapis.com/automotive-media/Jazz_In_Paris.mp3",
                    AlbumName = "Xamarin Album",
                    Artist = "Henry Lee",
                    Month = "March",
                    AlbumCoverSource = "http://storage.googleapis.com/automotive-media/album_art.jpg",
                    Title = "Henry4 Test Title",
                }

            };

            foreach (var data in sources)
            {
                List<Bitmap> images = GetImage(800, 480, 128, 128, data.AlbumCoverSource);
                _musicListById.Add(data.Id, new MediaMetadata.Builder()
                    .PutString(MediaMetadata.MetadataKeyMediaId, data.Id)
                    .PutString(PodcastSource, data.PodcastSource)
                    .PutString(MediaMetadata.MetadataKeyAlbum, data.AlbumName)
                    .PutString(MediaMetadata.MetadataKeyArtist, data.Artist)
                    .PutString(MediaMetadata.MetadataKeyGenre, data.Month)
                    .PutString(MediaMetadata.MetadataKeyAlbumArtUri, data.AlbumCoverSource)
                    .PutString(MediaMetadata.MetadataKeyTitle, data.Title)
                    .PutBitmap(MediaMetadata.MetadataKeyAlbumArt, images[0])
                    .PutBitmap(MediaMetadata.MetadataKeyDisplayIcon, images[1])
                    //.PutLong(MediaMetadata.MetadataKeyDuration, duration)
                    //.PutLong(MediaMetadata.MetadataKeyTrackNumber, trackNumber)
                    //.PutLong(MediaMetadata.MetadataKeyNumTracks, totalTrackCount)
                    .Build());
            }            
        }


        private static List<Bitmap> GetImage(int width, int height, int iconWidth, int iconHeight, string url)
        {
            List<Bitmap> images = new List<Bitmap>();

            using(WebClient client = new WebClient())
            {
                byte[] data = client.DownloadData(url);
                using(var original = SKBitmap.Decode(data))
                {
                    using(var resized = original.Resize(new SKImageInfo(width, height), SKBitmapResizeMethod.Lanczos3))
                    {
                        images.Add(resized.ToBitmap());
                    }
                    using(var resized = original.Resize(new SKImageInfo(iconWidth, iconHeight), SKBitmapResizeMethod.Lanczos3))
                    {
                        images.Add(resized.ToBitmap());
                    }
                }
            }

            return images;
        }

        public List<MediaSession.QueueItem> GetRandomQueue()
        {
            List<string> months = this.Months;

            if (months.Count <= 1)
                return new List<MediaSession.QueueItem>();

            string month = months[0];
            IEnumerable<MediaMetadata> tracks = this.GetMusicsByMonth(month);

            return ConvertToQueue(tracks, HierarchyHelper.PodcastsByMonth, month);
        }

        private List<MediaSession.QueueItem> ConvertToQueue(IEnumerable<MediaMetadata> tracks, params string[] categories)
        {
            var queue = new List<MediaSession.QueueItem>();
            int count = 0;
            foreach (var track in tracks)
            {
                string hierarchyAwareMediaID = HierarchyHelper.EncodeMediaID(track.Description.MediaId, categories);
                MediaMetadata trackCopy = new MediaMetadata.Builder(track)
                    .PutString(MediaMetadata.MetadataKeyMediaId, hierarchyAwareMediaID)
                    .Build();

                var item = new MediaSession.QueueItem(trackCopy.Description, count++);
                queue.Add(item);
            }
            return queue;

        }

        public List<MediaSession.QueueItem> GetPlayingQueue(string mediaId)
        {
            string[] hierarchy = HierarchyHelper.GetHierarchy(mediaId);

            if (hierarchy.Length != 2)
                return null;

            string categoryType = hierarchy[0];
            string categoryValue = hierarchy[1];

            IEnumerable<MediaMetadata> tracks = null;
            if (categoryType == HierarchyHelper.PodcastsByMonth)
                tracks = this.GetMusicsByMonth(categoryValue);
            else if (categoryType == HierarchyHelper.PodcastsBySearch)
                tracks = this.SearchMusic(categoryValue);

            if (tracks == null)
                return null;

            return ConvertToQueue(tracks, hierarchy[0], hierarchy[1]);
        }

        public List<MediaSession.QueueItem> GetPlayingQueueFromSearch(string query)
        {
            return ConvertToQueue(this.SearchMusic(query), HierarchyHelper.PodcastsBySearch, query);
        }
    }
}

