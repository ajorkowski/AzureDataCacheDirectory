using System.IO;
using Microsoft.ApplicationServer.Caching;

namespace Lucene.Net.Store.Azure
{
    public class AzureDataCacheIndexInput : IndexInput
    {
        private readonly DataCache _persistantCache;
        private readonly string _name;
        private readonly string _cacheRegion;
        private readonly Stream _stream;

        public AzureDataCacheIndexInput(string name, string cacheRegion, DataCache persistantCache)
        {
            _persistantCache = persistantCache;
            _name = name;
            _cacheRegion = cacheRegion;

            var data = _persistantCache.Get(name, _cacheRegion);
            byte[] streamData = data == null ? new byte[] { } : (byte[])data;
            _stream = new MemoryStream(streamData, false);
        }

        public override long FilePointer
        {
            get
            {
                return _stream.Position;
            }
        }

        public override long Length()
        {
            return _stream.Length;
        }

        public override byte ReadByte()
        {
            return (byte)_stream.ReadByte();
        }

        public override void ReadBytes(byte[] b, int offset, int len)
        {
            _stream.Read(b, offset, len);
        }

        public override void Seek(long pos)
        {
            _stream.Seek(pos, SeekOrigin.Begin);
        }

        public override object Clone()
        {
            var result = new AzureDataCacheIndexInput(_name, _cacheRegion, _persistantCache);
            result.Seek(FilePointer);
            return result;
        }

        private bool _disposed;
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _stream.Close();
                }

                _disposed = true;
            }
        }
    }
}