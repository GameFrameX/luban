namespace Luban;

public class EnvManager
{
    public static EnvManager Current { get; set; }

    private readonly Dictionary<string, List<string>> _options;

    public EnvManager(Dictionary<string, string> options)
    {
        _options = new Dictionary<string, List<string>>(options.Count);
        foreach (var kv in options)
        {
            _options[kv.Key] = new List<string> { kv.Value };
        }
    }

    public EnvManager(Dictionary<string, List<string>> options)
    {
        _options = options;
    }

    public bool HasOptionRaw(string optionName)
    {
        return _options.ContainsKey(optionName);
    }

    public string GetOptionRaw(string optionName)
    {
        return _options.TryGetValue(optionName, out var values) && values.Count > 0 ? values[0] : null;
    }

    public string GetOptionOrDefaultRaw(string optionName, string defaultValue)
    {
        return _options.TryGetValue(optionName, out var values) && values.Count > 0 ? values[0] : defaultValue;
    }


    public string GetOption(string namespaze, string name, bool useGlobalIfNotExits)
    {
        return TryGetOption(namespaze, name, useGlobalIfNotExits, out var value) ? value : throw new Exception($"option '{name}' not exists");
    }

    public bool TryGetOption(string namespaze, string name, bool useGlobalIfNotExits, out string value)
    {
        while (true)
        {
            string fullOptionName = string.IsNullOrEmpty(namespaze) ? name : namespaze + "." + name;
            if (_options.TryGetValue(fullOptionName, out var values) && values.Count > 0)
            {
                value = values[0];
                return true;
            }

            if (string.IsNullOrEmpty(namespaze) || !useGlobalIfNotExits)
            {
                value = null;
                return false;
            }

            int index = namespaze.LastIndexOf('.');
            if (index < 0)
            {
                namespaze = "";
            }
            else
            {
                namespaze = namespaze.Substring(0, index);
            }
        }
    }

    public string GetOptionOrDefault(string namespaze, string name, bool useGlobalIfNotExits, string defaultValue)
    {
        return TryGetOption(namespaze, name, useGlobalIfNotExits, out string value) ? value : defaultValue;
    }

    public bool GetBoolOptionOrDefault(string namespaze, string name, bool useGlobalIfNotExits, bool defaultValue)
    {
        if (TryGetOption(namespaze, name, useGlobalIfNotExits, out string value))
        {
            switch (value.ToLowerInvariant())
            {
                case "0":
                case "false":
                    return false;
                case "1":
                case "true":
                    return true;
                default:
                    throw new Exception($"invalid bool option value:{value}");
            }
        }
        return defaultValue;
    }

    /// <summary>
    /// 同名 option 的所有值，按命令行出现顺序。Option 缺失时返回空数组。
    /// </summary>
    public IReadOnlyList<string> GetOptionList(string namespaze, string name, bool useGlobalIfNotExits)
    {
        while (true)
        {
            string fullOptionName = string.IsNullOrEmpty(namespaze) ? name : namespaze + "." + name;
            if (_options.TryGetValue(fullOptionName, out var values))
            {
                return values;
            }

            if (string.IsNullOrEmpty(namespaze) || !useGlobalIfNotExits)
            {
                return Array.Empty<string>();
            }

            int index = namespaze.LastIndexOf('.');
            if (index < 0)
            {
                namespaze = "";
            }
            else
            {
                namespaze = namespaze.Substring(0, index);
            }
        }
    }

}
