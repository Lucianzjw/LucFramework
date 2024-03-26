using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace LucFramework.Scripts.Tools.BindData
{
    public interface IOptimizeValue<T> : IDisposable
    {
        int _coolTime { get; set; }
        void OnValueChanged(T value);
    }

    public class BindModel<T> : BindableBase<T>
{
    private T _value;

    public override T Value
    {
        get => _value;
        set
        {
            if (EqualityComparer<T>.Default.Equals(value, _value)) return;
            _value = value;
            OnValueChanged();
        }
    }

    /// <summary>
    /// 添加监听并立即触发
    /// </summary>
    /// <param name="value"></param>
    /// <param name="action"></param>
    public bool AddListenerWithInit(Action<T> action)
    {
        if (ContainsListener(action))
            return false;
        action?.Invoke(_value);
        TryAddListener(action);
        return true;
    }

    public bool _isOptimize;
    public IOptimizeValue<T> _optimizeValue;

    protected override void OnValueChanged()
    {
        if (_isOptimize)
        {
            _optimizeValue.OnValueChanged(_value);
        }
        else
        {
            base.OnValueChanged();
        }
    }

    /// <summary>
    /// 开启节流，异步执行
    /// </summary>
    /// <param name="coolTime"></param>
    public void OpenThrottled(int coolTime)
    {
        _isOptimize = true;
        _optimizeValue = new Throttled<T>(coolTime, base.OnValueChanged);
    }

    public void Closethrottled()
    {
        _isOptimize = false;
        _optimizeValue.Dispose();
        _optimizeValue = null;
    }

    /// <summary>
    ///  开启防抖，异步执行
    /// </summary>
    /// <param name="coolTime"></param>
    public void OpenDebounce(int coolTime)
    {
        _isOptimize = true;
        _optimizeValue = new Debounce<T>(coolTime, base.OnValueChanged);
    }

    public void CloseDebounce()
    {
        _isOptimize = false;
        _optimizeValue.Dispose();
        _optimizeValue = null;
    }

    /// <summary>
    /// 修改节流或者防抖的时间间隔
    /// </summary>
    /// <param name="coolTime"></param>
    public void ChangeInterval(int coolTime)
    {
        if (_isOptimize)
        {
            _optimizeValue._coolTime = coolTime;
        }
    }
}


/// <summary>
/// Triggered at Fixed intervals
/// </summary>
/// <typeparam name="T"></typeparam>
public class Throttled<T> : IOptimizeValue<T>
{
    public int _coolTime { get; set; }
    private Action _action;
    private Task _throttledTask;
    private CancellationTokenSource _tokenSource;

    public Throttled(int coolTime, Action action)
    {
        _coolTime = coolTime;
        _action = action;
        _tokenSource = new CancellationTokenSource();
    }

    public async void OnValueChanged(T value)
    {
        if (_throttledTask is not null) return;
        _throttledTask = CreateDebounceTask(_tokenSource.Token);
        try
        {
            await _throttledTask;
        }
        catch (TaskCanceledException)
        {
            Debug.Log("Cancel Task");
        }
    }

    private async Task CreateDebounceTask(CancellationToken token)
    {
        await Task.Delay(_coolTime * 1000, token);
        _action?.Invoke();
        _throttledTask = null;
    }

    public void Dispose()
    {
        _tokenSource.Cancel();
        _tokenSource.Dispose();
    }
}

/// <summary>
/// Execute the action n seconds after the last invoke, if invoked again within n seconds, reset the timer
/// </summary>
/// <typeparam name="T"></typeparam>
public class Debounce<T> : IOptimizeValue<T>
{
    public int _coolTime { get; set; }
    private int _curCooltime;
    private Action _action;
    private Task _debounceTask;
    private CancellationTokenSource _tokenSource;

    public Debounce(int coolTime, Action action)
    {
        _coolTime = coolTime;
        _action = action;
        _tokenSource = new CancellationTokenSource();
    }

    public async void OnValueChanged(T value)
    {
        _curCooltime = _coolTime;
        if (_debounceTask is not null) return;
        _debounceTask = CreateDebounceTask(_tokenSource.Token);
        try
        {
            await _debounceTask;
        }
        catch (TaskCanceledException)
        { 
            Debug.Log("Cancel Task");
        }
    }

    private async Task CreateDebounceTask(CancellationToken token)
    {
        while (_curCooltime > 0)
        {
            await Task.Delay(1000, token);
            _curCooltime--;
        }

        _action?.Invoke();
        _debounceTask = null;
    }

    public void Dispose()
    {
        _tokenSource.Cancel();
        _tokenSource.Dispose();
    }
}

}