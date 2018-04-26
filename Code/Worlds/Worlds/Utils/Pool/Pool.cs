// Object pooling pattern
namespace WorldsGame.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;

    public enum LoadingMode { Eager, Lazy, LazyExpanding };

    public enum AccessMode { FIFO, LIFO, Circular };

    public class PoolIsEmptyException : Exception
    {
        public PoolIsEmptyException(string message)
            : base(message)
        {
        }
    }

    public class Pool<T> : IDisposable where T : IPoolable
    {
        private bool _isDisposed;
        private readonly Func<Pool<T>, T> _factory;
        private readonly LoadingMode _loadingMode;
        private readonly IItemStore _itemStore;
        private readonly int _size;
        private int _count;
        private readonly Semaphore _sync;
        private int _aquiredSize;

        public Pool(int size, Func<Pool<T>, T> factory)
            : this(size, factory, LoadingMode.Lazy, AccessMode.FIFO)
        {
        }

        public Pool(int size, Func<Pool<T>, T> factory,
            LoadingMode loadingMode, AccessMode accessMode)
        {
            if (size <= 0)
                throw new ArgumentOutOfRangeException("size", size,
                    "Argument 'size' must be greater than zero.");
            if (factory == null)
                throw new ArgumentNullException("factory");

            _size = size;
            _factory = factory;
            _sync = new Semaphore(size, size);
            _loadingMode = loadingMode;
            _itemStore = CreateItemStore(accessMode, size);
            if (loadingMode == LoadingMode.Eager)
            {
                PreloadItems();
            }
        }

        public T Acquire()
        {
            //            if (_aquiredSize == _size)
            //            {
            //                throw new PoolIsEmptyException("");
            //            }

            T result;
            _sync.WaitOne();
            switch (_loadingMode)
            {
                case LoadingMode.Eager:
                    result = AcquireEager();
                    break;

                case LoadingMode.Lazy:
                    result = AcquireLazy();
                    break;

                default:
                    Debug.Assert(_loadingMode == LoadingMode.LazyExpanding,
                                    "Unknown LoadingMode encountered in Acquire method.");
                    result = AcquireLazyExpanding();
                    break;
            }

            _aquiredSize++;

            return result;
        }

        public void Release(T item)
        {
            item.Clear();
            lock (_itemStore)
            {
                _itemStore.Store(item);
            }
            _sync.Release();
            _aquiredSize--;
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }
            _isDisposed = true;
            if (typeof(IDisposable).IsAssignableFrom(typeof(T)))
            {
                lock (_itemStore)
                {
                    while (_itemStore.Count > 0)
                    {
                        var disposable = (IDisposable)_itemStore.Fetch();
                        disposable.Dispose();
                    }
                }
            }
            _sync.Close();
        }

        #region Acquisition

        private T AcquireEager()
        {
            lock (_itemStore)
            {
                return _itemStore.Fetch();
            }
        }

        private T AcquireLazy()
        {
            lock (_itemStore)
            {
                if (_itemStore.Count > 0)
                {
                    return _itemStore.Fetch();
                }
            }
            Interlocked.Increment(ref _count);
            return _factory(this);
        }

        private T AcquireLazyExpanding()
        {
            bool shouldExpand = false;
            if (_count < _size)
            {
                int newCount = Interlocked.Increment(ref _count);
                if (newCount <= _size)
                {
                    shouldExpand = true;
                }
                else
                {
                    // Another thread took the last spot - use the store instead
                    Interlocked.Decrement(ref _count);
                }
            }

            if (shouldExpand)
            {
                return _factory(this);
            }

            lock (_itemStore)
            {
                return _itemStore.Fetch();
            }
        }

        private void PreloadItems()
        {
            for (int i = 0; i < _size; i++)
            {
                T item = _factory(this);
                _itemStore.Store(item);
            }
            _count = _size;
        }

        #endregion Acquisition

        #region Collection Wrappers

        private interface IItemStore
        {
            T Fetch();

            void Store(T item);

            int Count { get; }
        }

        private IItemStore CreateItemStore(AccessMode mode, int capacity)
        {
            switch (mode)
            {
                case AccessMode.FIFO:
                    return new QueueStore(capacity);
                case AccessMode.LIFO:
                    return new StackStore(capacity);
                default:
                    Debug.Assert(mode == AccessMode.Circular,
                        "Invalid AccessMode in CreateItemStore");
                    return new CircularStore(capacity);
            }
        }

        private class QueueStore : Queue<T>, IItemStore
        {
            public QueueStore(int capacity)
                : base(capacity)
            {
            }

            public T Fetch()
            {
                return Dequeue();
            }

            public void Store(T item)
            {
                Enqueue(item);
            }
        }

        private class StackStore : Stack<T>, IItemStore
        {
            public StackStore(int capacity)
                : base(capacity)
            {
            }

            public T Fetch()
            {
                return Pop();
            }

            public void Store(T item)
            {
                Push(item);
            }
        }

        private class CircularStore : IItemStore
        {
            private readonly List<Slot> _slots;
            private int _freeSlotCount;
            private int _position = -1;

            public CircularStore(int capacity)
            {
                _slots = new List<Slot>(capacity);
            }

            public T Fetch()
            {
                if (Count == 0)
                    throw new InvalidOperationException("The buffer is empty.");

                int startPosition = _position;
                do
                {
                    Advance();
                    Slot slot = _slots[_position];
                    if (!slot.IsInUse)
                    {
                        slot.IsInUse = true;
                        --_freeSlotCount;
                        return slot.Item;
                    }
                } while (startPosition != _position);
                throw new InvalidOperationException("No free slots.");
            }

            public void Store(T item)
            {
                Slot slot = _slots.Find(s => object.Equals(s.Item, item));
                if (slot == null)
                {
                    slot = new Slot(item);
                    _slots.Add(slot);
                }
                slot.IsInUse = false;
                ++_freeSlotCount;
            }

            public int Count
            {
                get { return _freeSlotCount; }
            }

            private void Advance()
            {
                _position = (_position + 1) % _slots.Count;
            }

            private class Slot
            {
                public Slot(T item)
                {
                    Item = item;
                }

                public T Item { get; private set; }

                public bool IsInUse { get; set; }
            }
        }

        #endregion Collection Wrappers

        public bool IsDisposed
        {
            get { return _isDisposed; }
        }
    }

    // Пример использования

    //    public interface IFoo : IDisposable
    //    {
    //        void Test();
    //    }
    //
    //    public class Foo : IFoo
    //    {
    //        private static int _count = 0;
    //
    //        private readonly int num;
    //
    //        public Foo()
    //        {
    //            num = Interlocked.Increment(ref _count);
    //        }
    //
    //        public void Dispose()
    //        {
    //            Console.WriteLine("Goodbye from Foo #{0}", num);
    //        }
    //
    //        public void Test()
    //        {
    //            Console.WriteLine("Hello from Foo #{0}", num);
    //        }
    //    }
    //
    //    public class PooledFoo : IFoo
    //    {
    //        private readonly Foo _internalFoo;
    //        private readonly Pool<IFoo> _pool;
    //
    //        public PooledFoo(Pool<IFoo> pool)
    //        {
    //            if (pool == null)
    //                throw new ArgumentNullException("pool");
    //
    //            _pool = pool;
    //            _internalFoo = new Foo();
    //        }
    //
    //        public void Dispose()
    //        {
    //            if (_pool.IsDisposed)
    //            {
    //                _internalFoo.Dispose();
    //            }
    //            else
    //            {
    //                _pool.Release(this);
    //            }
    //        }
    //
    //        public void Test()
    //        {
    //            _internalFoo.Test();
    //        }
    //    }
}