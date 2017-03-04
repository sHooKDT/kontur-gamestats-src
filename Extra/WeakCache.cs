using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Kontur.GameStats.Server
{
    public class WeakCache<TKey, TValue> where TValue : class
    {
        const int CacheCleanInterval = 59;
        private readonly Func<TKey, TValue> getter;
        private readonly Dictionary<TKey, WeakReference> data = new Dictionary<TKey, WeakReference>();
        private readonly ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim();
        private DateTime lastCacheClean = DateTime.MinValue;

        public WeakCache(Func<TKey, TValue> getter)
        {
            this.getter = getter;
        }

        public TValue this[TKey key]
        {
            get
            {
                this.CleanCache();
                try
                {
                    rwLock.EnterUpgradeableReadLock();
                    WeakReference wr;
                    TValue val;
                    if (data.TryGetValue(key, out wr))
                    {
                        val = (TValue)wr.Target;
                        if (val != null)
                            return val;
                    }
                    try
                    {
                        rwLock.EnterWriteLock();
                        if (data.TryGetValue(key, out wr))
                        {
                            val = (TValue)wr.Target;
                            if (val != null)
                                return val;
                        }
                        data[key] = new WeakReference(val = getter(key));
                        return val;
                    }
                    finally
                    {
                        rwLock.ExitWriteLock();
                    }
                }
                finally
                {
                    rwLock.ExitUpgradeableReadLock();
                }
            }
        }

        private void CleanCache()
        {
            if ((DateTime.Now - lastCacheClean).TotalSeconds > CacheCleanInterval)
            {
                try
                {
                    rwLock.EnterWriteLock();
                    if ((DateTime.Now - lastCacheClean).TotalSeconds > CacheCleanInterval)
                    {
                        lastCacheClean = DateTime.Now;
                        var refs = data.ToArray();
                        foreach (var weakReference in refs)
                        {
                            //if (!weakReference.Value.IsAlive)
                                data.Remove(weakReference.Key);
                        }
                    }
                }
                finally
                {
                    rwLock.ExitWriteLock();
                }
            }
        }
    }
}