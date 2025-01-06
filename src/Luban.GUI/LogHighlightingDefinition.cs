using System.Collections.Generic;
using AvaloniaEdit.Highlighting;

namespace Luban.GUI;

public class LogHighlightingDefinition : IHighlightingDefinition
{
    private static readonly Dictionary<string, HighlightingColor> _colors = new()
    {
        ["Error"] = new HighlightingColor { Foreground = new SimpleHighlightingBrush(Avalonia.Media.Color.Parse("#FF0000")) },   // 红色
        ["Warning"] = new HighlightingColor { Foreground = new SimpleHighlightingBrush(Avalonia.Media.Color.Parse("#FFA500")) }, // 黄色
        ["Info"] = new HighlightingColor { Foreground = new SimpleHighlightingBrush(Avalonia.Media.Color.Parse("#000000")) }     // 黑色
    };

    private readonly Dictionary<string, HighlightingRuleSet> _namedRuleSets = new();

    public string Name => "LogHighlighting";

    public HighlightingRuleSet MainRuleSet { get; }

    public LogHighlightingDefinition()
    {
        MainRuleSet = new HighlightingRuleSet
        {
            Rules =
            {
                new HighlightingRule
                {
                    Color = _colors["Error"],
                    Regex = new System.Text.RegularExpressions.Regex(@"^.*(?:ERROR|Error).*$", System.Text.RegularExpressions.RegexOptions.Multiline)
                },
                new HighlightingRule
                {
                    Color = _colors["Warning"],
                    Regex = new System.Text.RegularExpressions.Regex(@"^.*(?:WARN|Warning).*$", System.Text.RegularExpressions.RegexOptions.Multiline)
                },
                new HighlightingRule
                {
                    Color = _colors["Info"],
                    Regex = new System.Text.RegularExpressions.Regex(@"^.*(?:INFO|Info).*$", System.Text.RegularExpressions.RegexOptions.Multiline)
                }
            }
        };
    }

    public HighlightingColor GetNamedColor(string name)
    {
        _colors.TryGetValue(name, out var color);
        return color;
    }

    public HighlightingRuleSet GetNamedRuleSet(string name)
    {
        _namedRuleSets.TryGetValue(name, out var ruleSet);
        return ruleSet;
    }

    public IDictionary<string, string> Properties { get; } = new Dictionary<string, string>();

    public IEnumerable<HighlightingColor> NamedHighlightingColors => _colors.Values;
}
