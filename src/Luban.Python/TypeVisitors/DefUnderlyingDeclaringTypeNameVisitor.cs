using Luban.Types;
using Luban.TypeVisitors;

namespace Luban.Python.TypeVisitors;

public class DefUnderlyingDeclaringTypeNameVisitor : ITypeFuncVisitor<string>
{
    public static DefUnderlyingDeclaringTypeNameVisitor Ins { get; } = new();

    public string Accept(TBool type) => "bool";

    public string Accept(TByte type) => "int";

    public string Accept(TShort type) => "int";

    public string Accept(TInt type) => "int";

    public string Accept(TLong type) => "int";

    public string Accept(TFloat type) => "float";

    public string Accept(TDouble type) => "float";

    public string Accept(TEnum type) => "int";

    public string Accept(TString type) => "str";

    public string Accept(TBean type) => $"{type.DefBean.FullName.Replace('.', '_')}Def";

    public string Accept(TArray type) => $"list[{type.ElementType.Apply(this)}]";

    public string Accept(TList type) => $"list[{type.ElementType.Apply(this)}]";

    public string Accept(TSet type) => $"set[{type.ElementType.Apply(this)}]";

    public string Accept(TMap type) => $"dict[{type.KeyType.Apply(this)}, {type.ValueType.Apply(this)}]";

    public string Accept(TDateTime type) => "int";
}
