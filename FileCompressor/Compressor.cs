using FileCompressor.Context;
using FileCompressor.Models;
using System;
using System.Threading;

namespace FileCompressor
{
    public class Compressor
    {
        private WaitHandle[] _waitHandles;
        private readonly int _degreeOfParallelism;
        private readonly Action _cancelCompression;

        public Compressor(int degreeOfParallelism, Action cancelCompression)
        {
            _degreeOfParallelism = degreeOfParallelism;
            _cancelCompression = cancelCompression;
        }

        public void Compress(string inFilePath, string toFilePath, CancellationToken cancellationToken)
        {
            using (var context = new CompressionContext(inFilePath, toFilePath, cancellationToken))
            {
                ExecuteCompression(context);
            }
        }

        public void Decompress(string inFilePath, string toFilePath, CancellationToken cancellationToken)
        {
            using (var context = new DecompressionContext(inFilePath, toFilePath, cancellationToken))
            {
                ExecuteCompression(context);
            }
        }

        private void ExecuteCompression<TRead, TWrite>(BaseContext<TRead, TWrite> context)
            where TRead : BaseChunk
            where TWrite : BaseChunk
        {
            var threadsCount = Math.Min(_degreeOfParallelism, context.PartitionsCount);
            _waitHandles = new WaitHandle[threadsCount];
            for (var i = 0; i < threadsCount; i++)
            {
                StartCompressionThread(context, i);
            }

            WaitHandle.WaitAll(_waitHandles);
            if (context.Exception != null)
            {
                throw context.Exception;
            }
        }

        private void StartCompressionThread<TRead, TWrite>(BaseContext<TRead, TWrite> context, int index)
            where TRead : BaseChunk
            where TWrite : BaseChunk
        {
            var handle = new EventWaitHandle(false, EventResetMode.ManualReset);
            _waitHandles[index] = handle;
            var thread = new Thread(() =>
            {
                try
                {
                    do
                    {
                        var readChunk = context.ReadChunk();
                        var writeChunk = context.ConvertReadToWriteModel(readChunk);
                        context.WriteChunk(writeChunk);
                    }
                    while (context.CheckCanReadAndNotCanceled());
                }
                catch (Exception ex)
                {
                    _cancelCompression();
                    context.Exception = ex;
                }
                finally
                {
                    handle.Set();
                    handle.Close();
                }
            });
            thread.Start();
        }
    }
}
