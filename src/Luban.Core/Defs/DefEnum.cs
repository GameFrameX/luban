using System.Collections.Concurrent;
using Luban.RawDefs;
using Luban.Utils;
using NLog;

namespace Luban.Defs;

public class DefEnum : DefTypeBase
{
    private static readonly Logger s_logger = LogManager.GetCurrentClassLogger();

    public class Item
    {
        public string Name { get; set; }

        public string Value { get; set; }

        public string Alias { get; set; }

        public string AliasOrName => string.IsNullOrWhiteSpace(Alias) ? Name : Alias;

        public int IntValue { get; set; }

        public string Comment { get; set; }

        public string CommentOrAlias => string.IsNullOrEmpty(Comment) ? Alias : Comment;

        public Dictionary<string, string> Tags { get; set; }

        public bool HasTag(string attrName)
        {
            return Tags != null && Tags.ContainsKey(attrName);
        }

        public string GetTag(string attrName)
        {
            return Tags != null && Tags.TryGetValue(attrName, out var value) ? value : null;
        }
    }

    public bool IsFlags { get; }

    public bool IsUniqueItemId { get; }

    public bool AutoExtend { get; }

    public List<Item> Items { get; } = new();

    private readonly Dictionary<string, int> _nameOrAlias2Value = new();

    private readonly Dictionary<int, string> _vaule2Name = new();

    // autoExtend 收集阶段使用的临时状态。使用并发容器以应对将来可能的并行加载。
    private bool _collecting;

    private readonly ConcurrentDictionary<string, byte> _pendingRawValues = new();

    public bool HasZeroValueItem => this.Items.Any(item => item.IntValue == 0);

    public bool TryValueByNameOrAlias(string name, out int value)
    {
        return _nameOrAlias2Value.TryGetValue(name, out value);
    }

    public int GetValueByNameOrAlias(string name)
    {
        // TODO flags ?
        if (!name.Contains('|'))
        {
            return GetBasicValueByNameOrAlias(name);
        }
        int combindValue = 0;
        foreach (var s in name.Split('|'))
        {
            combindValue |= GetBasicValueByNameOrAlias(s.Trim());
        }
        return combindValue;
    }

    private int GetBasicValueByNameOrAlias(string name)
    {
        if (_nameOrAlias2Value.TryGetValue(name, out var value))
        {
            return value;
        }
        else if (int.TryParse(name, out value))
        {
            if (!_vaule2Name.ContainsKey(value) && !IsFlags)
            {
                if (AutoExtend && _collecting)
                {
                    _pendingRawValues.TryAdd(name, 0);
                    return 0;
                }
                throw new Exception($"{value} 不是 enum:'{FullName}'的有效枚举值");
            }
            return value;
        }
        else
        {
            if (AutoExtend && _collecting)
            {
                _pendingRawValues.TryAdd(name, 0);
                return 0;
            }
            throw new Exception($"'{name}' 不是enum:'{FullName}'的有效枚举值");
        }
    }

    public DefEnum(RawEnum e)
    {
        Name = e.Name;
        Namespace = e.Namespace;
        IsFlags = e.IsFlags;
        IsUniqueItemId = e.IsUniqueItemId;
        AutoExtend = e.AutoExtend;
        Comment = e.Comment;
        Tags = e.Tags;
        Groups = e.Groups;
        TypeMappers = e.TypeMappers is { Count: > 0 } ? e.TypeMappers : null;
        foreach (var item in e.Items)
        {
            Items.Add(new Item
            {
                Name =
                    item.Name,
                Alias = item.Alias,
                Value = item.Value,
                Comment = string.IsNullOrWhiteSpace(item.Comment) ? item.Alias : item.Comment,
                Tags = item.Tags,
            });
        }
    }

    public override void Compile()
    {
        var fullName = FullName;

        int lastEnumValue = -1;
        var names = new HashSet<string>();
        foreach (var item in Items)
        {
            string value = item.Value.ToLower();
            if (!names.Add(item.Name))
            {
                throw new Exception($"enum:'{fullName}' 字段:'{item.Name}' 重复");
            }
            if (string.IsNullOrEmpty(value))
            {
                //  A,
                item.IntValue = ++lastEnumValue;
                item.Value = item.IntValue.ToString();
            }
            else if (int.TryParse(item.Value, out var v))
            {
                //  A = 5,
                item.IntValue = v;
                lastEnumValue = v;
            }
            else if (value.StartsWith("0x"))
            {

                if (int.TryParse(value.Substring(2), System.Globalization.NumberStyles.HexNumber, null, out var x))
                {
                    item.IntValue = x;
                    lastEnumValue = x;
                }
                else
                {
                    throw new Exception($"enum:'{fullName}' 枚举名:'{item.Name}' value:'{item.Value}' 非法");
                }
            }
            else if (IsFlags)
            {
                //  D = A | B | C,
                string[] itemNames = item.Value.Split('|').Select(s => s.Trim()).ToArray();
                foreach (var n in itemNames)
                {
                    var index = Items.FindIndex(i => i.Name == n);
                    if (index < 0)
                    {
                        throw new Exception($"enum:'{fullName}' 枚举名:'{item.Name}' 值:'{item.Value}' 非法");
                    }
                    item.IntValue |= Items[index].IntValue;
                }
            }
            else
            {
                throw new Exception($"enum:'{fullName}' 枚举名:'{item.Name}' value:'{item.Value}' 非法");
            }

            if (!string.IsNullOrWhiteSpace(item.Name) && !_nameOrAlias2Value.TryAdd(item.Name, item.IntValue))
            {
                throw new Exception($"enum:'{fullName}' 枚举名:'{Name}' 重复");
            }

            if (!string.IsNullOrWhiteSpace(item.Alias) && !_nameOrAlias2Value.TryAdd(item.Alias, item.IntValue))
            {
                throw new Exception($"enum:'{fullName}' 枚举名:'{Name}' alias:'{item.Alias}' 重复");
            }
            if (_vaule2Name.TryGetValue(item.IntValue, out var itemName))
            {
                if (IsUniqueItemId)
                {
                    throw new Exception($"enum:'{fullName}' 枚举值:{item.IntValue} 重复. 枚举名:'{itemName}' <=> '{item.Name}'");
                }
            }
            else
            {
                _vaule2Name.Add(item.IntValue, item.Name);
            }
        }


    }

