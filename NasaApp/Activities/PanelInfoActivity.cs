
using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Content.PM;
using Android.Widget;
using NasaApp.Database;
using NasaApp.Models;
using Android.Support.V7.Widget;
using Android.Views;
using NasaApp.Services;
using System;
using NasaApp.Utilities;
using NasaApp.ViewModels;
using System.Threading.Tasks;

namespace NasaApp
{
    [Activity(Label = "PanelInfoActivity", ScreenOrientation = ScreenOrientation.Portrait)]
    public class PanelInfoActivity : AppCompatActivity
    {
        private DatabaseHelper db;
        private Panel panel;
        private SolarNetwork sn;

        IPowerCalculator powerCalc = ServiceManager.Resolve<IPowerCalculator>();
        PVWattsV5ApiClient pvWattsApiClient = new PVWattsV5ApiClient(apiKey: Secrets.PVWattsApiKey);

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
            UpdateTiltAngle();
            UpdateSummaryAsync();
            var PanelCount = FindViewById<TextView>(Resource.Id.panelCount);
            PanelCount.Text = panel.Count.ToString();
            var ppr = FindViewById<TextView>(Resource.Id.ppr);
            ppr.Text = panel.PowerRating.ToString() + " kW/h";
            var tpr = FindViewById<TextView>(Resource.Id.tpr);
            tpr.Text = (panel.PowerRating * panel.Count).ToString() + " kW/h";
            var pmt = FindViewById<TextView>(Resource.Id.pmt);
            pmt.Text = Resources.GetStringArray(Resource.Array.module_type_array)[(int)panel.ModuleType];
            var cardview = FindViewById<CardView>(Resource.Id.itemCardView);
            var pat = FindViewById<TextView>(Resource.Id.pat);
            pat.Text = Resources.GetStringArray(Resource.Array.array_type_array)[(int)panel.ArrayType];
            var systemLosses = FindViewById<TextView>(Resource.Id.systemLosses);
            systemLosses.Text = panel.SystemLosses.ToString() + "%";
        }

        private void UpdateTiltAngle()
        {
            var lineView = FindViewById<View>(Resource.Id.lineView);
            var deg = FindViewById<TextView>(Resource.Id.deg);
            var zenithText = FindViewById<TextView>(Resource.Id.zenith);

            double zenith = GetZenith();
            double elevation = GetElevation();
            zenithText.Text = (Math.Round(zenith)).ToString() + "°";

            switch (panel.ArrayType)
            {
                case PanelArrayType.FixedOpenRack:
                case PanelArrayType.FixedRoofMounted:
                    lineView.Rotation = (float)panel.TiltAngle.Value;
                    deg.Text = (Math.Round(panel.TiltAngle.Value)).ToString() + "°";
                    break;
                case PanelArrayType.OneAxis:
                case PanelArrayType.OneAxisBacktracking:
                case PanelArrayType.TwoAxis:
                    if (elevation < 180.0 && elevation > 0.0)
                    {
                        lineView.Rotation = (float)elevation;
                        deg.Text = Math.Round(elevation).ToString() + "°";
                    }
                    else
                    {
                        lineView.Rotation = 90f;
                        deg.Text = "(No Sun)";
                    }
                    break;
                default:
                    throw new NotSupportedException();
            }
        }


        private async Task UpdateSummaryAsync()
        {
            var estDaily = FindViewById<TextView>(Resource.Id.estimated_daily);
            var estMonthly = FindViewById<TextView>(Resource.Id.estimated_monthly);
            var estAnnual = FindViewById<TextView>(Resource.Id.estimated_annual);

            PVSystemInfo systemInfo = await PVSystemInfo.FromDBAsync(db, sn.ID);
            var panelSummary = await systemInfo.GetPanelSummaryAsync(
                panel, powerCalc, pvWattsApiClient, 
                new GeoCoords(sn.Latitude, sn.Longtitude), 
                GetZenith(), DateTime.Now, GetTimeZoneOffset(), 
                tempCelsius: 25.0);

            estDaily.Text = (panelSummary.TodayDCArrayOutput).ToString("0.00") + "kW";
            estMonthly.Text = (panelSummary.MonthlyDCArrayOutput).ToString("0.00") + "kW";
            estAnnual.Text = (panelSummary.AnnualDCArrayOutput).ToString("0.00") + "kW";
        }

        private double GetTimeZoneOffset()
        {
            TimeZone timeZone = TimeZone.CurrentTimeZone;
            TimeSpan timeZoneOffset = timeZone.GetUtcOffset(DateTime.Now);
            return timeZoneOffset.Hours + timeZoneOffset.Minutes / 30.0;
        }

        private double GetZenith()
        {
            return powerCalc.CalculateZenith(new GeoCoords(sn.Latitude, sn.Longtitude), DateTime.Now, GetTimeZoneOffset());
        }

        private double GetElevation()
        {
            return powerCalc.CalculateElevation(new GeoCoords(sn.Latitude, sn.Longtitude), DateTime.Now, GetTimeZoneOffset());
        }
    }
}