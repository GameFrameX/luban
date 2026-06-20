using Luban.CodeTarget;
using Luban.Defs;
using Luban.Tmpl;
using Luban.Utils;
using Scriban;
using Scriban.Runtime;

namespace Luban.Java.CodeTarget;

public abstract class JavaSplitCodeTargetBase : JavaCodeTargetBase
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

    private void RenderBeanDef(GenerationContext ctx, DefBean bean, CodeWriter writer)
    {
        var template = GetDefTemplate("bean.def");
        var tplCtx = CreateTemplateContext(template);
        tplCtx.PushGlobal(CreateBeanContext(ctx, bean));
        writer.Write(template.Render(tplCtx));
    }

    private void RenderTableDef(GenerationContext ctx, DefTable table, CodeWriter writer)
    {
        var template = GetDefTemplate("table.def");
        var tplCtx = CreateTemplateContext(template);
        tplCtx.PushGlobal(CreateTableContext(ctx, table));
        writer.Write(template.Render(tplCtx));
    }

    private void RenderTablesDef(GenerationContext ctx, List<DefTable> tables, CodeWriter writer)
    {
        var template = GetDefTemplate("tables.def");
        var tplCtx = CreateTemplateContext(template);
        tplCtx.PushGlobal(new ScriptObject
        {
            { "__ctx", ctx },
            { "__name", ctx.Target.Manager },
            { "__namespace", ctx.Target.TopModule },
            { "__tables", tables },
            { "__code_style", CodeStyle },
        });
        writer.Write(template.Render(tplCtx));
    }

    private ScriptObject CreateBeanContext(GenerationContext ctx, DefBean bean)
    {
        return new ScriptObject
        {
            { "__ctx", ctx },
            { "__top_module", ctx.Target.TopModule },
            { "__manager_name", ctx.Target.Manager },
            { "__manager_name_with_top_module", TypeUtil.MakeFullName(ctx.TopModule, ctx.Target.Manager) },
            { "__name", bean.Name },
            { "__namespace", bean.Namespace },
            { "__namespace_with_top_module", bean.NamespaceWithTopModule },
            { "__full_name_with_top_module", bean.FullNameWithTopModule },
            { "__bean", bean },
            { "__this", bean },
            { "__export_fields", bean.ExportFields },
            { "__hierarchy_export_fields", bean.HierarchyExportFields },
            { "__parent_def_type", bean.ParentDefType },
            { "__code_style", CodeStyle },
        };
    }

    private ScriptObject CreateTableContext(GenerationContext ctx, DefTable table)
    {
        return new ScriptObject
        {
            { "__ctx", ctx },
            { "__top_module", ctx.Target.TopModule },
            { "__manager_name", ctx.Target.Manager },
            { "__manager_name_with_top_module", TypeUtil.MakeFullName(ctx.TopModule, ctx.Target.Manager) },
            { "__name", table.Name },
            { "__namespace", table.Namespace },
            { "__namespace_with_top_module", table.NamespaceWithTopModule },
            { "__full_name_with_top_module", table.FullNameWithTopModule },
            { "__table", table },
            { "__this", table },
            { "__key_type", table.KeyTType },
            { "__value_type", table.ValueTType },
            { "__code_style", CodeStyle },
        };
    }

    public override void Handle(GenerationContext ctx, OutputFileManifest manifest)
    {
        base.Handle(ctx, manifest);

        var tasks = new List<Task<OutputFile>>();
        tasks.Add(Task.Run(() =>
        {
            var writer = new CodeWriter();
            RenderTablesDef(ctx, ctx.ExportTables, writer);
            return CreateOutputFile($"{GetFileNameWithoutExtByTypeName(ctx.Target.Manager)}Def.{FileSuffixName}", writer.ToResult(FileHeader));
        }));

        foreach (var table in ctx.ExportTables)
        {
            tasks.Add(Task.Run(() =>
            {
                var writer = new CodeWriter();
                RenderTableDef(ctx, table, writer);
                return CreateOutputFile($"{GetFileNameWithoutExtByTypeName(table.FullName)}Def.{FileSuffixName}", writer.ToResult(FileHeader));
            }));
        }

        foreach (var bean in ctx.ExportBeans)
        {
            tasks.Add(Task.Run(() =>
            {
                var writer = new CodeWriter();
                RenderBeanDef(ctx, bean, writer);
                return CreateOutputFile($"{GetFileNameWithoutExtByTypeName(bean.FullName)}Def.{FileSuffixName}", writer.ToResult(FileHeader));
            }));
        }

        Task.WaitAll(tasks.ToArray());
        foreach (var task in tasks)
        {
            manifest.AddFile(task.Result);
        }
    }
}
