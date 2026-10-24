// GameFrameX 运行时 shim（GameFrameX.Config / GameFrameX.Config.Runtime）：
// fork 的 cs-bin / cs-simple-json / cs-litjson 模板生成代码引用 GameFrameX.Config.Runtime 下的
// ConfigComponent / BeanBase / BaseDataTable<T>；cs-dotnet-json 生成代码同时 using
// GameFrameX.Config 与 GameFrameX.Core.Config，因此该两个命名空间中的同名类型不得重复定义。

namespace GameFrameX.Config
{
    internal static class GameFrameXConfigNamespaceMarker
    {
    }

    public static class StringUtil
    {
        public static string CollectionToString<T>(System.Collections.Generic.IEnumerable<T> collection)
        {
            return global::Luban.StringUtil.CollectionToString(collection);
        }
    }
}

namespace GameFrameX.Config.Runtime
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
}
