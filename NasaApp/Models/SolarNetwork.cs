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
    public class SolarNetwork
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public double Latitude { get; set; }
        public double Longtitude { get; set; }
    }
}