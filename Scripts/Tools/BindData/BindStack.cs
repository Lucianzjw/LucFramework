using System.Collections;
using System.Collections.Generic;

namespace LucFramework.Scripts.Tools.BindData
{
    public interface IObservableStack<T> :  IEnumerable<T>
    {
        void Push(T item);
    
        T Pop();
    
        T Peek();
    
        int Count { get; }
    }


    public class BindStock<T> : BindableEnumerableBase<T>,IObservableStack<T>
    {
        private Stack<T> _stack = new Stack<T>();

        public override IEnumerable<T> Value
        {
            get => _stack;
            set
            {
                _stack = new Stack<T>();
                OnValueChanged();
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _stack.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Push(T item)
        {
            _stack.Push(item);
            RaiseInterChanged((byte)ChangeType.Added, item);
        }

        public T Pop()
        {
            var item = _stack.Pop();
            RaiseInterChanged((byte)ChangeType.Modified, item);
            return item;
        }

        public T Peek()
        {
            var item = _stack.Peek();
            RaiseInterChanged((byte)ChangeType.Peek, item);
            return item;
        }

        public int Count => _stack.Count;
    }
}