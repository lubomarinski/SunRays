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
using NasaApp.Models;
using NasaApp.Utilities;

namespace NasaApp.Services
{
    class LocalPowerCalculator : IPowerCalculator
    {
        private double CalculateModulePowerLoss(double temperatureCelsius, double referenceTemperature, double powerLossCoefficient)
        {
            return Math.Max(0.0, temperatureCelsius - referenceTemperature) * powerLossCoefficient;
        }

        public double CalculateModulePowerLoss(PanelModuleType moduleType, double temperatureCelsius)
        {
            switch (moduleType)
            {
                case PanelModuleType.Monocrystalline:
                    {
                        double referenceTemperature = 25;
                        double powerLossCoefficient = -0.47;
                        return CalculateModulePowerLoss(temperatureCelsius, referenceTemperature, powerLossCoefficient);
                    }
                case PanelModuleType.Polycrystalline:
                    {
                        double referenceTemperature = 25;
                        double powerLossCoefficient = -0.35;
                        return CalculateModulePowerLoss(temperatureCelsius, referenceTemperature, powerLossCoefficient);
                    }
                case PanelModuleType.ThinFilm:
                    {
                        double referenceTemperature = 25;
                        double powerLossCoefficient = -0.20;
                        return CalculateModulePowerLoss(temperatureCelsius, referenceTemperature, powerLossCoefficient);
                    }
                default: throw new InvalidProgramException($"Invalid {nameof(PanelModuleType)}");
            }
        }

        public double CalculateAzmuth(GeoCoords geoCoords, DateTime dateTime, double timeZone)
        {
            SPACalculator.SPAData spa = new SPACalculator.SPAData
            {
                Year = dateTime.Year,
                Month = dateTime.Month,
                Day = dateTime.Day,
                Hour = dateTime.Hour,
                Minute = dateTime.Minute,
                Second = dateTime.Second,
                Timezone = timeZone,
                Latitude = geoCoords.Latitude,
                Longitude = geoCoords.Longitude,
                Function = SPACalculator.CalculationMode.SPA_ZA
            };
            
            if (SPACalculator.SPACalculate(ref spa) == 0)
            {
                return spa.Azimuth;
            }
            else
            {
                throw new ArithmeticException();
            }
        }
    }
}