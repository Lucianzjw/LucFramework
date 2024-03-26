using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;


[AttributeUsage(AttributeTargets.Field)]
public class Inject : Attribute
{
}

[AttributeUsage(AttributeTargets.Class)]
public class InjectClass : Attribute
{
}

public class IOCContainer
{
    private static readonly ConcurrentDictionary<Type, Lazy<object>> _managers =
        new ConcurrentDictionary<Type, Lazy<object>>();

    public static int Length => _managers.Count;

    public static T Get<T>() where T : class
    {
        var manager = _managers.GetOrAdd(typeof(T), CreateManagerInstance<T>());
        return manager.Value as T;
    }

    public static void Register<T>(T mgr)
    {
        //Debug.Log($"注册{typeof(T)}");
        var type = typeof(T);
        var lazyInstance = new Lazy<object>(() => mgr);
        _managers.TryAdd(type, lazyInstance);
    }

    public static void RegisterByType(Type type, object mgr)
    {
        //Debug.Log($"注册{type}");
        var lazyInstance = new Lazy<object>(() => mgr);
        _managers.TryAdd(type, lazyInstance);
    }

    public static void Release<T>()
    {
        var type = typeof(T);
        _managers.TryRemove(type, out _);
    }

    public static void Clear()=>_managers.Clear();
    
    private static Lazy<object> CreateManagerInstance<T>() where T : class
    {
        return new Lazy<object>(() => Activator.CreateInstance<T>());
    }

    public static object GetByType(Type type)
    {
        _managers.TryGetValue(type, out var retInstance);
        return retInstance?.Value;
    }

    /// <summary>
    /// 自动注册所有标记过的依赖实例
    /// </summary>
    public static void RegisterDependence()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var types = assembly.GetTypes();
        foreach (var type in types)
        {
            // 检查类是否标注了InjectClass特性
            if (type.IsDefined(typeof(InjectClass), false))
            {

               //Debug.Log(type.Name + "标注了InjectClass特性");
                var aga = Object.FindObjectOfType(type);
                //Debug.Log($"{type}的实例当前存在：{aga.Length}");
                if (aga is null)
                {
                    if (type.IsSubclassOf(typeof(MonoBehaviour)))
                    {
                        //Debug.Log($"继承mono的类：{type.Name}为空");
                        //生成一个新的gameobject 然后挂载这个类
                        Register(new GameObject(type.Name).AddComponent(type));
                        continue;
                    }
                    else
                    {
                        //Debug.Log($"类：{type.Name}为空");
                        //生成一个类的实例
                        var c = Activator.CreateInstance(type);
                        Register(c);
                        continue;
                    }
                }

                //将实例注册到定位器
                RegisterByType(type, aga);
            }
        }
    }

    /// <summary>
    /// 自动解析所有标记过的依赖字段
    /// </summary>
    public static void InjectDependenceIntoField()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var types = assembly.GetTypes();
        foreach (var type in types)
        {
            InjectByType(type);
        }
    }

    public static void InjectByType(Type type)
    {
        // 获取所有字段
        var allFields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        // 遍历所有字段
        foreach (var fieldInfo in allFields)
        {
            //Debug.Log(field.Name);
            // 检查字段是否标注了特性
            if (fieldInfo.IsDefined(typeof(Inject), false))
            {
//                Debug.Log($"{type.Name}的{fieldInfo.Name}需要注入Inject！！！！！！！");
                //injectFields.Add(fieldInfo);
                    
                if (typeof(MonoBehaviour).IsAssignableFrom(type))
                {
                    var aims = Resources.FindObjectsOfTypeAll(type);
                    if (aims is null)
                    {
                        //Debug.LogError($"要设置依赖的目标{type.Name}>>>>空");
                        continue;
                    }

                    //Debug.Log($"要设置依赖的目标{type.Name}>>>>存在");
                    var instance = GetByType(fieldInfo.FieldType);
                    foreach (var aim in aims)
                    {
                        //Debug.Log($"设置 {aim.name} 的依赖{fieldInfo.Name}");
                        fieldInfo.SetValue(aim, instance);
                    }
                }
            }
        }
    }
    
    public static void InjectDependenceIntoSingleClass<T>(T obj) where T: class
    {
        var type = obj.GetType();
        // 获取所有字段
        var allFields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        // 遍历所有字段
        //Debug.Log($"要设置依赖的目标{type.Name}有字段数：{allFields.Length}");
        foreach (var fieldInfo in allFields)
        {
            //Debug.Log(fieldInfo.Name + "    >>>" + fieldInfo.IsDefined(typeof(Inject)));
            // 检查字段是否标注了特性
            if (fieldInfo.IsDefined(typeof(Inject), false))
            {
                //Debug.Log($"{type.Name}的{fieldInfo.Name}需要注入Inject！！！！！！！");
                    
                if (typeof(MonoBehaviour).IsAssignableFrom(type))
                {
                    //Debug.Log($"要设置依赖的目标{type.Name}>>>>存在");
                    var instance = GetByType(fieldInfo.FieldType);
                    fieldInfo.SetValue(obj, instance);
                }
            }
        }
    }
}

