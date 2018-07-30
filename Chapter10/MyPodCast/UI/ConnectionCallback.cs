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
    public class ConnectionCallback : MediaBrowser.ConnectionCallback
    {
        public Action OnConnectedImpl { get; set; }
        public Action OnConnectionFailedImpl { get; set; }
        public Action OnConnectionSuspendedImpl { get; set; }

        public override void OnConnected()
        {
            OnConnectedImpl();
        }
        public override void OnConnectionFailed()
        {
            OnConnectionFailedImpl();
        }
        public override void OnConnectionSuspended()
        {
            OnConnectionSuspendedImpl();
        }
    }
}