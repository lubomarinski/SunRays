using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using NasaApp.Fragments;

namespace NasaApp.Adapters
{
    class MainPagerAdapter : FragmentStatePagerAdapter
    {
        private int tabCount;

        public MainPagerAdapter(Android.Support.V4.App.FragmentManager fm, int tabCount) : base(fm)
        {
            this.tabCount = tabCount;
        }

        public override int Count => tabCount;

        public override Android.Support.V4.App.Fragment GetItem(int position)
        {
            switch (position)
            {
                case 0:
                    MainFragment tab1 = new MainFragment();
                    return tab1;
                case 1:
                    InfoFragment tab2 = new InfoFragment();
                    return tab2;
                default:
                    return null;
            }
        }
    }
}