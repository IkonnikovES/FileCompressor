using System;
using System.Collections.Generic;

namespace FileCompressor.Services
{
    public class CompressionQueue<T>
    {
        private readonly Queue<T> _queue = new Queue<T>();
        private readonly int _minBuffer;

        private int _capacity;

        public CompressionQueue(int minBuffer, int capacity)
        {
            _minBuffer = minBuffer;
            _capacity = capacity;
        }

        public bool HasFreeSpace()
        {
            lock (_queue)
            {
                return _capacity > 0;
            }
        }

        public bool TryDequeue(out T item)
        {
            lock (_queue)
            {
                if (_queue.Count > 0 && (_queue.Count > _minBuffer || _minBuffer > _capacity))
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
                if (--_capacity < 0)
                {
                    throw new ArgumentOutOfRangeException("Недопустимое добавление в очередь");
                }

                _queue.Enqueue(item);
            }
        }
    }
}
