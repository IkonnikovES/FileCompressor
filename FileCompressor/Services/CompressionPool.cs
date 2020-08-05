using System;
using System.Threading;

namespace FileCompressor.Services
{
    public class CompressionPool
    {
        private readonly CancellationTokenSource _cancellationTokenSrc = new CancellationTokenSource();

        private Exception _exception;
        private readonly WaitHandle[] _waitHandles;

        public CompressionPool()
        {
            _waitHandles = new WaitHandle[Environment.ProcessorCount];
        }

        public CompressionPool(int threadsCount)
        {
            _waitHandles = new WaitHandle[threadsCount];
        }

        public void Start(Action<CancellationToken> action)
        {
            for (var i = 0; i < _waitHandles.Length; i++)
            {
                var handle = new ManualResetEvent(false);
                _waitHandles[i] = handle;

                var thread = new Thread(() =>
                {
                    try
                    {
                        action(_cancellationTokenSrc.Token);
                    }
                    catch (OperationCanceledException)
                    {

                    }
                    catch (Exception ex)
                    {
                        _exception = ex;
                        _cancellationTokenSrc.Cancel();
                    }
                    finally
                    {
                        handle.Set();
                    }
                });

                thread.Start();
            }
        }

        public void WaitAll()
        {
            WaitHandle.WaitAll(_waitHandles);
            if (_exception != null)
            {
                throw _exception;
            }
        }
    }
}
