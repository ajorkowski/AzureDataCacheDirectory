using Microsoft.ApplicationServer.Caching;

namespace Lucene.Net.Store.Azure
{
    public class AzureDataCacheLockFactory : LockFactory
    {
        private readonly DataCache _persistantCache;
        private readonly string _lockNamespace;
        private readonly string _cacheRegion;

        public AzureDataCacheLockFactory(string lockNamespace, string cacheRegion, DataCache persistantCache)
        {
            _persistantCache = persistantCache;
            _lockNamespace = lockNamespace;
            _cacheRegion = cacheRegion;
        }

        public override void ClearLock(string lockName)
        {
            _persistantCache.Remove(_lockNamespace + lockName, _cacheRegion);
        }

        public override Lock MakeLock(string lockName)
        {
            return new AzureDataCacheLock(_lockNamespace + lockName, _cacheRegion, _persistantCache);
        }
    }
}