using NasaApp.Utilities;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;
using System.Threading.Tasks;

namespace NasaApp.Services
{
    class CrossLocationProvider : Observable<GeoCoords>, ILocationProvider
    {
        private IGeolocator locator;

        public CrossLocationProvider()
        {
            locator = CrossGeolocator.Current;
            locator.AllowsBackgroundUpdates = true;
            locator.DesiredAccuracy = 100;
            locator.PositionChanged += OnChanged;
        }

        public async Task<bool> StartListeningAsync()
        {
            if (locator.IsListening)
            {
                return true;
            }

            if (await locator.StartListeningAsync(10000, 500))
            {
                ForceUpdateAsync();
                return true;
            }
            return false;
        }

        private async Task ForceUpdateAsync()
        {
            OnNext(ToGeoCoords(await locator.GetPositionAsync()));
        }

        private GeoCoords ToGeoCoords(Position position) => new GeoCoords
        {
            Latitude = position.Latitude,
            Longitude = position.Longitude,
        };

        private void OnChanged(object sender, PositionEventArgs e)
        {
            OnNext(ToGeoCoords(e.Position));
        }
    }
}