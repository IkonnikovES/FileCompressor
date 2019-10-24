using FileCompressor.Context;
using FileCompressor.Models;
using System;
using System.Threading;

namespace FileCompressor
{
    public class Compressor
    {
        private readonly Semaphore _syncObject;

        public Compressor(int degreeOfParallelism)
        {
            _syncObject = new Semaphore(degreeOfParallelism, degreeOfParallelism);
        }

        public void Compress(string inFilePath, string toFilePath)
        {
            using (var context = new CompressionContext(inFilePath, toFilePath))
            {
                ExecuteCompression(context);
            }
        }

        public void Decompress(string inFilePath, string toFilePath)
        {
            using (var context = new DecompressionContext(inFilePath, toFilePath))
            {
                ExecuteCompression(context);
            }
        }

        private void ExecuteCompression<TRead, TWrite>(BaseContext<TRead, TWrite> context)
            where TRead : BaseChunk
            where TWrite : BaseChunk
        {
            while (context.CanRead)
            {
                _syncObject.WaitOne();
                var thread = new Thread(() =>
                {
                    var readChunk = context.Read();
                    var writeChunk = context.ConvertReadToWriteModel(readChunk);
                    context.Write(writeChunk);
                    _syncObject.Release();
                });
                thread.Start();
            }
        }
    }
}
