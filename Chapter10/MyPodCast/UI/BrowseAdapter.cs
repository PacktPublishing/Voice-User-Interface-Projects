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
    public class BrowseAdapter : ArrayAdapter<MediaBrowser.MediaItem>
    {
        public BrowseAdapter(Context context) :
            base(context, Resource.Layout.media_list_item, new List<MediaBrowser.MediaItem>())
        {
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            ViewHolder holder;
            if(convertView == null)
            {
                convertView = LayoutInflater.From(Context)
                    .Inflate(Resource.Layout.media_list_item, parent, false);
                holder = new ViewHolder();
                holder.ImageView = convertView.FindViewById<ImageView>(Resource.Id.play_eq);
                holder.ImageView.Visibility = ViewStates.Gone;
                holder.TitleView = convertView.FindViewById<TextView>(Resource.Id.title);
                holder.DescriptionView = convertView.FindViewById<TextView>(Resource.Id.description);
                convertView.Tag = holder;
            }
            else
            {
                holder = (ViewHolder)convertView.Tag;
            }
            MediaBrowser.MediaItem item = GetItem(position);
            holder.TitleView.Text = item.Description.Title;
            holder.DescriptionView.Text = item.Description.Description;
            if(item.IsPlayable)
            {
                holder.ImageView.SetImageDrawable(
                    Context.GetDrawable(Resource.Drawable.ic_play_arrow_white_24dp));
                holder.ImageView.Visibility = ViewStates.Visible;
            }
            return convertView;
        }
    }
}