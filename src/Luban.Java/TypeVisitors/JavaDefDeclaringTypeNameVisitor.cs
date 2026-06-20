using Luban.Types;
using Luban.Utils;

namespace Luban.Java.TypeVisitors;

public class JavaDefDeclaringTypeNameVisitor : JavaDeclaringTypeNameVisitor
{
    public new static JavaDefDeclaringTypeNameVisitor Ins { get; } = new();

    public override string Accept(TBean type)
    {
        return type.DefBean.TypeNameWithTypeMapper() ?? $"{type.DefBean.FullNameWithTopModule}Def";
    }

    public override string Accept(TArray type)
    {
        return $"{type.ElementType.Apply(this)}[]";
    }

    public override string Accept(TList type)
    {
        return $"java.util.ArrayList<{type.ElementType.Apply(JavaDefDeclaringBoxTypeNameVisitor.Ins)}>";
    }

    public override string Accept(TSet type)
    {
        return $"java.util.HashSet<{type.ElementType.Apply(JavaDefDeclaringBoxTypeNameVisitor.Ins)}>";
    }

    public override string Accept(TMap type)
    {
        return $"java.util.HashMap<{type.KeyType.Apply(JavaDeclaringBoxTypeNameVisitor.Ins)}, {type.ValueType.Apply(JavaDefDeclaringBoxTypeNameVisitor.Ins)}>";
    }
}
