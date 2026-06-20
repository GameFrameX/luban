using Luban.CodeTarget;
using Luban.Golang.TemplateExtensions;
using Scriban;

namespace Luban.Golang.CodeTarget;

[CodeTarget("go-bin-split")]
public class GoBinSplitCodeTarget : GoSplitCodeTargetBase
{
    protected override string DefTemplateDir => "go-bin-split";

    protected override string ImplTemplateDir => "go-bin-split";

    protected override void OnCreateTemplateContext(TemplateContext ctx)
    {
        base.OnCreateTemplateContext(ctx);
        ctx.PushGlobal(new GoBinTemplateExtension());
    }
}
