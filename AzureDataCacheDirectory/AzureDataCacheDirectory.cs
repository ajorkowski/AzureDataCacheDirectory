using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.ApplicationServer.Caching;

namespace Lucene.Net.Store.Azure
{
    public class AzureDataCacheDirectory : Lucene.Net.Store.Directory
    {
        private readonly string _cacheRegion;
        private readonly string _modifiedNamespace;
        private readonly string _fileSystemNamespace;
        private readonly string _fileSizeNamespace;
        private readonly DataCache _persistantCache;
        private readonly DataCacheTag _modifiedTag;

        public AzureDataCacheDirectory(string cacheRegion, DataCache persistantCache)
        {
            _cacheRegion = cacheRegion;
            _persistantCache = persistantCache;
            _fileSystemNamespace = cacheRegion + "::Files::";
            _fileSizeNamespace = cacheRegion + "::FileSize::";
            _modifiedNamespace = cacheRegion + "::Modified::";
            _modifiedTag = new DataCacheTag("Modified");

            // Make sure the region is created
            _persistantCache.CreateRegion(_cacheRegion);

            // Use our lock factory
            var lockNamespace = cacheRegion + "::Locks::";
            SetLockFactory(new AzureDataCacheLockFactory(lockNamespace, _cacheRegion, persistantCache));
        }

        public bool HasIndexes()
        {
            return _persistantCache.GetObjectsByTag(_modifiedTag, _cacheRegion).Any();
        }

        public void ClearAllIndexes()
        {
            _persistantCache.ClearRegion(_cacheRegion);
        }

        public override IndexOutput CreateOutput(string name)
        {
            return new AzureDataCacheIndexOutput(_fileSystemNamespace + name, _fileSizeNamespace + name, _cacheRegion, _modifiedNamespace + name, _modifiedTag, _persistantCache);
        }

        public override IndexInput OpenInput(string name)
        {
            return new AzureDataCacheIndexInput(_fileSystemNamespace + name, _cacheRegion, _persistantCache);
        }

        public override void DeleteFile(string name)
        {
            _persistantCache.Remove(_fileSystemNamespace + name, _cacheRegion);
            _persistantCache.Remove(_modifiedNamespace + name, _cacheRegion);
        }

        public override bool FileExists(string name)
        {
            var cacheItem = _persistantCache.GetCacheItem(_fileSystemNamespace + name, _cacheRegion);
            return cacheItem != null;
        }

        public override long FileLength(string name)
        {
            var size = _persistantCache.Get(_fileSizeNamespace + name, _cacheRegion);
            return size == null ? 0 : (long)size;
        }

        public override long FileModified(string name)
        {
            var modified = _persistantCache.Get(_modifiedNamespace + name, _cacheRegion);
            if (modified == null)
            {
                throw new FileNotFoundException(name);
            }

            return (long)modified;
        }

        [Obsolete]
        public override void RenameFile(string from, string to)
        {
            throw new NotImplementedException();
        }

        public override void TouchFile(string name)
        {
            var now = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            _persistantCache.Put(_modifiedNamespace + name, now, new List<DataCacheTag> { _modifiedTag }, _cacheRegion);
        }

        [Obsolete]
        public override string[] List()
        {
            throw new NotImplementedException();
        }

        public override string[] ListAll()
        {
            var data = _persistantCache.GetObjectsByTag(_modifiedTag, _cacheRegion);

            var files = data.Select(d => d.Key.Replace(_modifiedNamespace, string.Empty)).ToArray();

            return files;
        }

        public override void Close()
        {
            // Do nothing for now
        }

        public override void Dispose()
        {
            Close();
        }
    }
}