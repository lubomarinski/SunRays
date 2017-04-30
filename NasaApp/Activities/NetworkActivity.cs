
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using NasaApp.Adaptors;
using NasaApp.Database;
using NasaApp.Models;
using NasaApp.Services;
using NasaApp.Utilities;
using NasaApp.ViewModels;
using SystemSummary = NasaApp.ViewModels.PVSystemInfo.SystemSummary;

namespace NasaApp
{
    [Activity(Label = "NetworkActivity", ScreenOrientation = ScreenOrientation.Portrait)]
    public class NetworkActivity : AppCompatActivity
    {
        private enum RequestCode
        {
            Permissions = 333,
        }

        private static readonly string[] permissions =
        {
            Manifest.Permission.AccessCoarseLocation,
            Manifest.Permission.AccessFineLocation,
            Manifest.Permission.Internet,
            Manifest.Permission.ReadExternalStorage,
            Manifest.Permission.WriteExternalStorage
        };

        private static readonly string EXTRA_NETWORK_ID = $"{typeof(NetworkActivity).FullName}.NETWORK_ID";

        public static Intent CreateIntent(Context context, long networkID)
        {
            Intent intent = new Intent(context, typeof(NetworkActivity));
            intent.PutExtra(EXTRA_NETWORK_ID, networkID);
            return intent;
        }

        private SolarNetwork solarNetwork;
        private IWeatherProvider weatherProvider;
        private IPowerCalculator powerCalculator;
        private PVWattsV5ApiClient pvWattsApiClient;
        private DatabaseHelper db;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Request any necessary permissions. 
            ActivityCompat.RequestPermissions(this, permissions, (int)RequestCode.Permissions);
            // Setup the view.
            SetContentView(Resource.Layout.Network);
            FindViewById<Button>(Resource.Id.modules_button).Click += ToPanelList;
            FindViewById<Button>(Resource.Id.consumers_button).Click += ToConsomerList;
        }
        private void ToPanelList(object sender,EventArgs e)
        {
            var nextActivity = new Intent(this, typeof(PanelListActivity));
            nextActivity.PutExtra("netID", Intent.GetLongExtra(EXTRA_NETWORK_ID, -1));
            StartActivity(nextActivity);
        }

        private void ToConsomerList(object sender, EventArgs e)
        {
            var nextActivity = new Intent(this, typeof(ConsumerListActivity));
            nextActivity.PutExtra("netID", Intent.GetLongExtra(EXTRA_NETWORK_ID, -1));
            StartActivity(nextActivity);
        }

        protected override void OnResume()
        {
            base.OnResume();
            // Start the initialization process.
            BeginInitialize();
        }

