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

namespace NasaApp.Utilities
{
    public static class Exceptions
    {
        public static void ThrowUnhandled(this Exception e)
        {
            (new Java.Lang.Thread(delegate
            {
                throw e;
            })).Start();
        }
    }
}