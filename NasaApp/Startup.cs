using System;
using NasaApp.Services;

namespace NasaApp
{
    static class Startup
    {
        public static void Initialize()
        {
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            ServiceManager.Register<IWeatherProvider, OpenWeatherProvider>();
            ServiceManager.Register<ILocationProvider, CrossLocationProvider>();
            ServiceManager.Register<IPowerCalculator, LocalPowerCalculator>();
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception)
            {
                throw (Exception)e.ExceptionObject;
            }
            else
            {
                throw new AggregateException(e.ExceptionObject.ToString());
            }
        }
    }
}