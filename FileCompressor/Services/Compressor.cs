using FileCompressor.Context;
using FileCompressor.Models;
using FileCompressor.Services;

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
            var threadPool = new CompressionThreadPool();
            var queue = new CompressionQueue<TRead>(threadPool.ThreadsCount, context.PartitionsCount);

            threadPool.Start(ct =>
            {
                while (queue.TryDequeue(out var readChunk) || queue.HasFreeSpace())
                {
                    ct.ThrowIfCancellationRequested();
                    if (readChunk != null)
                    {
                        var compressedChunk = context.ConvertReadToWriteModel(readChunk);
                        context.WriteChunk(compressedChunk);
                    }
                    else if (context.TryReadChunk(out var chunk))
                    {
                        queue.Enqueue(chunk);
                    }
                    else
                    {
                        break;
                    }
                }
            });
            threadPool.WaitAll();
        }
    }
}
