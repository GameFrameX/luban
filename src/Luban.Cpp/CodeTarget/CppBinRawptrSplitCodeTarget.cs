using Luban.CodeTarget;
using Luban.Cpp.TemplateExtensions;
using Scriban;

namespace Luban.Cpp.CodeTarget;

[CodeTarget("cpp-rawptr-bin-split")]
public class CppBinRawptrSplitCodeTarget : CppSplitCodeTargetBase
{
    protected override string DefTemplateDir => "cpp-rawptr-bin-split";

    protected override void OnCreateTemplateContext(TemplateContext ctx)
    {
        base.OnCreateTemplateContext(ctx);
        ctx.PushGlobal(new CppRawptrBinTemplateExtension());
    }
}
