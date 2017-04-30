using System;
using System.Threading.Tasks;

namespace NasaApp.Services
{
    public interface ILocationProvider
    {
        Task<bool> StartListeningAsync();
        IDisposable Subscribe(IObserver<GeoCoords> observer);
    }
}