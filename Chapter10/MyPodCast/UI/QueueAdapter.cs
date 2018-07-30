using System;
using Android.Widget;
using Android.Media.Session;
using Android.App;
using System.Collections.Generic;
using Android.Views;

namespace MyPodCast
{
	public class QueueAdapter : ArrayAdapter<MediaSession.QueueItem>
	{
		public long ActiveQueueItemId { get; set; }

		public QueueAdapter(Activity context)
			: base(context, Resource.Layout.media_list_item, new List<MediaSession.QueueItem>())
		{
			ActiveQueueItemId = MediaSession.QueueItem.UnknownId;
		}

		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			ViewHolder holder;

			if(convertView == null) {
				convertView = LayoutInflater.From(Context)
					.Inflate(Resource.Layout.media_list_item, parent, false);
				holder = new ViewHolder();
				holder.ImageView = (ImageView) convertView.FindViewById(Resource.Id.play_eq);
				holder.TitleView = (TextView) convertView.FindViewById(Resource.Id.title);
				holder.DescriptionView = (TextView) convertView.FindViewById(Resource.Id.description);
				convertView.Tag = holder;
			} else {
				holder = (ViewHolder) convertView.Tag;
			}

			MediaSession.QueueItem item = GetItem(position);
			holder.TitleView.Text = item.Description.Title;
			if(item.Description.Description != null) {
				holder.DescriptionView.Text = item.Description.Description;
			}

			if(ActiveQueueItemId == item.QueueId) {
				holder.ImageView.SetImageDrawable(
					Context.GetDrawable(Resource.Drawable.ic_equalizer_white_24dp));
			} else {
				holder.ImageView.SetImageDrawable(
					Context.GetDrawable(Resource.Drawable.ic_play_arrow_white_24dp));
			}
			return convertView;
		}
	}
}

