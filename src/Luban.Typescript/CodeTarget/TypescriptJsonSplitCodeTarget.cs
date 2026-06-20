using Luban.CodeTarget;
using Luban.Typescript.TemplateExtensions;
using Scriban;

namespace Luban.Typescript.CodeTarget;

[CodeTarget("typescript-json-split")]
public class TypescriptJsonSplitCodeTarget : TypescriptSplitCodeTargetBase
{
    protected override string RuntimeTemplateDir => "typescript-json";

    protected override string DefTemplateDir => "typescript-json-split";

    protected override void OnCreateTemplateContext(TemplateContext ctx)
    {
        base.OnCreateTemplateContext(ctx);
        ctx.PushGlobal(new TypescriptJsonTemplateExtension());
    }
}
