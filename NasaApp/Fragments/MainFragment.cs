using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Gms.Location.Places;
using Android.Gms.Location.Places.UI;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using NasaApp.Adapters;
using NasaApp.Services;

namespace NasaApp.Fragments
{
    public class MainFragment : Android.Support.V4.App.Fragment
    {
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            View view = inflater.Inflate(Resource.Layout.MainFragment, container, false);
            
            adapter = new MainAdapter(Activity);
            adapter.UpdateAdapter(view);
            var button = view.FindViewById<ImageButton>(Resource.Id.button1);
            button.Click += AddNetwork;
            return view;
        }


        private MainAdapter adapter;
        private GeoCoords coords = new GeoCoords(0, 0);
        private View view;

        private void OnPickAPlaceButtonTapped(object sender, EventArgs eventArgs)
        {
            var builder = new PlacePicker.IntentBuilder();
            StartActivityForResult(builder.Build(Activity), 418);
        }

        public override void OnActivityResult(int requestCode, int resultCode, Intent data)
        {
            if ((requestCode == 418) && (resultCode == (int)Result.Ok))
            {
                GetPlaceFromPicker(data);
            }

            base.OnActivityResult(requestCode, resultCode, data);
        }

        private void GetPlaceFromPicker(Intent data)
        {
            IPlace placePicked = PlacePicker.GetPlace(Activity, data);
            var locationText = view.FindViewById<TextView>(Resource.Id.locationText);
            locationText.Text = String.Format("Location:({0},{1})", placePicked.LatLng.Latitude, placePicked.LatLng.Longitude);
            coords = new GeoCoords(placePicked.LatLng.Latitude, placePicked.LatLng.Longitude);
        }

        private void AddNetwork(object sender, EventArgs e)
        {
            Android.Support.V7.App.AlertDialog.Builder alertDialog = new Android.Support.V7.App.AlertDialog.Builder(Activity);
            var localView = Activity.LayoutInflater.Inflate(Resource.Layout.MainDialog, null);
            alertDialog.SetView(localView);
            alertDialog.SetTitle("Add new network");
            var name = localView.FindViewById<TextInputEditText>(Resource.Id.textInputEditText1);
            var button = localView.FindViewById<Button>(Resource.Id.changeLocationButton);
            coords = new GeoCoords(45, 45);
            view = localView;
            button.Click += OnPickAPlaceButtonTapped;
            alertDialog.SetNeutralButton("OK", async delegate
            {
                if (name.Text != String.Empty) await adapter.AddNetwork(name.Text, coords);
                alertDialog.Dispose();
            });
            alertDialog.SetNegativeButton("Cancel", delegate
            {
                alertDialog.Dispose();
            });
            alertDialog.Show();
        }
    }
}