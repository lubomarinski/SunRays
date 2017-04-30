using System.Threading.Tasks;

namespace NasaApp.Services
{
    interface IWeatherProvider
    {
        Task<WeatherInfo> GetWeatherInfoAsync(GeoCoords coords);
    }

    public struct GeoCoords
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public GeoCoords(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        public override string ToString()
        {
            return $"(Latitude: {Latitude}, Longitude: {Longitude})";
        }
    }

    class WeatherInfo
    {
        public GeoCoords? Coordinates { get; set; }
        public double? Humidity { get; set; }
        public double? Perssure { get; set; }
        public double? Clouds { get; set; }
        public double? Temperature { get; set; }

        public override string ToString()
        {
            string coords = Coordinates.HasValue ? Coordinates.Value.ToString() : string.Empty;
            string humidity = Humidity.HasValue ? Humidity.Value.ToString() : string.Empty;
            string pressure = Perssure.HasValue ? Perssure.Value.ToString() : string.Empty;
            string clouds = Clouds.HasValue ? Clouds.Value.ToString() : string.Empty;
            string temperature = Temperature.HasValue ? Temperature.Value.ToString() : string.Empty;

            return $"WeatherInfo: {{{coords}, {humidity}, {pressure}, {clouds}, {temperature}}}";
        }
    }
}