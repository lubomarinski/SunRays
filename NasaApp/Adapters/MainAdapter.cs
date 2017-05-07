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
using NasaApp.Models;
using NasaApp.Database;
using Android.Support.V7.Widget;
using System.Threading.Tasks;
using NasaApp.Services;

namespace NasaApp.Adapters
{
    class MainAdapter : BaseAdapter
    {
        private List<Network> netList = new List<Network>();
        private Activity activity;
        private DatabaseHelper db;
        public MainAdapter(Activity _activity)
        {
            activity = _activity;
            db = new DatabaseHelper(activity);
            LoadNetworks();
        }

        public void UpdateAdapter()
        {
            activity.FindViewById<ListView>(Resource.Id.listView1).Adapter = this;
            var emptyText = activity.FindViewById<TextView>(Resource.Id.emptyText);
            if (Count == 0)
            {
                emptyText.Visibility = ViewStates.Visible;
            }
            else
            {
                emptyText.Visibility = ViewStates.Gone;
            }
        }

        public void UpdateAdapter(View view)
        {
            view.FindViewById<ListView>(Resource.Id.listView1).Adapter = this;
            var emptyText = view.FindViewById<TextView>(Resource.Id.emptyText);
            if (Count == 0)
            {
                emptyText.Visibility = ViewStates.Visible;
            }
            else
            {
                emptyText.Visibility = ViewStates.Gone;
            }
        }

        private async Task DeleteNetwork(Network net)
        {
            netList.Remove(net);
            await db.SolarNetworks.DeleteAsync(await db.SolarNetworks.SelectByIdAsync(net.ID));
            foreach (var panel in await db.Panels.SelectAllAsync())
            {
                await db.Panels.DeleteAsync(panel);
            }
            foreach (var consumer in await db.Consumers.SelectAllAsync())
            {
                await db.Consumers.DeleteAsync(consumer);
            }
            UpdateAdapter();
        }

        public async Task AddNetwork(string name, GeoCoords coords)
        {
            Network net = new Network(name);
            SolarNetwork sn = new SolarNetwork();
            sn.Name = net.Name;
            sn.Longtitude = coords.Longitude;
            sn.Latitude = coords.Latitude;
            net.ID = (int)await db.SolarNetworks.InsertAsync(sn);
            netList.Add(net);
            UpdateAdapter();
        }

        public override int Count
        {
            get { return netList.Count; }
        }

        private async void LoadNetworks()
        {
            var snl = (await db.SolarNetworks.SelectAllAsync()).ToList();

            foreach (var sn in snl)
            {
                Network net = new Network(sn);
                List<Panel> panels = (await db.Panels.SelectAllAsync()).Where(x => x.NetworkID == net.ID).ToList();
                net.Panels = panels.Sum(x => x.Count);
                net.PanelsEnergy = panels.Sum(x => 
                {
                    return x.PowerRating * x.Count;
                });

                List<Consumer> consumers = (await db.Consumers.SelectAllAsync()).Where(x => x.NetworkID == net.ID).ToList();
                net.Consumers = consumers.Sum(x => x.Count);
                net.ConsumersEnergy = consumers.Sum(x =>
                {
                    return x.EnergyConsumation * x.Count * x.UsageHours;
                });

                netList.Add(net);
            }
            UpdateAdapter();
        }

        public override Java.Lang.Object GetItem(int index)
        {
            // could wrap a Contact in a Java.Lang.Object
            // to return it here if needed
            return null;
        }

        public override long GetItemId(int index)
        {
            return index;
        }

        public override View GetView(int index, View convertView, ViewGroup parent)
        {
            var view = activity.LayoutInflater.Inflate(Resource.Layout.MainListItem, parent, false);
            var network = netList[index];
            var mainTitle = view.FindViewById<TextView>(Resource.Id.textView1);
            mainTitle.Text = netList[index].Name;
            var cardview = view.FindViewById<CardView>(Resource.Id.itemCardView);
            
            view.FindViewById<TextView>(Resource.Id.panel_count).Text = network.Panels.ToString();
            view.FindViewById<TextView>(Resource.Id.panel_energy).Text = $"{network.PanelsEnergy.ToString("0.00")} kW";

            view.FindViewById<TextView>(Resource.Id.consumer_count).Text = network.Consumers.ToString();
            view.FindViewById<TextView>(Resource.Id.consumer_energy).Text = $"{network.ConsumersEnergy.ToString("0.00")} kW";

            cardview.Click += delegate
            {
                activity.StartActivity(NetworkActivity.CreateIntent(activity, netList[index].ID));
            };
            var del = view.FindViewById<Button>(Resource.Id.delete_button);
            del.Click += delegate
            {
                Android.Support.V7.App.AlertDialog.Builder alertDialog = new Android.Support.V7.App.AlertDialog.Builder(activity);
                alertDialog.SetTitle("Delete network");
                alertDialog.SetMessage("Are you sure you want to delete this network?");
                alertDialog.SetNeutralButton("Yes", async delegate
                {
                    await DeleteNetwork(netList[index]);
                    alertDialog.Dispose();
                });
                alertDialog.SetNegativeButton("No", delegate
                {
                    alertDialog.Dispose();
                });
                alertDialog.Show();
            };
            return view;
        }

        private class Network
        {
            public int ID { get; set; }
            public string Name { get; set; }
            public int Panels { get; set; }
            public double PanelsEnergy { get; set; }
            public int Consumers { get; set; }
            public double ConsumersEnergy { get; set; }
            public Network(string name)
            {
                Name = name;
                Panels = 0;
                PanelsEnergy = 0;
                Consumers = 0;
                ConsumersEnergy = 0;
            }
            public Network(SolarNetwork sn)
            {
                ID = sn.ID;
                Name = sn.Name;
                Panels = 0;
                PanelsEnergy = 0;
                Consumers = 0;
                ConsumersEnergy = 0;
            }
        }
    }
}