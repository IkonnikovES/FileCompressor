using System.Collections.Generic;

namespace FileCompressor.Services
{
    public class CompressionQueue<T>
    {
        private readonly Queue<T> _queue = new Queue<T>();
        private readonly int _minBufferSize;

        private int _chunksCount;

        public CompressionQueue(int minBufferSize, int chunksCount)
        {
            _minBufferSize = minBufferSize;
            _chunksCount = chunksCount;
        }

        public bool TryDequeue(out T item)
        {
            lock (_queue)
            {
                if (_queue.Count > 0 && (_queue.Count > _minBufferSize || _minBufferSize > _chunksCount))
                {
                    item = _queue.Dequeue();
                    return true;
                }
                item = default;
                return false;
            }
        }

        public void Enqueue(T item)
        {
            lock (_queue)
            {
                _chunksCount--;
                _queue.Enqueue(item);
            }
        }
    }
}
