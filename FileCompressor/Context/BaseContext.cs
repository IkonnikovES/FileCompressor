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
        public const int BufferSize = 1024 * 1024;

        private static readonly object _writeSyncObject = new object();
        private static readonly object _readSyncObject = new object();

        private readonly ManualResetEvent _disposeSyncObject = new ManualResetEvent(false);

        protected readonly FileStream InStream;
        protected readonly FileStream ToStream;

        private int _threadsCount;

        private int _finishedThreadsCount = 0;
        public int _startedThreadsCount = 0;

        public BaseContext(string inFilePath, string toFilePath)
        {
            InStream = File.OpenRead(inFilePath);
            ToStream = File.Create(toFilePath);
            _threadsCount = InitialPartitionsCount();
        }

        public long LeftBytes => InStream.Length - InStream.Position;
        public bool CanRead => _threadsCount > 0 && _threadsCount >= ++_startedThreadsCount;

        protected abstract int InitialPartitionsCount();
        protected abstract TRead ReadChunk();
        public abstract void WriteChunk(TWrite chunk);
        public abstract TWrite ConvertReadToWriteModel(TRead readChunk);

        public TRead Read()
        {
            lock(_readSyncObject)
            {
                var chunk = ReadChunk();
                return chunk;
            }
        }

        public void Write(TWrite chunk)
        {
            lock (_writeSyncObject)
            {
                WriteChunk(chunk);
                if (++_finishedThreadsCount == _threadsCount)
                {
                    _disposeSyncObject.Set();
                }
            }
        }

        public void Dispose()
        {
            _disposeSyncObject.WaitOne();
            InStream.Close();
            ToStream.Close();
        }
    }
}
