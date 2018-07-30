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
    public class SubscriptionCallback : MediaBrowser.SubscriptionCallback
    {
        public Action<string, IList<MediaBrowser.MediaItem>> OnChildrenLoadedImpl { get; set; }

        public Action<string> OnErrorImpl { get; set; }

        public override void OnChildrenLoaded(string parentId, IList<MediaBrowser.MediaItem> children)
        {
            OnChildrenLoadedImpl(parentId, children);
        }

        public override void OnError(string id)
        {
            OnErrorImpl(id);
        }
    }
}