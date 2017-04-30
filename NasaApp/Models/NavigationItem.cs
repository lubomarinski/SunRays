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

namespace NasaApp.Models
{
    class NavigationItem
    {
        public string Title { get; private set; }
        public Action<View> OnClick { get; private set; }

        public NavigationItem(string title, Action<View> onClick)
        {
            Title = title;
            OnClick = onClick;
        }
    }
}