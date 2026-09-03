namespace Luban.L10N;

public interface ITextProvider
{
    void Load();

    /// <summary>
    /// 合并业务表中收集到的内联文本；外部本地化表已有值必须保持优先。
    /// </summary>
    void AddInlineTexts(IReadOnlyList<InlineTextEntry> entries)
    {
    }

    void ProcessDatas();

    bool IsValidKey(string key);

    bool TryGetText(string key, out string text);

    void AddUnknownKey(string key);

    bool ConvertTextKeyToValue { get; }
}
