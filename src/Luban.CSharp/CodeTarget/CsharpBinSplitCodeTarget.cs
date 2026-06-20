using Luban.CodeTarget;
using Luban.CSharp.TemplateExtensions;
using Scriban;

namespace Luban.CSharp.CodeTarget;

[CodeTarget("cs-bin-split")]
public class CsharpBinSplitCodeTarget : CsharpSplitCodeTargetBase
{
    protected override string DefTemplateDir => "cs-bin-split";

    protected override string ImplTemplateDir => "cs-bin-split";

    protected override void OnCreateTemplateContext(TemplateContext ctx)
    {
        base.OnCreateTemplateContext(ctx);
        ctx.PushGlobal(new CsharpBinTemplateExtension());
    }
}
