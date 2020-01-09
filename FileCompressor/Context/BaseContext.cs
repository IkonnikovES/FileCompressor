using FileCompressor.Models;
using System;
using System.IO;
using System.Threading;

namespace FileCompressor.Context
{
    public abstract class BaseContext<TRead, TWrite> : IDisposable
        where TRead : BaseChunk
        where TWrite : BaseChunk
    {
        public const int BufferSize = 1024 * 1024 * 32;

        private static readonly object _readSyncObject = new object();
        private static readonly object _writeSyncObject = new object();

        protected readonly FileStream InStream;
        protected readonly FileStream ToStream;

        public int PartitionsCount;

        public BaseContext(string inFilePath, string toFilePath)
        {
            InStream = File.OpenRead(inFilePath);
            ToStream = File.Create(toFilePath);
            PartitionsCount = InitialPartitionsCount();
        }

        protected long LeftBytes => InStream.Length - InStream.Position;
        public Exception Exception { get; set; }

        protected abstract int InitialPartitionsCount();
        protected abstract TRead Read();
        protected abstract void Write(TWrite chunk);

        public abstract TWrite ConvertReadToWriteModel(TRead readChunk);

        public TRead ReadChunk()
        {
            lock(_readSyncObject)
            {
                return Read();
            }
        }

        public void WriteChunk(TWrite chunk)
        {
            lock (_writeSyncObject)
            {
                Write(chunk);
            }
        }

        public void Dispose()
        {
            InStream.Close();
            ToStream.Close();
        }
    }
}
