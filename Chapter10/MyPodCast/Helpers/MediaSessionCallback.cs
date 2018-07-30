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
    public class MediaSessionCallback : MediaSession.Callback
    {
        public Action OnPlayImpl { get; set; }

        public Action<long> OnSkipToQueueItemImpl { get; set; }

        public Action<long> OnSeekToImpl { get; set; }

        public Action<string, Bundle> OnPlayFromMediaIdImpl { get; set; }

        public Action OnPauseImpl { get; set; }

        public Action OnStopImpl { get; set; }

        public Action OnSkipToNextImpl { get; set; }

        public Action OnSkipToPreviousImpl { get; set; }

        public Action<string, Bundle> OnCustomActionImpl { get; set; }

        public Action<string, Bundle> OnPlayFromSearchImpl { get; set; }

        public override void OnPlay()
        {
            OnPlayImpl();
        }

        public override void OnSkipToQueueItem(long id)
        {
            OnSkipToQueueItemImpl(id);
        }

        public override void OnSeekTo(long pos)
        {
            OnSeekToImpl(pos);
        }

        public override void OnPlayFromMediaId(string mediaId, Bundle extras)
        {
            OnPlayFromMediaIdImpl(mediaId, extras);
        }

        public override void OnPause()
        {
            OnPauseImpl();
        }

        public override void OnStop()
        {
            OnStopImpl();
        }

        public override void OnSkipToNext()
        {
            OnSkipToNextImpl();
        }

        public override void OnSkipToPrevious()
        {
            OnSkipToPreviousImpl();
        }

        public override void OnCustomAction(string action, Bundle extras)
        {
            OnCustomActionImpl(action, extras);
        }

        public override void OnPlayFromSearch(string query, Bundle extras)
        {
            OnPlayFromSearchImpl(query, extras);
        }
    }
}