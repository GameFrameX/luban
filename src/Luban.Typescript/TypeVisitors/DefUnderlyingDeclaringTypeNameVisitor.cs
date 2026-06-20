using Luban.Types;

namespace Luban.Typescript.TypeVisitors;

public class DefUnderlyingDeclaringTypeNameVisitor : UnderlyingDeclaringTypeNameVisitor
{
    public new static DefUnderlyingDeclaringTypeNameVisitor Ins { get; } = new();

    public override string Accept(TBean type)
    {
        return $"{type.DefBean.FullName}Def";
    }

    public override string Accept(TArray type)
    {
        return $"{type.ElementType.Apply(this)}[]";
    }

    public override string Accept(TList type)
    {
        return $"{type.ElementType.Apply(this)}[]";
    }

    public override string Accept(TSet type)
    {
        return $"Set<{type.ElementType.Apply(this)}>";
    }

    public override string Accept(TMap type)
    {
        return $"Map<{type.KeyType.Apply(UnderlyingDeclaringTypeNameVisitor.Ins)}, {type.ValueType.Apply(this)}>";
    }
}
