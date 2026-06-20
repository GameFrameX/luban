using Luban.Types;

namespace Luban.Java.TypeVisitors;

public class JavaDefDeclaringBoxTypeNameVisitor : JavaDefDeclaringTypeNameVisitor
{
    public new static JavaDefDeclaringBoxTypeNameVisitor Ins { get; } = new();

    public override string Accept(TBool type)
    {
        return "Boolean";
    }

    public override string Accept(TByte type)
    {
        return "Byte";
    }

    public override string Accept(TShort type)
    {
        return "Short";
    }

    public override string Accept(TInt type)
    {
        return "Integer";
    }

    public override string Accept(TLong type)
    {
        return "Long";
    }

    public override string Accept(TFloat type)
    {
        return "Float";
    }

    public override string Accept(TDouble type)
    {
        return "Double";
    }

    public override string Accept(TDateTime type)
    {
        return "Long";
    }

    public override string Accept(TEnum type)
    {
        return "Integer";
    }
}
