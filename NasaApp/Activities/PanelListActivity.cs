using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Content.PM;
using NasaApp.Adapters;
using System;
using Android.Widget;
using NasaApp.Models;
using Android.Views;
using Android.Support.Design.Widget;
using System.Threading.Tasks;
using NasaApp.Database;

namespace NasaApp
{
    [Activity(Label = "PanelListActivity", ScreenOrientation = ScreenOrientation.Portrait)]
    public class PanelListActivity : AppCompatActivity
    {
        private PanelListAdapter adapter;
        private SolarNetwork sn;
        private DatabaseHelper db;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            db = new DatabaseHelper(this);
            GetObject((int)Intent.GetLongExtra("netID", -1));
            SetContentView(Resource.Layout.PanelList);
            adapter = new PanelListAdapter(this, (int)Intent.GetLongExtra("netID", -1));
            var listView = FindViewById<ListView>(Resource.Id.listView1);
            listView.Adapter = adapter;
            var button = FindViewById<ImageButton>(Resource.Id.button1);
            button.Click += AddPanel;
            // Create your application here
        }
        private async void GetObject(int ID)
        {
            sn = await db.SolarNetworks.SelectByIdAsync(ID);
            SupportActionBar.Title = sn.Name+" - Panels";
        }
        private void AddPanel(object sender, EventArgs e)
        {
            Panel panel = new Panel();
            panel.NetworkID = sn.ID;
            Android.Support.V7.App.AlertDialog.Builder alertDialog = new Android.Support.V7.App.AlertDialog.Builder(this);
            var localView = this.LayoutInflater.Inflate(Resource.Layout.PanelListDialog, null);
            var textView = localView.FindViewById<TextView>(Resource.Id.textView1);
            //s1
            var s1 = localView.FindViewById<Spinner>(Resource.Id.spinner1);
            s1.ItemSelected += (object s, AdapterView.ItemSelectedEventArgs ea) =>
            {
                Spinner spinner = (Spinner)s;
                panel.ModuleType = (PanelModuleType)ea.Position;
            };
            var localAdapter1 = ArrayAdapter.CreateFromResource(this, Resource.Array.module_type_array, Android.Resource.Layout.SimpleSpinnerItem);
            localAdapter1.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            s1.Adapter = localAdapter1;
            //s2
            var s2 = localView.FindViewById<Spinner>(Resource.Id.spinner2);
            s2.ItemSelected += (object s, AdapterView.ItemSelectedEventArgs ea) =>
            {
                Spinner spinner = (Spinner)s;
                panel.ArrayType = (PanelArrayType)ea.Position;
            };
            var localAdapter2 = ArrayAdapter.CreateFromResource(this, Resource.Array.array_type_array, Android.Resource.Layout.SimpleSpinnerItem);
            localAdapter2.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            s2.Adapter = localAdapter2;
            //seekbar
            var seekBar = localView.FindViewById<SeekBar>(Resource.Id.seekBar1);
            panel.TiltAngle = 90;
            seekBar.ProgressChanged += (object s, SeekBar.ProgressChangedEventArgs ea) =>
            {
                if (ea.FromUser)
                {
                    textView.Text = string.Format("Tilt Angle: {0}°", ea.Progress);
                    panel.TiltAngle = ea.Progress;
                }
            };
            //textview
            var systemLooses = localView.FindViewById<TextInputEditText>(Resource.Id.textInputEditText1);
            var powerRating = localView.FindViewById<TextInputEditText>(Resource.Id.textInputEditText2);
            var count = localView.FindViewById<TextInputEditText>(Resource.Id.textInputEditText5);
            //panel
            alertDialog.SetNeutralButton("OK", async delegate
            {
                bool success = true;
                try { panel.SystemLosses = double.Parse(systemLooses.Text); } catch (Exception) { success = false; }
                try { panel.PowerRating = double.Parse(powerRating.Text); } catch (Exception) { success = false; }
                try { panel.Count = int.Parse(count.Text); } catch (Exception) { success = false; }
                if (success) await adapter.AddPanel(panel);
                alertDialog.Dispose();

            });
            alertDialog.SetNegativeButton("Cancel", delegate
            {
                alertDialog.Dispose();
            });
            alertDialog.SetView(localView);
            alertDialog.SetTitle("Add Panel(s)");
            alertDialog.Show();
        }


    }
}