using Luban.Rust.TemplateExtensions;
using Luban.Types;

namespace Luban.Rust.TypeVisitors;

public class RustDefDeclaringTypeNameVisitor : RustDeclaringTypeNameVisitor
{
    public new static readonly RustDefDeclaringTypeNameVisitor Ins = new();

    public override string Accept(TBean type)
    {
        return type.DefBean.IsAbstractType
            ? "std::sync::Arc<AbstractBase>"
            : RustCommonTemplateExtension.FullDefName(type.DefBean);
    }

    public override string Accept(TArray type)
    {
        return $"Vec<{type.ElementType.Apply(this)}>";
    }

    public override string Accept(TList type)
    {
        return $"Vec<{type.ElementType.Apply(this)}>";
    }

    public override string Accept(TSet type)
    {
        return $"std::collections::HashSet<{type.ElementType.Apply(this)}>";
    }

    public override string Accept(TMap type)
    {
        return $"std::collections::HashMap<{type.KeyType.Apply(RustDeclaringTypeNameVisitor.Ins)}, {type.ValueType.Apply(this)}>";
    }
}
