using System.Collections;
using System.Collections.Generic;

namespace LucFramework.Scripts.Tools.BindData
{
    public interface IObservableQueue<T> :  IEnumerable<T>
    {
    
        void Enqueue(T item);
    
        T Dequeue();
    
        T Peek();
    
        int Count { get; }
    }

    public class BindQueue<T> : BindableEnumerableBase<T>, IObservableQueue<T>
    {

        private Queue<T> _queue = new Queue<T>();

        public override IEnumerable<T> Value { 
            get => _queue;
            set
            {
                _queue = new Queue<T>(value);
                OnValueChanged();
            } 
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _queue.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Enqueue(T item)
        {
            _queue.Enqueue(item);
            RaiseInterChanged((byte)ChangeType.Added, item);
        }

        public T Dequeue()
        {
            var item = _queue.Dequeue();
            RaiseInterChanged((byte)ChangeType.Removed, item);
            return item;
        }

        public T Peek()
        {
            var item = _queue.Peek();
            RaiseInterChanged((byte)ChangeType.Peek, item);
            return item;
        }

        public int Count => _queue.Count;
    }
}