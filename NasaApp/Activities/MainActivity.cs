using Android.App;
using Android.OS;
using Android.Support.V7.App;
using NasaApp.Adapters;
using System;
using Android.Support.Design.Widget;
using Android.Content.PM;
using Android.Gms.Location.Places.UI;
using Android.Content;
using Android.Gms.Location.Places;
using NasaApp.Services;
using Android.Views;
using Android.Support.V4.View;
using Android.Support.V7.Widget;

namespace NasaApp
{
    [Activity(Label = "SunRays", MainLauncher = true, Icon = "@drawable/icon", ScreenOrientation = ScreenOrientation.Portrait, Theme = "@style/AppThemeNoActionBar")]
    public class MainActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);

            Toolbar toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            InitializeAdapter();
        }

        private void InitializeAdapter()
        {
            TabLayout tabLayout = FindViewById<TabLayout>(Resource.Id.tab_layout);
            tabLayout.RemoveAllTabs();
            tabLayout.AddTab(tabLayout.NewTab().SetText("My Networks"));
            tabLayout.AddTab(tabLayout.NewTab().SetText("Information"));
            tabLayout.TabGravity = TabLayout.GravityFill;

            ViewPager viewPager = FindViewById<ViewPager>(Resource.Id.pager);
            PagerAdapter adapter = new MainPagerAdapter(SupportFragmentManager, tabLayout.TabCount);

            viewPager.Adapter = adapter;
            viewPager.AddOnPageChangeListener(new TabLayout.TabLayoutOnPageChangeListener(tabLayout));
            tabLayout.TabSelected += delegate (object sender, TabLayout.TabSelectedEventArgs e)
            {
                viewPager.CurrentItem = e.Tab.Position;
            };
        }

        protected override void OnResume()
        {
            base.OnResume();
            InitializeAdapter();
        }

        protected override void OnRestart()
        {
            base.OnRestart();
            InitializeAdapter();
        }
    }
}

