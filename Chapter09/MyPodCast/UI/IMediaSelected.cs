using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Media.Browse;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace MyPodCast
{
    public interface IMediaSelected
    {
        void OnMediaItemSelected(MediaBrowser.MediaItem item);
    }
}