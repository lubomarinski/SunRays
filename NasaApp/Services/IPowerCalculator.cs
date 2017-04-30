using System;
using NasaApp.Models;

namespace NasaApp.Services
{
    public interface IPowerCalculator
    {
        double CalculateModulePowerLoss(PanelModuleType moduleType, double temperatureCelsius);
        double CalculateAzmuth(GeoCoords geoCoords, DateTime dateTime, double timeZone);
    }
}