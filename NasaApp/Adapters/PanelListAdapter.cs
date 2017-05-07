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
        private SolarNetwork solarNetwork;
        private int netID;

        private IPowerCalculator powerCalc = ServiceManager.Resolve<IPowerCalculator>();

        public PanelListAdapter(Activity _activity, int ID)
        {
            netID = ID;
            activity = _activity;
            db = new DatabaseHelper(activity);
            LoadPanels();
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

        private async Task DeletePanel(PanelListItem panel)
        {
            panelList.Remove(panel);
            await db.Panels.DeleteAsync(await db.Panels.SelectByIdAsync(panel.ID));
            UpdateAdapter();
        }

        public async Task AddPanel(Panel panel)
        {
            PanelListItem pli = new PanelListItem(panel);
            pli.ID = (int)await db.Panels.InsertAsync(panel);
            panelList.Add(pli);
            UpdateAdapter();
        }

        public override int Count
        {
            get { return panelList.Count; }
        }

        private async void LoadPanels()
        {
            solarNetwork = await db.SolarNetworks.SelectByIdAsync(netID);
            var pl = (await db.Panels.SelectAllAsync()).ToList();
            foreach (var p in pl)
            {
                if (p.NetworkID == netID)
                {
                    PanelListItem pli = new PanelListItem(p);
                    panelList.Add(pli);
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
            var deg = view.FindViewById<TextView>(Resource.Id.deg);
            PanelRotation pr = GetRotation(panelList[index]);
            if (pr.IsDay)
            {
                lineView.Rotation = (float)Math.Round(pr.Rotation);
                deg.Text = Math.Round(pr.Rotation).ToString() + "°";
            }
            else
            {
                lineView.Rotation = 90f;
                deg.Text = "(No Sun)";
            }
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
            public PanelArrayType ArrayType { get; set; }
            public PanelModuleType ModuleType { get; set; }
            public double PowerRating { get; set; }
            public double? Rotation { get; set; }
            public PanelListItem(Panel panel)
            {
                ID = panel.ID;
                Count = panel.Count;
                ArrayType = panel.ArrayType;
                ModuleType = panel.ModuleType;
                PowerRating = panel.PowerRating;
                Rotation = panel.TiltAngle;
            }
        }

        private PanelRotation GetRotation(PanelListItem panel)
        {
            double elevation = GetElevation();

            switch (panel.ArrayType)
            {
                case PanelArrayType.FixedOpenRack:
                case PanelArrayType.FixedRoofMounted:
                    return new PanelRotation((float)panel.Rotation.Value, false);
                case PanelArrayType.OneAxis:
                case PanelArrayType.OneAxisBacktracking:
                case PanelArrayType.TwoAxis:
                    return new PanelRotation((float)(elevation > 180.0 ? 90.0 : elevation), true);
                default:
                    throw new NotSupportedException();
            }
        }

        private class PanelRotation
        {
            public float Rotation { get; set; }
            public bool Auto { get; set; }
            public PanelRotation(float rotation, bool auto)
            {
                Rotation = rotation;
                Auto = auto;
            }
            public bool IsDay
            {
                get
                {
                    if ((Rotation <= 180) && (Rotation >= 0)) return true;
                    else return false;
                }
            }
        }

        private double GetTimeZoneOffset()
        {
            TimeZone timeZone = TimeZone.CurrentTimeZone;
            TimeSpan timeZoneOffset = timeZone.GetUtcOffset(DateTime.Now);
            return timeZoneOffset.Hours + timeZoneOffset.Minutes / 30.0;
        }

        private double GetZenith()
        {
            return powerCalc.CalculateZenith(new GeoCoords(solarNetwork.Latitude, solarNetwork.Longtitude), DateTime.Now, GetTimeZoneOffset());
        }
        
        private double GetElevation()
        {
            return powerCalc.CalculateElevation(new GeoCoords(solarNetwork.Latitude, solarNetwork.Longtitude), DateTime.Now, GetTimeZoneOffset());
        }
    }
}