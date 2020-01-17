using System;
using System.Threading;

namespace FileCompressor.Services
{
    public class CompressionThreadPool
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private int _position = 0;
        private Exception _exception;
        private readonly WaitHandle[] _waitHandles;

        public CompressionThreadPool()
        {
            _waitHandles = new WaitHandle[Environment.ProcessorCount];
        }

        public CompressionThreadPool(int threadsCount)
        {
            _waitHandles = new WaitHandle[threadsCount];
        }

        public void StartThread(Action<CancellationToken> action)
        {
            var handle = new EventWaitHandle(false, EventResetMode.ManualReset);
            _waitHandles[_position++] = handle;
            var thread = new Thread(() =>
            {
                try
                {
                    action(_cancellationTokenSource.Token);
                }
                catch (Exception exception)
                {
                    _exception = exception;
                    _cancellationTokenSource.Cancel();
                }
                finally
                {
                    handle.Set();
                }
            });
            thread.Start();
        }

        public void WaitAllAndThowExceptionIfExists()
        {
            WaitHandle.WaitAll(_waitHandles);
            if (_exception != null)
            {
                throw _exception;
            }
        }

        public bool CanAdd()
        {
            return _position < _waitHandles.Length; 
        }
    }
}
