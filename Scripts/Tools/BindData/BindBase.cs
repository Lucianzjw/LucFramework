using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace LucFramework.Scripts.Tools.BindData
{
    public enum ChangeType : byte
    {
        Added,
        Removed,
        Modified,
        Peek,
        Cleared
    }

    public interface IBindable<T>
    {
        T Value { get; set; }
        bool TryAddListener(Action<T> action);
        bool TryRemoveListener(Action<T> action);
        void ClearListeners();
    }

    public abstract class BindableBase<T> : IBindable<T>
    {
        private readonly ConcurrentDictionary<Action<T>, byte> _onValueChangedActions =
            new ConcurrentDictionary<Action<T>, byte>();

        protected virtual void OnValueChanged()
        {
            foreach (var action in _onValueChangedActions.Keys)
            {
                action?.Invoke(Value);
            }
        }

        public bool ContainsListener(Action<T> action)
        {
            return _onValueChangedActions.ContainsKey(action);
        }

        public abstract T Value { get; set; }

        public bool TryAddListener(Action<T> action)
        {
            return _onValueChangedActions.TryAdd(action, 0);
        }

        public bool TryRemoveListener(Action<T> action)
        {
            return _onValueChangedActions.TryRemove(action, out _);
        }

        public void ClearListeners()
        {
            _onValueChangedActions.Clear();
        }
    }

    public class EnumerableChangedEventArgs<T> : EventArgs
    {
        [Tooltip("自定义enum转成byte")] public byte Type { get; }

        public T ChangedItem { get; }

        public EnumerableChangedEventArgs(byte type, T changedItem)
        {
            Type = type;
            ChangedItem = changedItem;
        }
    }

    public class EnumerableChangedEventArgs<T1, T2> : EventArgs
    {
        [Tooltip("自定义enum转成byte")] public byte Type { get; }

        public T1 Key { get; }

        public T2 Value { get; }

        public EnumerableChangedEventArgs(byte type, T1 key, T2 value)
        {
            Type = type;
            Key = key;
            Value = value;
        }
    }

    public abstract class BindableEnumerableBase<T> : BindableBase<IEnumerable<T>>
    {
        public event EventHandler<EnumerableChangedEventArgs<T>> OnInterChanged;

        protected void RaiseInterChanged(byte type, T item)
        {
            OnInterChanged?.Invoke(this, new EnumerableChangedEventArgs<T>(type, item));
        }
    }

    public abstract class BindableEnumerableBase<T1, T2> : BindableBase<IDictionary<T1, T2>>
    {
        public event EventHandler<EnumerableChangedEventArgs<T1, T2>> OnInterChanged;

        protected void RaiseInterChanged(byte type, T1 key, T2 value)
        {
            OnInterChanged?.Invoke(this, new EnumerableChangedEventArgs<T1, T2>(type, key, value));
        }
    }
}