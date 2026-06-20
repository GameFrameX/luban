using Luban.CodeTarget;
using Luban.Gdscript.TemplateExtensions;
using Scriban;

namespace Luban.Gdscript.CodeTarget;

[CodeTarget("gdscript-json-split")]
public class GdscriptJsonSplitCodeTarget : GdscriptSplitCodeTargetBase
{
    protected override string RuntimeTemplateDir => "gdscript-json";

    protected override string DefTemplateDir => "gdscript-json-split";

    protected override void OnCreateTemplateContext(TemplateContext ctx)
    {
        base.OnCreateTemplateContext(ctx);
        ctx.PushGlobal(new GdscriptJsonTemplateExtension());
    }
}
