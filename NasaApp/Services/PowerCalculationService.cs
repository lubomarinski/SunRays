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
using System.Threading;

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

        public double CalculateAizmuth(GeoCoords geoCoords, DateTime dateTime, double timeZone)
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

        public double CalculateZenith(GeoCoords geoCoords, DateTime dateTime, double timeZone)
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
                return spa.Zenith;
            }
            else
            {
                throw new ArithmeticException();
            }
        }

        public double CalculateElevation(GeoCoords geoCoords, DateTime dateTime, double timeZone)
        {
            double azimuth = CalculateAizmuth(geoCoords, dateTime, timeZone);
            double zenithCosine = Math.Cos(CalculateZenith(geoCoords, dateTime, timeZone) * Math.PI / 180.0);
            double elevation = Math.Asin(zenithCosine) / Math.PI * 180.0;
            return azimuth > 180.0 ? 180.0f - elevation : elevation;
        }
    }
}