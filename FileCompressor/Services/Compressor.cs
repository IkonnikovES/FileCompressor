using FileCompressor.Context;
using FileCompressor.Models;
using FileCompressor.Services;
using System;
using System.Collections.Concurrent;
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
            var partCount = context.PartitionsCount;
            var compressionPool = new CompressionPool();
            var queue = new ConcurrentQueue<TRead>();

            compressionPool.Start(ct =>
            {
                while (Interlocked.Decrement(ref partCount) >= 0)
                {
                    ct.ThrowIfCancellationRequested();
                    if (queue.TryDequeue(out var readChunk))
                    {
                        var compressedChunk = context.ConvertReadToWriteModel(readChunk);
                        context.WriteChunk(compressedChunk);
                    }
                    else
                    {
                        queue.Enqueue(context.ReadChunk());
                    }
                }
            });
            compressionPool.WaitAll();
        }
    }
}
