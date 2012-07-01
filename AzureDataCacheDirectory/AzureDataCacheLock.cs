using System;
using Microsoft.ApplicationServer.Caching;

namespace Lucene.Net.Store.Azure
{
    public class AzureDataCacheLock : Lock
    {
        private readonly string _lockName;
        private readonly string _cacheRegion;
        private readonly DataCache _persistantCache;

        private DataCacheLockHandle _lockHandle;

        public AzureDataCacheLock(string lockName, string cacheRegion, DataCache persistantCache)
        {
            _lockName = lockName;
            _persistantCache = persistantCache;
            _cacheRegion = cacheRegion;
        }

        public override bool IsLocked()
        {
            bool isLocked = _lockHandle != null;

            if (!isLocked)
            {
                // Ignore the real locks here, just do a quick get
                var locked = _persistantCache.Get(_lockName, _cacheRegion);
                isLocked = locked != null && (bool)locked;
            }

            return isLocked;
        }

        public override bool Obtain()
        {
            bool hasLock = _lockHandle != null;

            if (!hasLock)
            {
                try
                {
                    _persistantCache.GetAndLock(_lockName, TimeSpan.FromSeconds(30), out _lockHandle, _cacheRegion, true);
                    hasLock = true;
                }
                catch (DataCacheException ex)
                {
                    if (ex.ErrorCode == (int)DataCacheErrorCode.KeyDoesNotExist)
                    {
                        // Put is not effected by locks
                        _persistantCache.Put(_lockName, true, _cacheRegion);
                        hasLock = true;
                    }
                    else if (ex.ErrorCode == (int)DataCacheErrorCode.ObjectLocked)
                    {
                        // make sure out lock handle isn't set
                        _lockHandle = null;
                    }
                }
            }

            return hasLock;
        }

        public override void Release()
        {
            if (_lockHandle != null)
            {
                _persistantCache.PutAndUnlock(_lockName, false, _lockHandle, _cacheRegion);
                _lockHandle = null;

                // Try to get rid of the lock entirely
                try
                {
                    _persistantCache.Remove(_lockName, _cacheRegion);
                }
                catch { /* Eat the exception here */ }
            }
        }
    }
}