using Luban.CodeTarget;
using Luban.Lua.TemplateExtensions;
using Scriban;

namespace Luban.Lua.CodeTarget;

[CodeTarget("lua-bin-split")]
public class LuaBinSplitCodeTarget : LuaSplitCodeTargetBase
{
    protected override string RuntimeTemplateDir => "lua-bin";

    protected override string DefTemplateDir => "lua-bin-split";

    protected override void OnCreateTemplateContext(TemplateContext ctx)
    {
        base.OnCreateTemplateContext(ctx);
        ctx.PushGlobal(new LuaBinTemplateExtension());
    }
}
