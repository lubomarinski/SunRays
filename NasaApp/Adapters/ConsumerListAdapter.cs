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
using NasaApp.Database;
using NasaApp.Models;
using System.Threading.Tasks;
using Android.Support.V7.Widget;

namespace NasaApp.Adapters
{
    class ConsumerListAdapter : BaseAdapter
    {
        private List<ConsumerListItem> consumerList = new List<ConsumerListItem>();
        private Activity activity;
        private DatabaseHelper db;
        private int netID;
        public ConsumerListAdapter(Activity _activity, int ID)
        {
            netID = ID;
            activity = _activity;
            db = new DatabaseHelper(activity);
            LoadConsumers();
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
        private async Task DeleteConsumer(ConsumerListItem consumer)
        {
            consumerList.Remove(consumer);
            await db.Consumers.DeleteAsync(await db.Consumers.SelectByIdAsync(consumer.ID));
            UpdateAdapter();
        }

        public async Task AddConsumer(Consumer consumer)
        {
            ConsumerListItem cli = new ConsumerListItem(consumer);
            cli.ID = (int)await db.Consumers.InsertAsync(consumer);
            consumerList.Add(cli);
            UpdateAdapter();
        }

        public override int Count
        {
            get { return consumerList.Count; }
        }

        private async void LoadConsumers()
        {
            var cl = (await db.Consumers.SelectAllAsync()).ToList();
            foreach (var c in cl)
            {
                if (c.NetworkID == netID)
                {
                    ConsumerListItem cli = new ConsumerListItem(c);
                    consumerList.Add(cli);
                }
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
            var view = activity.LayoutInflater.Inflate(Resource.Layout.CustomerListItem, parent, false);
            var ConsumerName = view.FindViewById<TextView>(Resource.Id.textView1);
            ConsumerName.Text = consumerList[index].Name.ToString();
            var EnergyConsumation = view.FindViewById<TextView>(Resource.Id.energyConsumation);
            EnergyConsumation.Text = consumerList[index].EnergyConsumation.ToString() + " kW/h";
            var DailyUsage = view.FindViewById<TextView>(Resource.Id.dailyUsage);
            DailyUsage.Text = consumerList[index].UsageHours.ToString() + " h";
            var Count = view.FindViewById<TextView>(Resource.Id.numberConsumers);
            Count.Text = consumerList[index].Count.ToString();
            var Consumation = view.FindViewById<TextView>(Resource.Id.dec);
            Consumation.Text = (consumerList[index].UsageHours * consumerList[index].EnergyConsumation * consumerList[index].Count).ToString() + " kW";
            var del = view.FindViewById<Button>(Resource.Id.delete_button);
            del.Click += delegate
            {
                Android.Support.V7.App.AlertDialog.Builder alertDialog = new Android.Support.V7.App.AlertDialog.Builder(activity);
                alertDialog.SetTitle("Delete consumer");
                alertDialog.SetMessage(String.Format("Are you sure you want to delete this consumer?"));
                alertDialog.SetNeutralButton("Yes", async delegate
                {
                    await DeleteConsumer(consumerList[index]);
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

        private class ConsumerListItem
        {
            public int ID { get; set; }
            public string Name { get; set; }
            public double EnergyConsumation { get; set; }
            public double UsageHours { get; set; }
            public int Count { get; set; }
            public ConsumerListItem(Consumer consumer)
            {
                ID = consumer.ID;
                Name = consumer.Name;
                EnergyConsumation = consumer.EnergyConsumation;
                UsageHours = consumer.UsageHours;
                Count = (int)consumer.Count;
            }
        }
    }
}