    // 开启 autoExtend 收集阶段。此后 DEnum 构造遇到未定义值时不再抛异常，
    // 而是记录到待处理集合。调用方在扫描完所有相关表数据后须调用 EndAutoExtendCollectAndApply。
    public void BeginAutoExtendCollect()
    {
        if (AutoExtend)
        {
            _collecting = true;
        }
    }

    // 是否处于 autoExtend 收集阶段（供数据加载器判断是否应宽松处理未定义值）
    public bool IsAutoExtendCollecting => AutoExtend && _collecting;

    // 主动记录一个未定义的原始值（用于 excel 字段模式下“列名即枚举项”的场景）
    public void RecordAutoExtendPending(string raw)
    {
        if (AutoExtend && _collecting && !string.IsNullOrEmpty(raw))
        {
            _pendingRawValues.TryAdd(raw, 0);
        }
    }

    // 结束收集阶段，把收集到的未定义值按确定性顺序（按字符串序排序）固化为正式枚举项：
    //   - 数字型原始值（如 "42"）：以该字面值作为枚举值，生成形如 AUTO_VALUE_42 的名字；
    //   - 名字型原始值（如 "RED"）：按递增规则分配新的枚举值。
    // 排序保证不同运行、不同并行加载顺序下产出一致。
    public void EndAutoExtendCollectAndApply()
    {
        if (!AutoExtend)
        {
            return;
        }
        _collecting = false;
        if (_pendingRawValues.IsEmpty)
        {
            return;
        }

        var usedValues = new HashSet<int>(Items.Select(i => i.IntValue));
        var usedNames = new HashSet<string>(Items.Select(i => i.Name), StringComparer.Ordinal);

        var sorted = _pendingRawValues.Keys.OrderBy(s => s, StringComparer.Ordinal).ToList();

        var added = new List<string>();
        int nextValue = ComputeNextAutoValue(usedValues);
        foreach (var raw in sorted)
        {
            if (int.TryParse(raw, out var intVal))
            {
                if (!usedValues.Add(intVal))
                {
                    continue;
                }
                string genName = GenerateAutoItemName(intVal, usedNames);
                AddAutoExtendItem(genName, intVal, raw);
                usedNames.Add(genName);
                added.Add($"{genName}={intVal}");
            }
            else
            {
                if (!usedNames.Add(raw))
                {
                    continue;
                }
                while (usedValues.Contains(nextValue))
                {
                    nextValue = NextAutoValueAfter(nextValue);
                }
                AddAutoExtendItem(raw, nextValue, raw);
                usedValues.Add(nextValue);
                added.Add($"{raw}={nextValue}");
                nextValue = NextAutoValueAfter(nextValue);
            }
        }
        _pendingRawValues.Clear();

        if (added.Count > 0)
        {
            s_logger.Info("enum:'{}' auto-extend 新增 {} 个枚举项: {}", FullName, added.Count, string.Join(", ", added));
            s_logger.Warn("enum:'{}' 的 auto-extend 项其 int 值由当前数据集决定，数据增删可能导致已分配值变化（binary 导出请注意）。", FullName);
        }
    }

    private int ComputeNextAutoValue(HashSet<int> usedValues)
    {
        if (usedValues.Count == 0)
        {
            return IsFlags ? 1 : 0;
        }
        if (IsFlags)
        {
            int max = usedValues.Max();
            int bit = 1;
            while (bit <= max)
            {
                bit <<= 1;
            }
            return bit;
        }
        return usedValues.Max() + 1;
    }

    private int NextAutoValueAfter(int current)
    {
        return IsFlags ? (current << 1) : (current + 1);
    }

    private static string GenerateAutoItemName(int value, HashSet<string> usedNames)
    {
        string baseName = "AUTO_VALUE_" + value;
        string name = baseName;
        int suffix = 1;
        while (usedNames.Contains(name))
        {
            name = baseName + "_" + suffix;
            suffix++;
        }
        return name;
    }

    private void AddAutoExtendItem(string name, int intValue, string comment)
    {
        var item = new Item
        {
            Name = name,
            Alias = "",
            Value = intValue.ToString(),
            IntValue = intValue,
            Comment = comment,
            Tags = new Dictionary<string, string> { ["auto"] = "1" },
        };
        Items.Add(item);
        _nameOrAlias2Value.TryAdd(name, intValue);
        _vaule2Name.TryAdd(intValue, name);
    }

}
