using Luban.CodeTarget;
using Luban.CSharp.TemplateExtensions;
using Scriban;

namespace Luban.CSharp.CodeTarget;

[CodeTarget("cs-simple-json-split")]
public class CsharpSimpleJsonSplitCodeTarget : CsharpSplitCodeTargetBase
{
    protected override string DefTemplateDir => "cs-simple-json-split";

    protected override string ImplTemplateDir => "cs-simple-json-split";

    protected override void OnCreateTemplateContext(TemplateContext ctx)
    {
        base.OnCreateTemplateContext(ctx);
        ctx.PushGlobal(new CsharpSimpleJsonTemplateExtension());
    }
}
