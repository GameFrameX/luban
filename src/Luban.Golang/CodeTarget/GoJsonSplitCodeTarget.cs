using Luban.CodeTarget;
using Luban.Golang.TemplateExtensions;
using Scriban;

namespace Luban.Golang.CodeTarget;

[CodeTarget("go-json-split")]
public class GoJsonSplitCodeTarget : GoSplitCodeTargetBase
{
    protected override string DefTemplateDir => "go-json-split";

    protected override string ImplTemplateDir => "go-json-split";

    protected override void OnCreateTemplateContext(TemplateContext ctx)
    {
        base.OnCreateTemplateContext(ctx);
        ctx.PushGlobal(new GoJsonTemplateExtension());
    }
}
