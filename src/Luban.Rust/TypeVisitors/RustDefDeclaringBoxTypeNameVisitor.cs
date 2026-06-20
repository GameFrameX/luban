using Luban.Types;
using Luban.TypeVisitors;

namespace Luban.Rust.TypeVisitors;

public class RustDefDeclaringBoxTypeNameVisitor : DecoratorFuncVisitor<string>
{
    public static readonly RustDefDeclaringBoxTypeNameVisitor Ins = new();

    public override string DoAccept(TType type)
    {
        var origin = type.Apply(RustDefDeclaringTypeNameVisitor.Ins);
        return type.IsNullable ? $"Option<{origin}>" : origin;
    }
}
