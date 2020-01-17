using FileCompressor.Context;
using FileCompressor.Models;
using FileCompressor.Services;
using System;
using System.Threading;

namespace FileCompressor
{
    public static class Compressor
    {
        public static void Compress(string inFilePath, string toFilePath)
        {
            using (var context = new CompressionContext(inFilePath, toFilePath))
            {
                ExecuteCompression(context);
            }
        }

        public static void Decompress(string inFilePath, string toFilePath)
        {
            using (var context = new DecompressionContext(inFilePath, toFilePath))
            {
                ExecuteCompression(context);
            }
        }

        private static void ExecuteCompression<TRead, TWrite>(BaseContext<TRead, TWrite> context)
            where TRead : BaseChunk
            where TWrite : BaseChunk
        {
            var threadsCount = Math.Min(Environment.ProcessorCount, context.PartitionsCount);
            var threadPool = new CompressionThreadPool(threadsCount);

            var partitionsCount = context.PartitionsCount;
            while (threadPool.CanAdd())
            {
                threadPool.StartThread(cancellationToken =>
                {
                    while (!cancellationToken.IsCancellationRequested && Interlocked.Decrement(ref partitionsCount) >= 0)
                    {
                        var readChunk = context.ReadChunk();
                        var writeChunk = context.ConvertReadToWriteModel(readChunk);
                        context.WriteChunk(writeChunk);
                    }
                });
            }
            threadPool.WaitAllAndThowExceptionIfExists();
        }
    }
}
