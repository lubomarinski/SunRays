using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using NasaApp.Models;
using Newtonsoft.Json.Linq;
using Params = System.Collections.Generic.Dictionary<string, string>;

namespace NasaApp.Services
{
    public class PVWattsV5ApiClient
    {
        public class SystemInfo
        {
            public IList<double> HourlyDCArrayOutput { get; set; }
            public IList<double> MonthlyDCArrayOutput { get; set; }
            public double AnnualDCArrayOutput { get; set; }

            public double GetHourlyDCArrayOutput(DateTime time)
            {
                return HourlyDCArrayOutput[GetHourOfYear(time) - 1];
            }

            public double GetDailyDCArrayOutput(DateTime time)
            {
                DateTime date = time.Date;
                double dailyOutput = 0.0;
                for (int i = 0; i < 24; i++)
                {
                    dailyOutput += GetHourlyDCArrayOutput(date.AddHours(i));
                }
                return dailyOutput;
            }

            public double GeMonthlyDCArrayOutput(DateTime time)
            {
                return MonthlyDCArrayOutput[time.Month - 1];
            }

            private int GetHourOfYear(DateTime dateTime)
            {
                return (dateTime.DayOfYear - 1) * 24 + dateTime.Hour;
            }
        }

        private const string BaseUrl = "https://developer.nrel.gov/api/pvwatts/v5.json";

        private readonly string apiKey = "";
        private readonly ConcurrentDictionary<long, SystemInfo> sysInfoCache;

        public PVWattsV5ApiClient(string apiKey)
        {
            this.apiKey = apiKey;
            this.sysInfoCache = new ConcurrentDictionary<long, SystemInfo>();
        }

        public async Task<SystemInfo> GetSystemInfoCachedAsync(
            long cacheID,
            double systemCapacity,
            PanelModuleType moduleType,
            PanelArrayType arrayType,
            double systemLosses,
            double? tilt,
            double azmuth,
            GeoCoords coords)
        {
            if (sysInfoCache.TryGetValue(cacheID, out SystemInfo cached))
            {
                return cached;
            }
            else
            {
                SystemInfo systemInfo = await GetSystemInfoAsync(systemCapacity, moduleType, arrayType, systemLosses, tilt, azmuth, coords);
                sysInfoCache.TryAdd(cacheID, systemInfo);
                return systemInfo;
            }
        }

        public async Task<SystemInfo> GetSystemInfoAsync(
            double systemCapacity,
            PanelModuleType moduleType,
            PanelArrayType arrayType,
            double systemLosses,
            double? tilt,
            double azmuth,
            GeoCoords coords)
        {
            using (var client = new HttpClient())
            {
                string query = CreateQueryParams(new Params
                {
                    { "api_key", apiKey },
                    { "timeframe", "hourly" },
                    { "dataset", "intl" },
                    { "system_capacity", systemCapacity.ToString() },
                    { "module_type", ((int)moduleType).ToString() },
                    { "losses", systemLosses.ToString() },
                    { "array_type", ((int)arrayType).ToString() },
                    { "tilt", tilt.ToString() },
                    { "azimuth", azmuth.ToString() },
                    { "lat", coords.Latitude.ToString() },
                    { "lon", coords.Longitude.ToString() }
                });

                string requestUrl = $"{BaseUrl}?{query}";
                string json = null;

                try
                {
                    json = await client.GetStringAsync(requestUrl);
                }
                catch (Exception e)
                {
                    throw e;
                }

                JObject data = JObject.Parse(json);
                JArray errors = data["errors"] as JArray;

                if (errors != null && errors.Count != 0)
                {
                    throw new AggregateException(errors.Select(error => new Exception(error.ToString())));
                }
                else
                {
                    JObject output = (JObject)data["outputs"];

                    List<double> dcArrayOutputMonthly = ((JArray)output["dc_monthly"]).Values<double>().ToList();
                    double dcArrayOutputAnnually = dcArrayOutputMonthly.Sum();

                    return new SystemInfo
                    {
                        HourlyDCArrayOutput = ((JArray)output["dc"]).Values<double>().Select(x => x / 1000).ToList(),
                        MonthlyDCArrayOutput = dcArrayOutputMonthly.ToList(),
                        AnnualDCArrayOutput = dcArrayOutputAnnually
                    };
                }
            }
        }

        private string CreateQueryParams(Params parameters)
        {
            return string.Join("&", parameters.Select(kvp =>
            {
                return $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}";
            }));
        }
    }
}