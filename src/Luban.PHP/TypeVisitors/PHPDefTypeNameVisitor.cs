using Luban.Types;
using Luban.TypeVisitors;

namespace Luban.PHP.TypeVisitors;

public class PHPDefTypeNameVisitor : ITypeFuncVisitor<string>
{
    public static PHPDefTypeNameVisitor Ins { get; } = new();

    public string Accept(TBool type) => "bool";

    public string Accept(TByte type) => "int";

    public string Accept(TShort type) => "int";

    public string Accept(TInt type) => "int";

    public string Accept(TLong type) => "int";

    public string Accept(TFloat type) => "float";

    public string Accept(TDouble type) => "float";

    public string Accept(TEnum type) => "int";

    public string Accept(TString type) => "string";

    public string Accept(TBean type) => $"{type.DefBean.FullName.Replace('.', '_')}Def";

    public string Accept(TArray type) => $"array<int,{type.ElementType.Apply(this)}>";

    public string Accept(TList type) => $"array<int,{type.ElementType.Apply(this)}>";

    public string Accept(TSet type) => $"array<int,{type.ElementType.Apply(this)}>";

    public string Accept(TMap type) => $"array<{type.KeyType.Apply(this)},{type.ValueType.Apply(this)}>";

    public string Accept(TDateTime type) => "int";
}
