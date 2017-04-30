
using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Content.PM;
using Android.Widget;
using NasaApp.Database;
using NasaApp.Models;
using Android.Support.V7.Widget;
using Android.Views;

namespace NasaApp
{
    [Activity(Label = "PanelInfoActivity", ScreenOrientation = ScreenOrientation.Portrait)]
    public class PanelInfoActivity : AppCompatActivity
    {
        private DatabaseHelper db;
        private Panel panel;
        private SolarNetwork sn;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.PanelInfo);
            SetObjects((int)Intent.GetLongExtra("netID", -1), (int)Intent.GetLongExtra("panelID", -1));

            // Create your application here
        }

        private async void SetObjects(int ID1, int ID2)
        {
            db = new DatabaseHelper(this);
            sn = await db.SolarNetworks.SelectByIdAsync(ID1);
            panel = await db.Panels.SelectByIdAsync(ID2);
            SupportActionBar.Title = sn.Name + " - Panels - Info";
            var PanelCount = FindViewById<TextView>(Resource.Id.panelCount);
            PanelCount.Text = panel.Count.ToString();
            var ppr = FindViewById<TextView>(Resource.Id.ppr);
            ppr.Text = panel.PowerRating.ToString() + " kW/h";
            var tpr = FindViewById<TextView>(Resource.Id.tpr);
            tpr.Text = (panel.PowerRating * panel.Count).ToString() + " kW/h";
            var pmt = FindViewById<TextView>(Resource.Id.pmt);
            pmt.Text = Resources.GetStringArray(Resource.Array.module_type_array)[(int)panel.ModuleType];
            var cardview = FindViewById<CardView>(Resource.Id.itemCardView);
            var lineView = FindViewById<View>(Resource.Id.lineView);
            lineView.Rotation = (float)panel.TiltAngle.Value;
            var deg = FindViewById<TextView>(Resource.Id.deg);
            deg.Text = ((float)panel.TiltAngle.Value).ToString() + "°";
            var pat = FindViewById<TextView>(Resource.Id.pat);
            pat.Text = Resources.GetStringArray(Resource.Array.array_type_array)[(int)panel.ArrayType];
            var dpr = FindViewById<TextView>(Resource.Id.dpr);
            dpr.Text = (panel.PowerRating * panel.Count * 11.8).ToString() + " kW";
            var mpr = FindViewById<TextView>(Resource.Id.mpr);
            mpr.Text = (panel.PowerRating * panel.Count * 11.8 * 30.5).ToString() + " kW";
            var ypr = FindViewById<TextView>(Resource.Id.ypr);
            ypr.Text = (panel.PowerRating * panel.Count * 11.8 * 30.5 * 12).ToString() + " kW";
            var systemLosses = FindViewById<TextView>(Resource.Id.systemLosses);
            systemLosses.Text = panel.SystemLosses.ToString() + "%";
        }
    }
}