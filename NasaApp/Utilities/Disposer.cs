using System;
using System.Threading;

namespace NasaApp.Utilities
{
    class DisposableAction : IDisposable
    {
        private Action action;

        public DisposableAction(Action action)
        {
            this.action = action;
        }

        public void Dispose()
        {
            Interlocked.Exchange(ref action, null)?.Invoke();
        }
    }
}