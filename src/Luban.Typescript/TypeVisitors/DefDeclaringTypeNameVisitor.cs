using Luban.Types;
using Luban.TypeVisitors;

namespace Luban.Typescript.TypeVisitors;

public class DefDeclaringTypeNameVisitor : DecoratorFuncVisitor<string>
{
    public static DefDeclaringTypeNameVisitor Ins { get; } = new();

    public override string DoAccept(TType type)
    {
        return type.IsNullable ? $"{type.Apply(DefUnderlyingDeclaringTypeNameVisitor.Ins)}|undefined" : type.Apply(DefUnderlyingDeclaringTypeNameVisitor.Ins);
    }
}
