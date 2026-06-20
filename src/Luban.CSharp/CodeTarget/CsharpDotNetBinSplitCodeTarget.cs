using Luban.CodeTarget;
using Luban.CSharp.TemplateExtensions;
using Scriban;

namespace Luban.CSharp.CodeTarget;

[CodeTarget("cs-dotnet-bin-split")]
public class CsharpDotNetBinSplitCodeTarget : CsharpSplitCodeTargetBase
{
    protected override string DefTemplateDir => "cs-dotnet-bin-split";

    protected override string ImplTemplateDir => "cs-dotnet-bin-split";

    protected override void OnCreateTemplateContext(TemplateContext ctx)
    {
        base.OnCreateTemplateContext(ctx);
        ctx.PushGlobal(new CsharpBinTemplateExtension());
    }
}
