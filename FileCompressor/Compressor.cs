using FileCompressor.Context;
using FileCompressor.Models;
using System;
using System.Threading;

namespace FileCompressor
{
    public class Compressor
    {
        private readonly int _degreeOfParallelism;

        public Compressor(int degreeOfParallelism)
        {
            _degreeOfParallelism = degreeOfParallelism;
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
            var threadsCount = Math.Min(_degreeOfParallelism, context.PartitionsCount);
            for (var i = 0; i < threadsCount; i++)
            {
                var thread = new Thread(() =>
                {
                    while (context.CanRead)
                    {
                        var readChunk = context.Read();
                        var writeChunk = context.ConvertReadToWriteModel(readChunk);
                        context.Write(writeChunk);
                    }
                });
                thread.Start();
            }
        }
    }
}
