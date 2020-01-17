using System.Threading;

namespace FileCompressor.Services
{
    public class CompressionQueue<T>
    {
        private readonly T[] _items;

        private int _putPosition = 0;
        private int _takePosition = 0;

        private readonly Semaphore _availableSpaces;
        private readonly Semaphore _availableItems;

        public CompressionQueue(int capacity)
        {
            _items = new T[capacity];
            _availableSpaces = new Semaphore(capacity, capacity);
            _availableItems = new Semaphore(0, capacity);
        }

        public void Add(T item)
        {
            _availableSpaces.WaitOne();
            lock (_items)
            {
                _items[_putPosition] = item;
                _putPosition = (_putPosition + 1) % _items.Length;
            }
            _availableItems.Release();
        }

        public T Take()
        {
            _availableItems.WaitOne();
            T item;
            lock (_items)
            {
                item = _items[_takePosition];
                _items[_takePosition] = default;
                _takePosition = (_takePosition + 1) % _items.Length;
            }
            _availableSpaces.Release();

            return item;
        }
    }
}
