using Luban.Types;
using Luban.TypeVisitors;

namespace Luban.Python.TypeVisitors;

public class DefDeclaringTypeNameVisitor : DecoratorFuncVisitor<string>
{
    public static DefDeclaringTypeNameVisitor Ins { get; } = new();

    public override string DoAccept(TType type)
    {
        return type.IsNullable ? $"Optional[{type.Apply(DefUnderlyingDeclaringTypeNameVisitor.Ins)}]" : type.Apply(DefUnderlyingDeclaringTypeNameVisitor.Ins);
    }
}
