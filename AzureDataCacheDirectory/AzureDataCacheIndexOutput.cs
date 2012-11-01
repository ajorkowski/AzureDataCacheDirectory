using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.ApplicationServer.Caching;

namespace Lucene.Net.Store.Azure
{
    public class AzureDataCacheIndexOutput : IndexOutput
    {
        private readonly DataCache _persistantCache;
        private readonly string _name;
        private readonly string _sizeName;
        private readonly string _cacheRegion;
        private readonly string _modifiedName;
        private readonly DataCacheTag _modifiedTag;
        private readonly MemoryStream _stream;

        public AzureDataCacheIndexOutput(string name, string sizeName, string cacheRegion, string modifiedName, DataCacheTag modifiedTag, DataCache persistantCache)
        {
            _name = name;
            _sizeName = sizeName;
            _persistantCache = persistantCache;
            _cacheRegion = cacheRegion;
            _modifiedName = modifiedName;
            _modifiedTag = modifiedTag;
            _stream = new MemoryStream();
        }

        public override long FilePointer
        {
            get { return _stream.Position; }
        }

        public override long Length
        {
            get { return _stream.Length; }
        }

        public override void Seek(long pos)
        {
            _stream.Seek(pos, SeekOrigin.Begin);
        }

        public override void WriteByte(byte b)
        {
            _stream.WriteByte(b);
        }

        public override void WriteBytes(byte[] b, int offset, int length)
        {
            _stream.Write(b, offset, length);
        }

        public override void Flush()
        {
            _stream.Flush();
        }

        private bool _disposed;
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _stream.Flush();
                    var data = _stream.ToArray();
                    _stream.Dispose();
                    var now = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                    _persistantCache.Put(_name, data, _cacheRegion);
                    _persistantCache.Put(_modifiedName, now, new List<DataCacheTag> { _modifiedTag }, _cacheRegion);
                    _persistantCache.Put(_sizeName, (long)data.Length, _cacheRegion);
                }

                _disposed = true;
            }
        }
    }
}