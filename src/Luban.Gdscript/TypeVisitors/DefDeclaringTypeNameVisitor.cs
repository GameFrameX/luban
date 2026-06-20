using Luban.Gdscript.TemplateExtensions;
using Luban.Types;
using Luban.TypeVisitors;

namespace Luban.Gdscript.TypeVisitors;

public class DefDeclaringTypeNameVisitor : ITypeFuncVisitor<string>
{
    public static DefDeclaringTypeNameVisitor Ins { get; } = new();

    public string Accept(TBool type) => "bool";

    public string Accept(TByte type) => "int";

    public string Accept(TShort type) => "int";

    public string Accept(TInt type) => "int";

    public string Accept(TLong type) => "int";

    public string Accept(TFloat type) => "float";

    public string Accept(TDouble type) => "float";

    public string Accept(TEnum type) => "int";

    public string Accept(TString type) => "String";

    public string Accept(TBean type) => $"{GdscriptCommonTemplateExtension.FullName(type.DefBean)}Def";

    public string Accept(TArray type) => $"Array[{type.ElementType.Apply(this)}]";

    public string Accept(TList type) => $"Array[{type.ElementType.Apply(this)}]";

    public string Accept(TSet type) => $"Array[{type.ElementType.Apply(this)}]";

    public string Accept(TMap type) => "Dictionary";

    public string Accept(TDateTime type) => "int";
}
