using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NasaApp.Services
{
    class OpenWeatherProvider : IWeatherProvider
    {
        private static readonly string API_KEY = "4db6a05dfa01c9e584eb59fec257e137";

        public async Task<WeatherInfo> GetWeatherInfoAsync(GeoCoords coords)
        {
            string url = CreateWeatherQuery(coords);
            JObject data = null;

            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();
                data = JObject.Parse(content);
            }

            JObject main = (JObject)data.GetValue("main");

            double? clouds = ((JObject)data.GetValue("clouds")).GetValue("all").Value<double?>();
            double? humidity = main.GetValue("humidity").Value<double?>();
            double? pressure = main.GetValue("pressure").Value<double?>();
            double? temperature = main.GetValue("temp").Value<double?>();

            return new WeatherInfo
            {
                Coordinates = coords,
                Clouds = clouds,
                Humidity = humidity,
                Perssure = pressure,
                Temperature = temperature,
            };
        }

        private string CreateAuthenticatedQuery(string query)
        {
            return $"{query}&APPID={API_KEY}";
        }

        private string CreateWeatherQuery(GeoCoords coords)
        {
            return CreateAuthenticatedQuery($"http://api.openweathermap.org/data/2.5/weather?lat={coords.Latitude}&lon={coords.Longitude}");
        }
    }
}