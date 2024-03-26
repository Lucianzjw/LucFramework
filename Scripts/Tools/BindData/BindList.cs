using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LucFramework.Scripts.Tools.BindData
{
    public class BindList<T> : BindableEnumerableBase<T>,IList<T>
    {
        private List<T> _list = new List<T>();

        public override IEnumerable<T> Value
        {
            get => _list;
            set
            {
                _list = new List<T>(value);
                OnValueChanged();
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(T item)
        {
            _list.Add(item);
            RaiseInterChanged((byte)ChangeType.Added, item);
        }

        public void Clear()
        {
            _list.Clear();
            RaiseInterChanged((byte)ChangeType.Cleared, default(T));
        }

        public bool Contains(T item)
        {
            return _list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            var result = _list.Remove(item);
            if (result)
            {
                RaiseInterChanged((byte)ChangeType.Removed, item);
            }

            return result;
        }

        public int Count => _list.Count();

        public bool IsReadOnly => false;

        public int IndexOf(T item)
        {
            return _list.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            _list.Insert(index, item);
            RaiseInterChanged((byte)ChangeType.Added, item);
        }

        public void RemoveAt(int index)
        {
            var item = _list[index];
            _list.RemoveAt(index);
            RaiseInterChanged((byte)ChangeType.Removed, item);
        }

        public T this[int index]
        {
            get => _list[index];
            set
            {
                if (EqualityComparer<T>.Default.Equals(_list[index], value)) return;
                _list[index] = value;
                RaiseInterChanged((byte)ChangeType.Modified, value);
            }
        }

    }
}