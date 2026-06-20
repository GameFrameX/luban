using Luban.CodeTarget;
using Luban.Typescript.TemplateExtensions;
using Scriban;

namespace Luban.Typescript.CodeTarget;

[CodeTarget("typescript-bin-split")]
public class TypescriptBinSplitCodeTarget : TypescriptSplitCodeTargetBase
{
    protected override string RuntimeTemplateDir => "typescript-bin";

    protected override string DefTemplateDir => "typescript-bin-split";

    protected override void OnCreateTemplateContext(TemplateContext ctx)
    {
        base.OnCreateTemplateContext(ctx);
        ctx.PushGlobal(new TypescriptBinTemplateExtension());
    }
}
