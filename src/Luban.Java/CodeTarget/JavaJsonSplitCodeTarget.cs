using Luban.CodeTarget;
using Luban.Java.TemplateExtensions;
using Scriban;

namespace Luban.Java.CodeTarget;

[CodeTarget("java-json-split")]
public class JavaJsonSplitCodeTarget : JavaSplitCodeTargetBase
{
    protected override string RuntimeTemplateDir => "java-json";

    protected override string DefTemplateDir => "java-json-split";

    protected override void OnCreateTemplateContext(TemplateContext ctx)
    {
        base.OnCreateTemplateContext(ctx);
        ctx.PushGlobal(new JavaJsonTemplateExtension());
    }
}
