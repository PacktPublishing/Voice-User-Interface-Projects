using System;
using System.Text;

namespace MyPodCast
{
	public static class HierarchyHelper
	{
		public const string PodcastRoot = "ROOT";
		public const string PodcastsByMonth = "BY_MONTH";
		public const string PodcastsBySearch = "BY_SEARCH";
		public const char CategorySeparator = '/';
        private const char MusicSeparator = '|';

		public static string EncodeMediaID(string podcastID, params string[] categories)
		{
			var sb = new StringBuilder();
			if(categories != null && categories.Length > 0)
            {
				sb.Append(categories [0]);
				for(var i = 1; i < categories.Length; i++)
					sb.Append(CategorySeparator).Append(categories [i]);
			}
			if(!string.IsNullOrEmpty(podcastID))
				sb.Append(MusicSeparator).Append(podcastID);

			return sb.ToString();
		}

        public static string[] GetHierarchy(string mediaId)
        {
            int pos = mediaId.IndexOf(MusicSeparator);
            if (pos >= 0)
                mediaId = mediaId.Substring(0, pos);
            return mediaId.Split(CategorySeparator);
        }

        public static string ExtractMusicIDFromMediaID(string mediaId)
		{
			int pos = mediaId.IndexOf(MusicSeparator);
			return pos >= 0 ? mediaId.Substring(pos + 1) : null;
		}
	}
}