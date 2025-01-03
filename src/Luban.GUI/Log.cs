using System.Collections.Generic;
using System.Text.RegularExpressions;
using Avalonia.Media;
using AvaloniaEdit.Highlighting;

namespace Luban.GUI;

public sealed class LogHighlightingDefinition : IHighlightingDefinition
{
    public HighlightingRuleSet GetNamedRuleSet(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return MainRuleSet;
        }

        return _ruleSetDict.GetValueOrDefault(name);
    }

    public HighlightingColor GetNamedColor(string name)
    {
        return _colorDict.GetValueOrDefault(name);
    }

    public string Name { get; } = "Log";

    public LogHighlightingDefinition(HighlightingManager instance)
    {
        MainRuleSet = new HighlightingRuleSet();
        instance.RegisterHighlighting(Name, null, this);
        var item = new HighlightingRule();
        // item.Regex = @"^\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}";
        item.Color = new HighlightingColor();
        item.Color.Foreground = new SimpleHighlightingBrush(Colors.Red);
        MainRuleSet.Rules.Add(item);
        var span = new HighlightingSpan();
        span.EndColor = new HighlightingColor() { Foreground = new SimpleHighlightingBrush(Colors.Red) };
        span.StartExpression = new Regex("//");
        span.EndExpression = new Regex("$");
        MainRuleSet.Spans.Add(span);
    }

    private readonly Dictionary<string, HighlightingRuleSet> _ruleSetDict = new Dictionary<string, HighlightingRuleSet>();
    private readonly Dictionary<string, HighlightingColor> _colorDict = new Dictionary<string, HighlightingColor>();
    private readonly Dictionary<string, string> _propDict = new Dictionary<string, string>();
    public HighlightingRuleSet MainRuleSet { get; }

    public IEnumerable<HighlightingColor> NamedHighlightingColors
    {
        get
        {
            return _colorDict.Values;
        }
    }

    public IDictionary<string, string> Properties
    {
        get
        {
            return _propDict;
        }
    }
}
