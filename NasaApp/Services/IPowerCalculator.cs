using System;
using NasaApp.Models;

namespace NasaApp.Services
{
    public interface IPowerCalculator
    {
        double CalculateModulePowerLoss(PanelModuleType moduleType, double temperatureCelsius);
        double CalculateAizmuth(GeoCoords geoCoords, DateTime dateTime, double timeZone);
        double CalculateZenith(GeoCoords geoCoords, DateTime dateTime, double timeZone);
        double CalculateElevation(GeoCoords geoCoords, DateTime dateTime, double timeZone);
    }
}