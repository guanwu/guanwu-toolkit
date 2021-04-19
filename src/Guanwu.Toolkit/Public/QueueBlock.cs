using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Guanwu.Toolkit
{
    public class QueueBlock<T>
    {
        public event EventHandler<Exception> OnException;

        /// <summary>
        /// Occurs when an item from the <see cref="QueueBlock{T}"/> is taked in the specified time period.
        /// </summary>
        public event EventHandler<T> OnTaked;

        /// <summary>
        /// Occurs when an item from the <see cref="QueueBlock{T}"/> is not taked in the specified time period.
        /// </summary>
        public event EventHandler<T> OnTakeBlocked;

        /// <summary>
        /// Occurs when an operation that cancellation of taking item from the <see cref="QueueBlock{T}"/> is performed.
        /// </summary>
        public event EventHandler<T> OnTakingCanceled;

        /// <summary>
        /// Occurs when an operation that completion of taking item from the <see cref="QueueBlock{T}"/> is performed.
        /// </summary>
        public event EventHandler<T> OnTakingCompleted;

        /// <summary>
        /// Occurs when an item is added to the <see cref="QueueBlock{T}"/> in the specified time period. 
        /// </summary>
        public event EventHandler<T> OnAdded;

        /// <summary>
        /// Occurs when an item is not added to the <see cref="QueueBlock{T}"/> in the specified time period. 
        /// </summary>
        public event EventHandler<T> OnAddBlocked;

        /// <summary>
        /// Occurs when an operation that cancellation of adding item to the <see cref="QueueBlock{T}"/> is performed.
        /// </summary>
        public event EventHandler<T> OnAddingCanceled;

        /// <summary>
        /// Gets whether this QueueBlock<typeparamref name="T"/> has been marked as complete for adding.
        /// </summary>
        /// <exception cref="ObjectDisposedException" />
        public bool IsAddingCompleted => _collection.IsAddingCompleted;

        /// <summary>
        /// Gets whether this QueueBlock<typeparamref name="T"/> has been marked as complete for adding and is empty.
        /// </summary>
        /// <exception cref="ObjectDisposedException" />
        public bool IsCompleted => _collection.IsCompleted;

        private BlockingCollection<T> _collection;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueueBlock{T}"/> class with a default upper-bound.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException" />
        public QueueBlock()
            : this(10) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueueBlock{T}"/> class with the specified upper-bound.
        /// </summary>
        /// <param name="boundedCapacity">The bounded size of the collection.</param>
        public QueueBlock(int boundedCapacity)
        {
            _collection = new BlockingCollection<T>(boundedCapacity);
        }

        public void CompleteAdding()
        {
            _collection.CompleteAdding();
        }

        public void Produce(CancellationToken ct, params T[] items)
        {
            if (items.Length == 0) return;
            int index = 0;
            T item = default;
            do {
                try {
                    item = items[index];
                    if (_collection.TryAdd(item, 10, ct)) {
                        Interlocked.Increment(ref index);
                        OnAdded?.Invoke(this, item);
                    } 
                    else {
                        OnAddBlocked?.Invoke(this, item);
                    }
                } 
                catch (OperationCanceledException) {
                    OnAddingCanceled?.Invoke(this, item);
                    break;
                } 
                catch (Exception ex) {
                    OnException?.Invoke(this, ex);
                }
            } 
            while (index < items.Length);
        }

        public void Consume(CancellationToken ct)
        {
            T item = default;
            while (!_collection.IsCompleted) {
                try {
                    if (_collection.TryTake(out item, 10, ct)) {
                        OnTaked?.Invoke(this, item);
                    } 
                    else {
                        OnTakeBlocked?.Invoke(this, item);
                    }
                } 
                catch (OperationCanceledException) {
                    OnTakingCanceled?.Invoke(this, item);
                    break;
                } 
                catch (Exception ex) {
                    OnException?.Invoke(this, ex);
                }
            }
            OnTakingCompleted?.Invoke(this, item);
        }

    }
}