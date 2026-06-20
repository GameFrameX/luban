using Luban.CodeTarget;
using Luban.PHP.TemplateExtensions;
using Scriban;

namespace Luban.PHP.CodeTarget;

[CodeTarget("php-json-split")]
public class PHPJsonSplitCodeTarget : PHPSplitCodeTargetBase
{
    protected override string RuntimeTemplateDir => "php-json";

    protected override string DefTemplateDir => "php-json-split";

    protected override void OnCreateTemplateContext(TemplateContext ctx)
    {
        base.OnCreateTemplateContext(ctx);
        ctx.PushGlobal(new PHPJsonTemplateExtension());
    }
}
