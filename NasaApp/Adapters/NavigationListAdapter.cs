using System;
using System.Collections.Generic;
using System.Linq;

using Android.App;
using Android.Views;
using Android.Widget;
using NasaApp.Models;

namespace NasaApp.Adaptors
{
    class NavigationListAdapter : BaseAdapter
    {
        private Activity context;
        private IList<NavigationItem> items;

        public NavigationListAdapter(Activity context, IEnumerable<NavigationItem> items)
        {
            this.context = context;
            this.items = items.ToList();
        }

        public override int Count => items.Count;

        public override Java.Lang.Object GetItem(int position)
        {
            return null;
        }

        public override long GetItemId(int position)
        {
            return items[position].GetHashCode();
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var view = convertView 
                ?? context.LayoutInflater.Inflate(Resource.Layout.NavigationItem, parent, false);

            var item = items[position];

            TextView titleTextView = view.FindViewById<TextView>(Resource.Id.title);
            titleTextView.Text = item.Title;
            titleTextView.SetOnClickListener(new ClickActionListener(item.OnClick));

            return view;
        }

        private class ClickActionListener : Java.Lang.Object, View.IOnClickListener
        {
            private Action<View> onClick;

            public ClickActionListener(Action<View> onClick)
            {
                this.onClick = onClick;
            }

            public void OnClick(View v)
            {
                onClick(v);
            }
        }
    }
}