using Luban.CodeTarget;
using Luban.Typescript.TemplateExtensions;
using Scriban;

namespace Luban.Typescript.CodeTarget;

[CodeTarget("typescript-protobuf-split")]
public class TypescriptProtobufSplitCodeTarget : TypescriptSplitCodeTargetBase
{
    protected override string RuntimeTemplateDir => "typescript-protobuf";

    protected override string DefTemplateDir => "typescript-protobuf-split";

    protected override void OnCreateTemplateContext(TemplateContext ctx)
    {
        base.OnCreateTemplateContext(ctx);
        ctx.PushGlobal(new TypescriptBinTemplateExtension());
    }
}
