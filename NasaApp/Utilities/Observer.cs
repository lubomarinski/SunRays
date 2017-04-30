using System;

namespace NasaApp.Utilities
{
    class Observer<T> : IObserver<T>
    {
        Action<T> onNext;
        Action<Exception> onError;
        Action onCompleted;

        public Observer(Action<T> onNext)
        {
            this.onNext = onNext;
            this.onError = null;
            this.onCompleted = null;

            if (onNext == null)
            {
                throw new ArgumentNullException(nameof(onNext));
            }
        }

        public Observer(Action<T> onNext, Action<Exception> onError, Action onCompleted)
        {
            this.onNext = onNext;
            this.onError = onError;
            this.onCompleted = onCompleted;

            if (onNext == null)
            {
                throw new ArgumentNullException(nameof(onNext));
            }
        }

        public void OnNext(T value)
        {
            onNext?.Invoke(value);
        }

        public void OnError(Exception error)
        {
            if (onError != null)
            {
                onError(error);
            }
            else
            {
                throw error;
            }
        }

        public void OnCompleted()
        {
            onCompleted?.Invoke();
        }
    }
}