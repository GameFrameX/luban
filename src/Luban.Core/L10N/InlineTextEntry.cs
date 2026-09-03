using Luban.Datas;
using Luban.Defs;
using Luban.Types;

namespace Luban.L10N;

/// <summary>
/// 一条来自业务配置表的内联本地化映射。
/// </summary>
public sealed record InlineTextEntry(string TableName, string Source, string FieldPath, string Key, string Value);

/// <summary>
/// 保存本次导表发现的内联本地化映射，并按业务表分组。
/// </summary>
public sealed class InlineTextCollection
{
    private readonly Dictionary<string, List<InlineTextEntry>> _entriesByTable = new();
    private readonly Dictionary<string, InlineTextEntry> _entriesByKey = new();

    /// <summary>
    /// 获取去重后的全部内联映射。
    /// </summary>
    public IReadOnlyList<InlineTextEntry> Entries => _entriesByTable.Values.SelectMany(x => x).ToList();

    /// <summary>
    /// 获取按完整业务表名分组的内联映射。
    /// </summary>
    public IReadOnlyDictionary<string, List<InlineTextEntry>> EntriesByTable => _entriesByTable;

    /// <summary>
    /// 添加一条映射；同一 key 的不同 value 会立即报告冲突。
    /// </summary>
    /// <param name="tableName">业务表完整名称。</param>
    /// <param name="source">来源文件或工作表。</param>
    /// <param name="fieldPath">记录中的字段路径。</param>
    /// <param name="key">文本 key。</param>
    /// <param name="value">当前命令行指定语言的文本 value。</param>
    public void Add(string tableName, string source, string fieldPath, string key, string value)
    {
        if (string.IsNullOrEmpty(key))
        {
            return;
        }

        if (_entriesByKey.TryGetValue(key, out var globalExisting) && globalExisting.Value != value)
        {
            throw new Exception($"内联本地化 key:'{key}' 在不同表中存在不同 value。文件1:{globalExisting.Source} 字段:{globalExisting.FieldPath} 文件2:{source} 字段:{fieldPath}");
        }

        if (!_entriesByTable.TryGetValue(tableName, out var entries))
        {
            entries = new List<InlineTextEntry>();
            _entriesByTable.Add(tableName, entries);
        }

        var existing = entries.FirstOrDefault(x => x.Key == key);
        if (existing != null)
        {
            if (existing.Value != value)
            {
                throw new Exception($"内联本地化 key:'{key}' 在表:'{tableName}' 中存在不同 value。文件1:{existing.Source} 字段:{existing.FieldPath} 文件2:{source} 字段:{fieldPath}");
            }
            return;
        }

        entries.Add(new InlineTextEntry(tableName, source, fieldPath, key, value));
        _entriesByKey.TryAdd(key, entries[^1]);
    }
}

public static class InlineTextMerger
{
    /// <summary>
    /// 将内联文本加入现有字典；已存在的外部本地化值保持不变。
    /// </summary>
    /// <param name="texts">已加载的本地化字典。</param>
    /// <param name="entries">当前导表批次收集到的内联文本。</param>
    /// <param name="logger">用于记录外部值覆盖内联值的警告日志。</param>
    public static void Merge(Dictionary<string, string> texts, IReadOnlyList<InlineTextEntry> entries, NLog.Logger logger)
    {
        foreach (var entry in entries)
        {
            if (texts.TryGetValue(entry.Key, out var externalValue))
            {
                if (externalValue != entry.Value)
                {
                    logger.Warn($"外部本地化值优先，忽略冲突的内联值。key:{entry.Key} table:{entry.TableName} source:{entry.Source}");
                }
                continue;
            }
            texts.Add(entry.Key, entry.Value);
        }
    }
}

internal sealed class InlineTextCollector
{
    private readonly InlineTextCollection _collection;
    private readonly string _tableName;
    private readonly string _source;

    public InlineTextCollector(InlineTextCollection collection, string tableName, string source)
    {
        _collection = collection;
        _tableName = tableName;
        _source = source;
    }

    public void Add(string fieldPath, string key, string value) => _collection.Add(_tableName, _source, fieldPath, key, value);

    public static InlineTextCollection Collect(GenerationContext context)
    {
        var collection = new InlineTextCollection();
        foreach (var table in context.Tables)
        {
            foreach (var record in context.GetTableAllDataList(table))
            {
                var collector = new InlineTextCollector(collection, table.FullName, record.Source);
                CollectData(record.Data, table.ValueTType, collector, "");
            }
        }
        return collection;
    }

    private static void CollectData(DType data, TType type, InlineTextCollector collector, string fieldPath)
    {
        if (data == null)
        {
            return;
        }
        if (data is DString text && type.HasTag("text"))
        {
            if (text.InlineValue != null)
            {
                collector.Add(fieldPath, text.Value, text.InlineValue);
            }
            return;
        }
        if (data is not DBean bean)
        {
            return;
        }

        var fields = bean.ImplType.HierarchyFields;
        for (int i = 0; i < bean.Fields.Count; i++)
        {
            if (bean.Fields[i] == null)
            {
                continue;
            }
            string childPath = string.IsNullOrEmpty(fieldPath) ? fields[i].Name : $"{fieldPath}.{fields[i].Name}";
            CollectData(bean.Fields[i], fields[i].CType, collector, childPath);
        }
    }
}
