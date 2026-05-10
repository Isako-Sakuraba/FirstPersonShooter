using System;

namespace Game.Utils
{
    public struct PooledObjectHandle
    {
        public int Index;
        public uint Generation;
    }

    public class HardObjectPool<T> : IDisposable where T : class
    {
        private readonly T[] _items;
        private readonly uint[] _generations;
        private readonly bool[] _active;

        private readonly int _capacity;
        private readonly bool _collectionCheck;

        private readonly Func<T> _createFunc;
        private readonly Action<T> _actionOnGet;
        private readonly Action<T> _actionOnRelease;
        private readonly Action<T> _actionOnReuse;

        private int _head;
        private int _tail;
        private int _activeCount;

        public int CountAll => _capacity;
        public int CountActive => _activeCount;
        public int CountInactive => CountAll - CountActive;

        public HardObjectPool(
            Func<T> createFunc,
            Action<T> actionOnGet,
            Action<T> actionOnRelease,
            Action<T> actionOnReuse,
            int capacity = 1,
            bool collectionCheck = true)
        {
            capacity = capacity < 1 ? 1 : capacity;

            _items = new T[capacity];
            _generations = new uint[capacity];
            _active = new bool[capacity];

            _capacity = capacity;

            _createFunc = createFunc;
            _actionOnGet = actionOnGet;
            _actionOnRelease = actionOnRelease;
            _actionOnReuse = actionOnReuse;
            _collectionCheck = collectionCheck;

            InitializePool();
        }

        private void InitializePool()
        {
            for (int i = 0; i < _capacity; i++)
            {
                _items[i] = _createFunc.Invoke();
                _active[i] = false;
            }
        }

        public void Dispose()
        {
            // TODO: Dispose!
        }

        public PooledObjectHandle Get()
        {
            int index;

            if (_activeCount == _capacity)
            {
                // reuse oldest
                index = _head;

                var reused = _items[index];
                _actionOnReuse?.Invoke(reused);

                _generations[index]++;
                _head = (_head + 1) % _capacity;
            }
            else
            {
                index = _tail;
                _activeCount++;
            }

            _active[index] = true;
            _tail = (index + 1) % _capacity;

            var item = _items[index];
            _actionOnGet?.Invoke(item);

            return new PooledObjectHandle
            {
                Index = index,
                Generation = _generations[index]
            };
        }

        public bool Release(PooledObjectHandle handle)
        {
            int index = handle.Index;

            if (_generations[index] != handle.Generation)
                return false;

            if (!_active[index])
                return false;

            _active[index] = false;
            _activeCount--;

            _actionOnRelease?.Invoke(_items[index]);

            if (index == _head)
            {
                while (_activeCount > 0 && !_active[_head])
                    _head = (_head + 1) % _capacity;
            }

            return true;
        }

        public T GetItem(PooledObjectHandle handle)
        {
            return _items[handle.Index];
        }

        public T PeekOldestActive()
        {
            if (_activeCount == 0)
                return null;
            return _items[_head];
        }
    }
}