using Luban.CodeTarget;
using Luban.Rust.TemplateExtensions;
using Scriban;

namespace Luban.Rust.CodeTarget;

[CodeTarget("rust-json-split")]
public class RustJsonSplitCodeTarget : RustSplitCodeTargetBase
{
    protected override string RuntimeTemplateDir => "rust-json";

    protected override string DefTemplateDir => "rust-json-split";

    protected override void OnCreateTemplateContext(TemplateContext ctx)
    {
        base.OnCreateTemplateContext(ctx);
        ctx.PushGlobal(new RustJsonTemplateExtension());
    }
}
