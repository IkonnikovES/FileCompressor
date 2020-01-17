using FileCompressor.Context;
using FileCompressor.Models;
using FileCompressor.Services;
using System;

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
            var compressionPool = new CompressionThreadPool(3);
            var partCount = context.PartitionsCount;

            var queueToCompress = new CompressionQueue<TRead>(Environment.ProcessorCount);
            var queueToWrite = new CompressionQueue<TWrite>(Environment.ProcessorCount);

            compressionPool.StartThread(cancellationToken =>
            {
                var readedCount = 0;
                while (!cancellationToken.IsCancellationRequested && readedCount++ <= partCount)
                {
                    queueToCompress.Add(context.ReadChunk());
                }
            });

            compressionPool.StartThread(cancellationToken =>
            {
                var compressedCount = 0;
                while (!cancellationToken.IsCancellationRequested && compressedCount++ <= partCount)
                {
                    var chunk = queueToCompress.Take();
                    queueToWrite.Add(context.ConvertReadToWriteModel(chunk));
                }
            });

            compressionPool.StartThread(cancellationToken =>
            {
                var writedCount = 0;
                while (!cancellationToken.IsCancellationRequested && writedCount++ <= partCount)
                {
                    var chunk = queueToWrite.Take();
                    context.WriteChunk(chunk);
                }
            });

            compressionPool.WaitAllAndThowExceptionIfExists();
        }
    }
}
