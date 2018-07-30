using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace MyPodCast
{
    public class PodcastData
    {
        public string Id { get; set; }
        public string PodcastSource { get; set; }
        public string AlbumName { get; set; }
        public string Artist { get; set; }
        public string Month { get; set; }
        public string AlbumCoverSource { get; set; }
        public string Title { get; set; }
    }
}