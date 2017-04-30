using System;
using System.Collections.Concurrent;

namespace NasaApp.Utilities
{
    class Observable<T> : IObservable<T>
    {
        private class Unit { }
        private ConcurrentDictionary<IObserver<T>, Unit> observers;

        protected Observable()
        {
            observers = new ConcurrentDictionary<IObserver<T>, Unit>();
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            observers.TryAdd(observer, null);
            return new DisposableAction(delegate
            {
                observers.TryRemove(observer, out Unit temp);
            });
        }

        protected void OnNext(T value)
        {
            foreach (var observer in observers.Keys)
            {
                observer.OnNext(value);
            }
        }

        protected void OnError(Exception error)
        {
            foreach (var observer in observers.Keys)
            {
                observer.OnError(error);
            }
        }

        protected void OnCompleted()
        {
            foreach (var observer in observers.Keys)
            {
                observer.OnCompleted();
            }
        }
    }
}