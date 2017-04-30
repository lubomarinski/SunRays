
using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Content.PM;
using NasaApp.Models;
using NasaApp.Adapters;
using NasaApp.Database;
using Android.Widget;
using System;

namespace NasaApp
{
    [Activity(Label = "ConsumerListActivity", ScreenOrientation = ScreenOrientation.Portrait)]
    public class ConsumerListActivity : AppCompatActivity
    {
        private ConsumerListAdapter adapter;
        private SolarNetwork sn;
        private DatabaseHelper db;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            db = new DatabaseHelper(this);
            GetObject((int)Intent.GetLongExtra("netID", -1));
            SetContentView(Resource.Layout.ConsumerList);
            adapter = new ConsumerListAdapter(this, (int)Intent.GetLongExtra("netID", -1));
            var listView = FindViewById<ListView>(Resource.Id.listView1);
            listView.Adapter = adapter;
            var button = FindViewById<ImageButton>(Resource.Id.button1);
            button.Click += AddConsumer;
            // Create your application here
        }

        private async void GetObject(int ID)
        {
            sn = await db.SolarNetworks.SelectByIdAsync(ID);
            SupportActionBar.Title = sn.Name + " - Consumers";
        }
        private void AddConsumer(object sender, EventArgs e)
        {
            Consumer consumer = new Consumer();
            consumer.NetworkID = this.sn.ID;
            Android.Support.V7.App.AlertDialog.Builder alertDialog = new Android.Support.V7.App.AlertDialog.Builder(this);
            var localView = this.LayoutInflater.Inflate(Resource.Layout.ConsumerListDialog, null);
            var consumerName = localView.FindViewById<TextView>(Resource.Id.textInputEditText1);
            var energyConsumation = localView.FindViewById<TextView>(Resource.Id.textInputEditText2);
            var seekBar = localView.FindViewById<SeekBar>(Resource.Id.seekBar1);
            var usageText = localView.FindViewById<TextView>(Resource.Id.usageText);
            var countText = localView.FindViewById<TextView>(Resource.Id.numberConsumers);
            seekBar.ProgressChanged += (object s, SeekBar.ProgressChangedEventArgs ea) =>
            {
                if (ea.FromUser)
                {
                    usageText.Text = string.Format("Usage Hours:{0}", ea.Progress / 2.0);
                    consumer.UsageHours = ea.Progress / 2.0;
                }
            };
            alertDialog.SetNeutralButton("OK", async delegate
            {
                bool success = true;
                try { consumer.Name = consumerName.Text; } catch (Exception) { success = false; }
                try { consumer.EnergyConsumation = double.Parse(energyConsumation.Text) / 1000.0; } catch (Exception) { success = false; }
                try { consumer.Count = int.Parse(countText.Text); } catch (Exception) { success = false; }
                if (success) await adapter.AddConsumer(consumer);
                alertDialog.Dispose();

            });
            alertDialog.SetNegativeButton("Cancel", delegate
            {
                alertDialog.Dispose();
            });
            alertDialog.SetView(localView);
            alertDialog.SetTitle("Add Consumer");
            alertDialog.Show();
        }
    }
}