public static class RegistrationManager
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void RegisterInjectedClasses()
    {
        IOCContainer.RegisterDependence();
        IOCContainer.InjectDependenceIntoField();
    }
}

public static class InstantiateExtension
{
    public static T InjectInGameObjectWithChildren<T>(this T instance) where T : UnityEngine.Object
    {
        //GameObject gameObject = (obj as Component)?.gameObject;
        if (instance is GameObject gameobject)
        {
            //Debug.Log($"{instance.name}是物体");
            InjectGameObject(gameobject);
        }
        else if (instance is Component component)
        {
            //Debug.Log($"{instance.name}是组件");
            InjectGameObject(component.gameObject);
        }
        return instance; 
    }
    
    public static void InjectGameObject(GameObject gameObject)
    {
        void InjectGameObjectRecursive(GameObject current)
        {
            if (current == null) return;

            using (UnityEngineObjectListBuffer<MonoBehaviour>.Get(out var buffer))
            {
                buffer.Clear();
                current.GetComponents(buffer);
                //ebug.Log(buffer.Count);
                foreach (var monoBehaviour in buffer)
                {
                    if (monoBehaviour != null)
                    { // Can be null if the MonoBehaviour's type wasn't found (e.g. if it was stripped)
                       // Debug.Log($"Inject {monoBehaviour.GetType().Name} in {current.name}");
                        IOCContainer.InjectDependenceIntoSingleClass(monoBehaviour);
                    }
                }
            }

            var transform = current.transform;
            for (var i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                InjectGameObjectRecursive(child.gameObject);
            }
        }

        InjectGameObjectRecursive(gameObject);
    }
}

/// <summary>
/// 解析物体上的monobehavior
/// </summary>
/// <typeparam name="T"></typeparam>
static class UnityEngineObjectListBuffer<T> where T : UnityEngine.Object
{
    const int DefaultCapacity = 32;

    [ThreadStatic] 
    private static Stack<List<T>> _pool = new Stack<List<T>>(4);
        
    /// <summary>
    /// BufferScope supports releasing a buffer with using clause.
    /// </summary>
    public struct BufferScope : IDisposable
    {
        private readonly List<T> _buffer;

        public BufferScope(List<T> buffer)
        {
            _buffer = buffer;
        }
            
        public void Dispose()
        {
            Release(_buffer);
        }
    }

    /// <summary>
    /// Get a buffer from the pool.
    /// </summary>
    /// <returns></returns>
    public static List<T> Get()
    {
        if (_pool.Count == 0)
        {
            return new List<T>(DefaultCapacity);
        }

        return _pool.Pop();
    }

    /// <summary>
    /// Get a buffer from the pool. Returning a disposable struct to support recycling via using clause.
    /// </summary>
    /// <param name="buffer"></param>
    /// <returns></returns>
    public static BufferScope Get(out List<T> buffer)
    {
        buffer = Get();
        return new BufferScope(buffer);
    }

    /// <summary>
    /// Declare a buffer won't be used anymore and put it back to the pool.  
    /// </summary>
    /// <param name="buffer"></param>
    public static void Release(List<T> buffer)
    {
        buffer.Clear();
        _pool.Push(buffer);
    }
}