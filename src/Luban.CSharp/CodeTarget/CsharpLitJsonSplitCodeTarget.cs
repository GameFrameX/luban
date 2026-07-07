using Luban.CodeTarget;
using Luban.CSharp.TemplateExtensions;
using Scriban;

namespace Luban.CSharp.CodeTarget;

[CodeTarget("cs-litjson-split")]
public class CsharpLitJsonSplitCodeTarget : CsharpSplitCodeTargetBase
{
    protected override string DefTemplateDir => "cs-litjson-split";

    protected override string ImplTemplateDir => "cs-litjson-split";

    protected override void OnCreateTemplateContext(TemplateContext ctx)
    {
        base.OnCreateTemplateContext(ctx);
        ctx.PushGlobal(new CsharpLitJsonTemplateExtension());
    }
}
