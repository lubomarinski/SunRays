using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NasaApp.Database;
using NasaApp.Models;
using NasaApp.Services;

namespace NasaApp.ViewModels
{
    class PVSystemInfo
    {
        public int ID { get; set; }
        public double SystemSize { get; private set; }
        public IList<Panel> Panels { get; private set; }
        public int PanelCount { get => Panels.Count; }

        public PVSystemInfo(int id, double systemSize, IList<Panel> panels)
        {
            ID = id;
            SystemSize = systemSize;
            Panels = panels;
        }

        public static async Task<PVSystemInfo> FromDBAsync(DatabaseHelper db, long networkID)
        {
            IList<Panel> panels = (await db.Panels
                .SelectAllAsync())
                .Where(x => x.ID == networkID)
                .ToList();
            return new PVSystemInfo(
                (int)networkID,
                systemSize: panels.Sum(x => x.PowerRating * x.Count),
                panels: panels
            );
        }

        public class SystemSummary
        {
            public double HourlyDCArrayOutput { get; set; }
            public double TodayDCArrayOutput { get; set; }
            public double MonthlyDCArrayOutput { get; set; }
            public double AnnualDCArrayOutput { get; set; }
        }

        private async Task<SystemSummary> GetPanelSummaryAsync(Panel panel, IPowerCalculator powerCalculator, PVWattsV5ApiClient pvWattsApiClient, GeoCoords coords, double azmuth, DateTime dateTime, double timeZoneOffsetHours, double tempCelsius)
        {
            int count = panel.Count;
            double powerLoss = powerCalculator.CalculateModulePowerLoss(panel.ModuleType, tempCelsius);
            var pvSystemInfo = await pvWattsApiClient.GetSystemInfoCachedAsync(
                ID,
                panel.PowerRating,
                panel.ModuleType,
                panel.ArrayType,
                systemLosses: powerLoss,
                tilt: panel.TiltAngle,
                azmuth: azmuth,
                coords: coords
            );

            double hourly = pvSystemInfo.GetHourlyDCArrayOutput(dateTime);
            double hourlyWithLosses = hourly - (hourly * powerLoss);
            double totalHourlyWithLosses = hourlyWithLosses * count;

            double today = pvSystemInfo.GetDailyDCArrayOutput(dateTime);
            double todayWithLosses = today - (today * powerLoss);

            double monthly = pvSystemInfo.GeMonthlyDCArrayOutput(dateTime);
            double monthlyWithLosses = monthly - (monthly * powerLoss);
            double totalMonthlyWithLosses = monthlyWithLosses * count;

            double annual = pvSystemInfo.AnnualDCArrayOutput;
            double annualWithLosses = annual - (annual * powerLoss);
            double totalAnnualWithLosses = annualWithLosses * count;

            return new SystemSummary
            {
                HourlyDCArrayOutput = totalHourlyWithLosses,
                TodayDCArrayOutput = todayWithLosses,
                MonthlyDCArrayOutput = totalMonthlyWithLosses,
                AnnualDCArrayOutput = totalAnnualWithLosses
            };
        }

        public Task<SystemSummary> GetSystemSummaryAsync(IPowerCalculator powerCalculator, PVWattsV5ApiClient pvWattsApiClient, GeoCoords coords, DateTime dateTime, double timeZoneOffsetHours, double tempCelsius)
        {
            return Task.Run(async delegate
            {
                double azmuth = powerCalculator.CalculateAzmuth(coords, dateTime, timeZoneOffsetHours);
                int totalPanelCount = Panels.Count;

                IEnumerable<Task<SystemSummary>> summaryPerTypeTask = 
                    Panels.Select(x => GetPanelSummaryAsync(x, powerCalculator, pvWattsApiClient, coords, azmuth, dateTime, timeZoneOffsetHours, tempCelsius));

                return (await Task.WhenAll(summaryPerTypeTask)).Aggregate(new SystemSummary { }, (acc, x) =>
                {
                    return new SystemSummary
                    {
                        HourlyDCArrayOutput = acc.HourlyDCArrayOutput + x.HourlyDCArrayOutput,
                        TodayDCArrayOutput = acc.TodayDCArrayOutput + x.TodayDCArrayOutput,
                        MonthlyDCArrayOutput = acc.MonthlyDCArrayOutput + x.MonthlyDCArrayOutput,
                        AnnualDCArrayOutput = acc.AnnualDCArrayOutput + x.AnnualDCArrayOutput,
                    };
                });
            });
        }
    }
}