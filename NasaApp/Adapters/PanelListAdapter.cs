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
using NasaApp.Services;
using NasaApp.Models;
using Android.Support.V7.Widget;
using System.Threading.Tasks;

namespace NasaApp.Adapters
{
    class PanelListAdapter : BaseAdapter
    {

        private List<PanelListItem> panelList = new List<PanelListItem>();
        private Activity activity;
        private DatabaseHelper db;
        private int netID;
        public PanelListAdapter(Activity _activity, int ID)
        {
            netID = ID;
            activity = _activity;
            db = new DatabaseHelper(activity);
            LoadPanels();
        }

        private async Task DeletePanel(PanelListItem panel)
        {
            panelList.Remove(panel);
            await db.Panels.DeleteAsync(await db.Panels.SelectByIdAsync(panel.ID));
            activity.FindViewById<ListView>(Resource.Id.listView1).Adapter = this;
        }

        public async Task AddPanel(Panel panel)
        {
            PanelListItem pli = new PanelListItem(panel);
            pli.ID = (int)await db.Panels.InsertAsync(panel);
            panelList.Add(pli);
            activity.FindViewById<ListView>(Resource.Id.listView1).Adapter = this;
        }

        public override int Count
        {
            get { return panelList.Count; }
        }

        private async void LoadPanels()
        {
            var pl = (await db.Panels.SelectAllAsync()).ToList();
            foreach (var p in pl)
            {
                if (p.NetworkID == netID)
                {
                    PanelListItem pli = new PanelListItem(p);
                    panelList.Add(pli);
                }
            }
            activity.FindViewById<ListView>(Resource.Id.listView1).Adapter = this;
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
            var view = activity.LayoutInflater.Inflate(Resource.Layout.PanelListItem, parent, false);
            var PanelCount = view.FindViewById<TextView>(Resource.Id.panelCount);
            PanelCount.Text = panelList[index].Count.ToString();
            var ppr = view.FindViewById<TextView>(Resource.Id.ppr);
            ppr.Text = panelList[index].PowerRating.ToString() + " kW/h";
            var tpr = view.FindViewById<TextView>(Resource.Id.tpr);
            tpr.Text = (panelList[index].PowerRating * panelList[index].Count).ToString() + " kW/h";
            var pmt = view.FindViewById<TextView>(Resource.Id.pmt);
            pmt.Text = activity.Resources.GetStringArray(Resource.Array.module_type_array)[(int)panelList[index].ModuleType];
            var cardview = view.FindViewById<CardView>(Resource.Id.itemCardView);
            var lineView = view.FindViewById<View>(Resource.Id.lineView);
            lineView.Rotation = (float)panelList[index].Rotation.Value;
            var deg = view.FindViewById<TextView>(Resource.Id.deg);
            deg.Text = ((float)panelList[index].Rotation.Value).ToString() + "°";
            cardview.Click += delegate
            {
                var nextActivity = new Intent(activity, typeof(PanelInfoActivity));
                nextActivity.PutExtra("netID", (long)netID);
                nextActivity.PutExtra("panelID", (long)panelList[index].ID);
                activity.StartActivity(nextActivity);
            };
            var del = view.FindViewById<Button>(Resource.Id.delete_button);
            del.Click += delegate
            {
                Android.Support.V7.App.AlertDialog.Builder alertDialog = new Android.Support.V7.App.AlertDialog.Builder(activity);
                string msg;
                if (panelList[index].Count == 1) msg = "panel";
                else msg = "panels";
                alertDialog.SetTitle("Delete " + msg);
                alertDialog.SetMessage(String.Format("Are you sure you want to delete this {0}?", msg));
                alertDialog.SetNeutralButton("Yes", async delegate
                {
                    await DeletePanel(panelList[index]);
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

        private class PanelListItem
        {
            public int ID { get; set; }
            public int Count { get; set; }
            public PanelModuleType ModuleType { get; set; }
            public double PowerRating { get; set; }
            public double? Rotation { get; set; }
            public PanelListItem(Panel panel)
            {
                ID = panel.ID;
                Count = panel.Count;
                ModuleType = panel.ModuleType;
                PowerRating = panel.PowerRating;
                Rotation = panel.TiltAngle;
            }
        }
    }
}