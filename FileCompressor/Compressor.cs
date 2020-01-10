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
            using (var context = new CompressionContext(inFilePath, toFilePath))
            {
                ExecuteCompression(context, cancellationToken);
            }
        }

        public void Decompress(string inFilePath, string toFilePath, CancellationToken cancellationToken)
        {
            using (var context = new DecompressionContext(inFilePath, toFilePath))
            {
                ExecuteCompression(context, cancellationToken);
            }
        }

        private void ExecuteCompression<TRead, TWrite>(BaseContext<TRead, TWrite> context, CancellationToken cancellationToken)
            where TRead : BaseChunk
            where TWrite : BaseChunk
        {
            var threadsCount = Math.Min(_degreeOfParallelism, context.PartitionsCount);
            _waitHandles = new WaitHandle[threadsCount];
            for (var i = 0; i < threadsCount; i++)
            {
                StartCompressionThread(context, i, cancellationToken);
            }

            WaitHandle.WaitAll(_waitHandles);
            if (context.Exception != null)
            {
                throw context.Exception;
            }
        }

        private void StartCompressionThread<TRead, TWrite>(BaseContext<TRead, TWrite> context, int index, CancellationToken cancellationToken)
            where TRead : BaseChunk
            where TWrite : BaseChunk
        {
            var handle = new EventWaitHandle(false, EventResetMode.ManualReset);
            _waitHandles[index] = handle;
            var thread = new Thread(() =>
            {
                try
                {
                    while (Interlocked.Decrement(ref context.PartitionsCount) >= 0 && !cancellationToken.IsCancellationRequested)
                    {
                        var readChunk = context.ReadChunk();
                        var writeChunk = context.ConvertReadToWriteModel(readChunk);
                        context.WriteChunk(writeChunk);
                    }
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
