using Luban.CSharp.TypeVisitors;
using Luban.Types;
using Scriban.Runtime;

namespace Luban.CSharp.TemplateExtensions;

public class CsharpLitJsonTemplateExtension : ScriptObject
{
    public static string Deserialize(string fieldName, string jsonVar, TType type)
    {
        if (type.IsNullable)
        {
            return $"{{var _j = {jsonVar}; if (_j != null) {{ {type.Apply(LitJsonDeserializeVisitor.Ins, "_j", fieldName, 0)} }} else {{ {fieldName} = null; }} }}";
        }
        else
        {
            return type.Apply(LitJsonDeserializeVisitor.Ins, jsonVar, fieldName, 0);
        }
    }

    public static string DeserializeField(string fieldName, string jsonVar, string jsonFieldName, TType type)
    {
        if (type.IsNullable)
        {
            return $"{{ var _j = {jsonVar}.ContainsKey(\"{jsonFieldName}\") ? {jsonVar}[\"{jsonFieldName}\"] : null; if (_j != null) {{ {type.Apply(LitJsonDeserializeVisitor.Ins, "_j", fieldName, 0)} }} else {{ {fieldName} = null; }} }}";
        }
        else
        {
            return type.Apply(LitJsonDeserializeVisitor.Ins, $"{jsonVar}[\"{jsonFieldName}\"]", fieldName, 0);
        }
    }
}
