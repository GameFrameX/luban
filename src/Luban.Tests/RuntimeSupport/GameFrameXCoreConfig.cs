// GameFrameX 运行时 shim（GameFrameX.Core.Config）：fork 模板生成的代码引用该命名空间下的
// ConfigComponent / BeanBase / BaseDataTable<T> / StringUtil；官方集成测试 harness 中以最小实现代替外部运行时。

namespace GameFrameX.Core.Config
{
    public class ConfigComponent
    {
        private readonly System.Collections.Generic.Dictionary<string, object> _configs = new();

        public void Add(string name, object config)
        {
            _configs[name] = config;
        }

        public void RemoveAllConfigs()
        {
            _configs.Clear();
        }
    }

    public abstract class BeanBase
    {
        public virtual int GetTypeId() => 0;
    }

    public abstract class BaseDataTable<T>
    {
        public System.Collections.Generic.List<T> DataList { get; } = new();

        public System.Collections.Generic.Dictionary<long, T> LongDataMaps { get; } = new();

        public System.Collections.Generic.Dictionary<string, T> StringDataMaps { get; } = new();

        public bool TryGet(long key, out T value)
        {
            return LongDataMaps.TryGetValue(key, out value);
        }

        public bool TryGet(string key, out T value)
        {
            return StringDataMaps.TryGetValue(key, out value);
        }

        public T GetOrDefault(long key)
        {
            return LongDataMaps.TryGetValue(key, out var value) ? value : default;
        }

        public T GetOrDefault(string key)
        {
            return StringDataMaps.TryGetValue(key, out var value) ? value : default;
        }

        public T Get(long key)
        {
            return LongDataMaps[key];
        }

        public T Get(string key)
        {
            return StringDataMaps[key];
        }

        public virtual System.Threading.Tasks.Task LoadAsync()
        {
            return System.Threading.Tasks.Task.CompletedTask;
        }
    }

    public class SerializationException : System.Exception
    {
        public SerializationException() { }
        public SerializationException(string message) : base(message) { }
    }

    public static class StringUtil
    {
        public static string CollectionToString<T>(System.Collections.Generic.IEnumerable<T> collection)
        {
            return global::Luban.StringUtil.CollectionToString(collection);
        }
    }
}
