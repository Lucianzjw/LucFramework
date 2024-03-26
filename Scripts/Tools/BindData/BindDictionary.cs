using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LucFramework.Scripts.Tools.BindData
{
    public class BindDictionary<TKey, TValue> : BindableEnumerableBase<TKey, TValue>, IDictionary<TKey, TValue>
    {
        protected event EventHandler<EnumerableChangedEventArgs<TKey, TValue>> _onInterChanged;
        private Dictionary<TKey, TValue> _dictionary = new Dictionary<TKey, TValue>();

        public override IDictionary<TKey, TValue> Value
        {
            get => _dictionary;
            set
            {
                _dictionary = new Dictionary<TKey, TValue>(value);
                OnValueChanged();
            }
        }

        public ICollection<TKey> Keys => _dictionary.Keys;

        public ICollection<TValue> Values => _dictionary.Values;

        public int Count => _dictionary.Count;

        public bool IsReadOnly => false;

        public TValue this[TKey key]
        {
            get => _dictionary[key];
            set
            {
                if (!_dictionary.TryGetValue(key, out var currentValue) ||
                    !EqualityComparer<TValue>.Default.Equals(currentValue, value))
                {
                    _dictionary[key] = value;
                    RaiseInterChanged((byte)ChangeType.Modified, key, value);
                }
            }
        }

        public void Add(TKey key, TValue value)
        {
            _dictionary.Add(key, value);
            RaiseInterChanged((byte)ChangeType.Added, key, value);
        }

        public bool Remove(TKey key)
        {
            if (!_dictionary.Remove(key, out var currentValue)) return false;
            RaiseInterChanged((byte)ChangeType.Removed, key, currentValue);
            return true;
        }

        public bool ContainsKey(TKey key)
        {
            return _dictionary.ContainsKey(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return _dictionary.TryGetValue(key, out value);
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            _dictionary.Add(item.Key, item.Value);
            RaiseInterChanged((byte)ChangeType.Added, item.Key, item.Value);
        }

        public void Clear()
        {
            _dictionary.Clear();
            RaiseInterChanged((byte)ChangeType.Cleared, default, default);
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return _dictionary.Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ((IDictionary<TKey, TValue>)_dictionary).CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (!_dictionary.Remove(item.Key)) return false;
            RaiseInterChanged((byte)ChangeType.Removed, item.Key, item.Value);
            return true;
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

    }
}