        protected override void OnRestart()
        {
            base.OnRestart();
            // Start the initialization process.
            BeginInitialize();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        private async void BeginInitialize()
        {
            // Setup the database.
            db = new DatabaseHelper(this);
            // Use the intent values to bind to a specific network ID.
            long networkID = Intent.GetLongExtra(EXTRA_NETWORK_ID, -1);
            if (networkID != -1)
            {
                solarNetwork = await db.SolarNetworks.SelectByIdAsync(networkID);
                SupportActionBar.Title = solarNetwork.Name;
            }
            else
            {
                throw new InvalidProgramException();
            }

            // Setup the drawer.
            InitializeDrawer();
            // Setup the network card
            InitializeCards();
            // Get the weather service.
            weatherProvider = ServiceManager.Resolve<IWeatherProvider>();
            // Get the calculator service.
            powerCalculator = ServiceManager.Resolve<IPowerCalculator>();
            // Initialize the PVWatts Client.
            pvWattsApiClient = new PVWattsV5ApiClient(apiKey: "o1q77053uqJE20N3dhWPdQGqrhFDfZoSWMoArikE");
        }

        private void InitializeDrawer()
        {
            // Enable the hamburger icon.
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            DrawerLayout drawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            // Create the drawer toggle.
            var drawerToggle = new Android.Support.V7.App.ActionBarDrawerToggle(this, drawerLayout, 0, 0);
            drawerToggle.DrawerIndicatorEnabled = true;
            drawerToggle.SyncState();
            // Set the drawer list adapter.
            ListView drawerListView = FindViewById<ListView>(Resource.Id.drawer_list);
            drawerListView.Adapter = new NavigationListAdapter(this, new[] {
                new NavigationItem(title: "PV Panels", onClick: v => { }),
                new NavigationItem(title: "Consumers", onClick: v => { })
            });
        }

        private void InitializeCards()
        {
            UpdateLocationCard(new GeoCoords
            {
                Latitude = solarNetwork.Latitude,
                Longitude = solarNetwork.Longtitude
            });
            UpdateWeatherInfoView(null);
            UpdatePVSystemInfo(null);
            UpdateConsumerInfo(null);
            UpdateArraySummary(null);

            UpdateCardsAsync().ContinueWith(task =>
            {
                if (task.Exception != null)
                {
                    throw task.Exception;
                }
            });
        }

        private async Task UpdateCardsAsync()
        {
            GeoCoords geoCoords = new GeoCoords { Latitude = solarNetwork.Latitude, Longitude = solarNetwork.Longtitude };

            PVSystemInfo systemInfo = await PVSystemInfo.FromDBAsync(db, solarNetwork.ID);
            UpdatePVSystemInfo(systemInfo);

            WeatherInfo weatherInfo = await weatherProvider.GetWeatherInfoAsync(geoCoords);
            UpdateWeatherInfoView(weatherInfo);

            ConsumerInfo consumerInfo = await ConsumerInfo.FromDBAsync(db);
            UpdateConsumerInfo(consumerInfo);

            SystemSummary arraySummary = await GetArraySummaryAsync(weatherInfo);
            UpdateArraySummary(arraySummary, consumerInfo);
        }

        private void UpdateLocationCard(GeoCoords? geoCoords)
        {
            if (!Looper.MainLooper.IsCurrentThread)
            {
                RunOnUiThread(delegate { UpdateLocationCard(geoCoords); });
                return;
            }

            View card = FindViewById<CardView>(Resource.Id.location_card);
            TextView latitudeText = card.FindViewById<TextView>(Resource.Id.latitude_text);
            TextView longtitudeText = card.FindViewById<TextView>(Resource.Id.longtitude_text);

            if (!geoCoords.HasValue)
            {
                latitudeText.Text = "...";
                longtitudeText.Text = "...";
            }
            else
            {
                latitudeText.Text = geoCoords.Value.Latitude.ToString("0.0000");
                longtitudeText.Text = geoCoords.Value.Longitude.ToString("0.0000");
            }
        }

        private void UpdateWeatherInfoView(WeatherInfo weatherInfo = null)
        {
            if (!Looper.MainLooper.IsCurrentThread)
            {
                RunOnUiThread(delegate { UpdateWeatherInfoView(weatherInfo); });
                return;
            }

            TextView cloudsText = FindViewById<TextView>(Resource.Id.clouds_text);
            TextView tempText = FindViewById<TextView>(Resource.Id.temp_text);

            if (weatherInfo == null)
            {
                cloudsText.Text = "...";
                tempText.Text = "...";
            }
            else
            {
                cloudsText.Text = weatherInfo.Clouds.HasValue
                    ? weatherInfo.Clouds.Value.ToString() + "%"
                    : "N/A";
                tempText.Text = weatherInfo.Temperature.HasValue
                    ? (weatherInfo.Temperature.Value - 273.15).ToString() + "°C"
                    : "N/A";
            }
        }

        private void UpdatePVSystemInfo(PVSystemInfo systemInfo)
        {
            if (!Looper.MainLooper.IsCurrentThread)
            {
                RunOnUiThread(delegate { UpdatePVSystemInfo(systemInfo); });
                return;
            }

            TextView systemSizeText = FindViewById<TextView>(Resource.Id.system_size_text);
            TextView moduleCountText = FindViewById<TextView>(Resource.Id.module_count_text);

            if (systemInfo != null)
            {
                systemSizeText.Text = $"{systemInfo.SystemSize} kW";
                moduleCountText.Text = $"{systemInfo.PanelCount}";
            }
            else
            {
                systemSizeText.Text = "...";
                moduleCountText.Text = "...";
            }
        }

        private void UpdateArraySummary(SystemSummary systemInfo, ConsumerInfo consumerInfo = null)
        {
            View summaryCard = FindViewById<View>(Resource.Id.production_card_bg_root);

            TextView productionHourlyText = FindViewById<TextView>(Resource.Id.production_per_hour);
            TextView productionTodayText = FindViewById<TextView>(Resource.Id.production_daily);
            TextView productionMonthlyText = FindViewById<TextView>(Resource.Id.production_per_month);
            TextView productionAnnualText = FindViewById<TextView>(Resource.Id.production_per_year);

            if (systemInfo != null)
            {
                productionHourlyText.Text = systemInfo.HourlyDCArrayOutput.ToString("0.00") + " kW";
                productionTodayText.Text = systemInfo.TodayDCArrayOutput.ToString("0.00") + " kW";
                productionMonthlyText.Text = systemInfo.MonthlyDCArrayOutput.ToString("0.00") + " kW";
                productionAnnualText.Text = systemInfo.AnnualDCArrayOutput.ToString("0.00") + " kW";

                if (consumerInfo != null)
                {
                    if (consumerInfo.TotalUsage >= systemInfo.TodayDCArrayOutput)
                    {
                        summaryCard.SetBackgroundResource(Resource.Color.md_red_100);
                    }
                    else
                    {
                        summaryCard.SetBackgroundResource(Resource.Color.md_green_100);
                    }
                }
            }
            else
            {
                productionHourlyText.Text = "...";
                productionTodayText.Text = "...";
                productionMonthlyText.Text = "...";
                productionAnnualText.Text = "...";
            }
        }

        private void UpdateConsumerInfo(ConsumerInfo consumerInfo)
        {
            if (!Looper.MainLooper.IsCurrentThread)
            {
                RunOnUiThread(delegate { UpdateConsumerInfo(consumerInfo); });
                return;
            }

            TextView totalUsageText = FindViewById<TextView>(Resource.Id.total_energy_usage_text);
            TextView consumerCountText = FindViewById<TextView>(Resource.Id.consumer_count_text);

            if (consumerInfo != null)
            {
                totalUsageText.Text = $"{consumerInfo.TotalUsage.ToString("0.00")} kW/h";
                consumerCountText.Text = $"{consumerInfo.ConsumerCount}";
            }
            else
            {
                totalUsageText.Text = "...";
                consumerCountText.Text = "...";
            }
        }

        private async Task<SystemSummary> GetArraySummaryAsync(WeatherInfo weatherInfo)
        {
            GeoCoords coords = new GeoCoords
            {
                Latitude = solarNetwork.Latitude,
                Longitude = solarNetwork.Longtitude
            };

            TimeZone timeZone = TimeZone.CurrentTimeZone;
            TimeSpan timeZoneOffset = timeZone.GetUtcOffset(DateTime.Now);
            double timeZoneOffsetHours = timeZoneOffset.Hours + timeZoneOffset.Minutes / 30.0;

            PVSystemInfo systemInfo = await PVSystemInfo.FromDBAsync(db, solarNetwork.ID);
            return await systemInfo.GetSystemSummaryAsync(
                powerCalculator,
                pvWattsApiClient,
                coords,
                DateTime.Now,
                timeZoneOffsetHours,
                tempCelsius: weatherInfo.Temperature.Value - 273.15
            );
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            if (requestCode == (int)RequestCode.Permissions)
            {
                foreach (Permission permission in grantResults)
                {
                    if (permission != Permission.Granted)
                    {
                        Finish();
                    }
                }
            }
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            switch (requestCode)
            {
                default:
                    break;
            }
        }
    }
}