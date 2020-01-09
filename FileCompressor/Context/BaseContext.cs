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

        private int _partitionsCount;
        private readonly CancellationToken _cancellationToken;

        public BaseContext(string inFilePath, string toFilePath, CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;

            InStream = File.OpenRead(inFilePath);
            ToStream = File.Create(toFilePath);
            _partitionsCount = InitialPartitionsCount();
        }

        public int PartitionsCount => _partitionsCount;
        protected long LeftBytes => InStream.Length - InStream.Position;
        public Exception Exception { get; set; }

        protected abstract int InitialPartitionsCount();
        protected abstract TRead Read();
        protected abstract void Write(TWrite chunk);

        public abstract TWrite ConvertReadToWriteModel(TRead readChunk);

        public bool CheckCanReadAndNotCanceled()
        {
            return Interlocked.Decrement(ref _partitionsCount) > 0 && !_cancellationToken.IsCancellationRequested;
        }

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
