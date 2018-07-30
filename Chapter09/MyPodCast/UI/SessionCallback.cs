using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Media.Session;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace MyPodCast
{
    public class SessionCallback : Android.Media.Session.MediaController.Callback
    {
        public Action OnSessionDestroyedImpl { get; set; }
        public Action<PlaybackState> OnPlaybackStateChangedImpl { get; set; }
        public Action<IList<MediaSession.QueueItem>> OnQueueChangedImpl { get; set; }
        public override void OnSessionDestroyed()
        {
            OnSessionDestroyedImpl();
        }

        public override void OnPlaybackStateChanged(PlaybackState state)
        {
            OnPlaybackStateChangedImpl(state);
        }

        public override void OnQueueChanged(IList<MediaSession.QueueItem> queue)
        {
            OnQueueChangedImpl(queue);
        }
    }
}