using System;
using System.Threading;

namespace Velo.Utils
{
    public readonly struct Lock : IDisposable
    {
        private readonly object _lockObject;

        private Lock(object lockObject)
        {
            _lockObject = lockObject;
        }

        public static Lock Enter(object lockObject)
        {
            Monitor.Enter(lockObject);
            return new Lock(lockObject);
        }
        
        public void Dispose()
        {
            Monitor.Exit(_lockObject);
        }
    }
}