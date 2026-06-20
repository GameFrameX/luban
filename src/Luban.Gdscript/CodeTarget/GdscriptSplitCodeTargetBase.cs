using Luban.CodeTarget;
using Luban.Defs;
using Luban.Tmpl;
using Luban.Utils;
using Scriban;
using Scriban.Runtime;

namespace Luban.Gdscript.CodeTarget;

public abstract class GdscriptSplitCodeTargetBase : GdscriptCodeTargetBase
{
    protected abstract string RuntimeTemplateDir { get; }

    protected abstract string DefTemplateDir { get; }

    protected override string TemplateDir => RuntimeTemplateDir;

    private Template GetDefTemplate(string name)
    {
        if (TemplateManager.Ins.TryGetTemplate($"{DefTemplateDir}/{name}", out var template))
        {
            return template;
        }
        throw new Exception($"def template:{name} not found in {DefTemplateDir}/");
    }

    private void RenderDef(Template template, ScriptObject context, CodeWriter writer)
    {
        var tplCtx = CreateTemplateContext(template);
        tplCtx.PushGlobal(context);
        writer.Write(template.Render(tplCtx));
    }

    public override void Handle(GenerationContext ctx, OutputFileManifest manifest)
    {
        base.Handle(ctx, manifest);

        var tasks = new List<Task<OutputFile>>
        {
            Task.Run(() =>
            {
                var writer = new CodeWriter();
                RenderDef(GetDefTemplate("tables.def"), new ScriptObject
                {
                    { "__ctx", ctx },
                    { "__name", ctx.Target.Manager },
                    { "__namespace", ctx.Target.TopModule },
                    { "__full_name", TypeUtil.MakeFullName(ctx.TopModule, ctx.Target.Manager) },
                    { "__tables", ctx.ExportTables },
                    { "__code_style", CodeStyle },
                }, writer);
                return CreateOutputFile($"{ctx.Target.Manager}.def.gd", writer.ToResult(FileHeader));
            })
        };

        foreach (var table in ctx.ExportTables)
        {
            tasks.Add(Task.Run(() =>
            {
                var writer = new CodeWriter();
                RenderDef(GetDefTemplate("table.def"), new ScriptObject
                {
                    { "__ctx", ctx },
                    { "__name", table.Name },
                    { "__namespace", table.Namespace },
                    { "__full_name", table.FullName },
                    { "__table", table },
                    { "__this", table },
                    { "__key_type", table.KeyTType },
                    { "__value_type", table.ValueTType },
                    { "__code_style", CodeStyle },
                }, writer);
                return CreateOutputFile($"{table.FullName}.def.gd", writer.ToResult(FileHeader));
            }));
        }

        foreach (var bean in ctx.ExportBeans)
        {
            tasks.Add(Task.Run(() =>
            {
                var writer = new CodeWriter();
                RenderDef(GetDefTemplate("bean.def"), new ScriptObject
                {
                    { "__ctx", ctx },
                    { "__name", bean.Name },
                    { "__namespace", bean.Namespace },
                    { "__full_name", bean.FullName },
                    { "__bean", bean },
                    { "__this", bean },
                    { "__hierarchy_export_fields", bean.HierarchyExportFields },
                    { "__parent_def_type", bean.ParentDefType },
                    { "__code_style", CodeStyle },
                }, writer);
                return CreateOutputFile($"{bean.FullName}.def.gd", writer.ToResult(FileHeader));
            }));
        }

        Task.WaitAll(tasks.ToArray());
        foreach (var task in tasks)
        {
            manifest.AddFile(task.Result);
        }
    }
}
