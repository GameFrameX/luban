using Luban.Defs;
using Luban.Rust.TypeVisitors;
using Luban.Types;
using Luban.Utils;
using Scriban.Runtime;

namespace Luban.Rust.TemplateExtensions;

public class RustCommonTemplateExtension : ScriptObject
{
    public static string DeclaringTypeName(TType type)
    {
        return type?.Apply(RustDeclaringBoxTypeNameVisitor.Ins) ?? string.Empty;
    }

    public static string DeclaringDefTypeName(TType type)
    {
        return type?.Apply(RustDefDeclaringBoxTypeNameVisitor.Ins) ?? string.Empty;
    }

    public static string GetterName(string name)
    {
        return "get_" + name;
    }

    public static string FullName(DefTypeBase type)
    {
        return $"crate::{ToRustFullName(type)}";
    }

    public static string FullDefName(DefTypeBase type)
    {
        return $"{FullName(type)}Def";
    }

    public static string BaseTraitName(DefBean bean)
    {
        if (!bean.IsAbstractType) return string.Empty;

        var name = $"crate::{ToRustFullName(bean)}";
        return name.Insert(name.Length - bean.Name.Length, "T");

    }

    public static string ToRustFullName(DefTypeBase type)
    {
        if (string.IsNullOrEmpty(type.Namespace))
        {
            return type.Name;
        }
        return $"{string.Join("::", type.Namespace.Split('.').Select(x => x.ToLowerInvariant()))}::{type.Name}";
    }
}
