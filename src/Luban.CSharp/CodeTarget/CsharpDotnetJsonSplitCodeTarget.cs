using Luban.CodeTarget;
using Luban.CSharp.TemplateExtensions;
using Scriban;

namespace Luban.CSharp.CodeTarget;

[CodeTarget("cs-dotnet-json-split")]
public class CsharpDotnetJsonSplitCodeTarget : CsharpSplitCodeTargetBase
{
    protected override string DefTemplateDir => "cs-dotnet-json-split";

    protected override string ImplTemplateDir => "cs-dotnet-json-split";

    protected override void OnCreateTemplateContext(TemplateContext ctx)
    {
        base.OnCreateTemplateContext(ctx);
        ctx.PushGlobal(new CsharpDotNetJsonTemplateExtension());
    }
}
