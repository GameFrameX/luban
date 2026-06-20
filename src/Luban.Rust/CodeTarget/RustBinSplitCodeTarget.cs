using Luban.CodeTarget;
using Luban.Rust.TemplateExtensions;
using Scriban;

namespace Luban.Rust.CodeTarget;

[CodeTarget("rust-bin-split")]
public class RustBinSplitCodeTarget : RustSplitCodeTargetBase
{
    protected override string RuntimeTemplateDir => "rust-bin";

    protected override string DefTemplateDir => "rust-bin-split";

    protected override void OnCreateTemplateContext(TemplateContext ctx)
    {
        base.OnCreateTemplateContext(ctx);
        ctx.PushGlobal(new RustBinTemplateExtension());
    }
}
