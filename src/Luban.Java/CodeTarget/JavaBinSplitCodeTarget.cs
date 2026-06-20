using Luban.CodeTarget;
using Luban.Java.TemplateExtensions;
using Scriban;

namespace Luban.Java.CodeTarget;

[CodeTarget("java-bin-split")]
public class JavaBinSplitCodeTarget : JavaSplitCodeTargetBase
{
    protected override string RuntimeTemplateDir => "java-bin";

    protected override string DefTemplateDir => "java-bin-split";

    protected override void OnCreateTemplateContext(TemplateContext ctx)
    {
        base.OnCreateTemplateContext(ctx);
        ctx.PushGlobal(new JavaBinTemplateExtension());
    }
}
