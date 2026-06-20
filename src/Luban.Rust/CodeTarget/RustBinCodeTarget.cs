using Luban.CodeTarget;
using Luban.Rust.TemplateExtensions;
using Scriban;

namespace Luban.Rust.CodeTarget;

[CodeTarget("rust-bin")]
public class RustBinCodeTarget : RustCodeTargetBase
{
    protected override void OnCreateTemplateContext(TemplateContext ctx)
    {
        base.OnCreateTemplateContext(ctx);
        ctx.PushGlobal(new RustBinTemplateExtension());
    }
}
