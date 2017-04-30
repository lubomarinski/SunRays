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
using Microsoft.Practices.Unity;

namespace NasaApp.Services
{
    public static class ServiceManager
    {
        private static UnityContainer unityContainer = new UnityContainer();

        public static void Register<TFrom, TTo>()
             where TTo : TFrom
        {
            unityContainer.RegisterType<TFrom, TTo>();
        }

        public static T Resolve<T>()
        {
            return unityContainer.Resolve<T>();
        }

    }
}