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
    public class ViewHolder : Java.Lang.Object
    {
        public ImageView ImageView;
        public TextView TitleView;
        public TextView DescriptionView;
    }
}