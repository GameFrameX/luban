using Luban.CodeTarget;
using Luban.Cpp.TemplateExtensions;
using Scriban;

namespace Luban.Cpp.CodeTarget;

[CodeTarget("cpp-sharedptr-bin-split")]
public class CppBinSharedptrSplitCodeTarget : CppSplitCodeTargetBase
{
    protected override string DefTemplateDir => "cpp-sharedptr-bin-split";

    protected override void OnCreateTemplateContext(TemplateContext ctx)
    {
        base.OnCreateTemplateContext(ctx);
        ctx.PushGlobal(new CppSharedptrBinTemplateExtension());
    }
}
