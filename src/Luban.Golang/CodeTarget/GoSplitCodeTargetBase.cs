using Luban.CodeTarget;
using Luban.Defs;
using Luban.Tmpl;
using Luban.Utils;
using Scriban;
using Scriban.Runtime;

namespace Luban.Golang.CodeTarget;

public abstract class GoSplitCodeTargetBase : GoCodeTargetBase
{
    protected abstract string DefTemplateDir { get; }

    protected abstract string ImplTemplateDir { get; }

    private Template GetDefTemplate(string name)
    {
        if (TemplateManager.Ins.TryGetTemplate($"{DefTemplateDir}/{name}", out var template))
        {
            return template;
        }
        throw new Exception($"def template:{name} not found in {DefTemplateDir}/");
    }

    private Template GetImplTemplate(string name)
    {
        if (TemplateManager.Ins.TryGetTemplate($"{ImplTemplateDir}/{name}", out var template))
        {
            return template;
        }
        throw new Exception($"impl template:{name} not found in {ImplTemplateDir}/");
    }

    private void RenderBeanDef(GenerationContext ctx, DefBean bean, CodeWriter writer)
    {
        var template = GetDefTemplate("bean.def");
        var tplCtx = CreateTemplateContext(template);
        tplCtx.PushGlobal(CreateBeanContext(ctx, bean));
        writer.Write(template.Render(tplCtx));
    }

    private void RenderBeanImpl(GenerationContext ctx, DefBean bean, CodeWriter writer)
    {
        var template = GetImplTemplate("bean.impl");
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

    private void RenderTableImpl(GenerationContext ctx, DefTable table, CodeWriter writer)
    {
        var template = GetImplTemplate("table.impl");
        var tplCtx = CreateTemplateContext(template);
        tplCtx.PushGlobal(CreateTableContext(ctx, table));
        writer.Write(template.Render(tplCtx));
    }

    private void RenderTablesDef(GenerationContext ctx, List<DefTable> tables, CodeWriter writer)
    {
        var template = GetDefTemplate("tables.def");
        var tplCtx = CreateTemplateContext(template);
        tplCtx.PushGlobal(CreateTablesContext(ctx, tables));
        writer.Write(template.Render(tplCtx));
    }

    private void RenderTablesImpl(GenerationContext ctx, List<DefTable> tables, CodeWriter writer)
    {
        var template = GetImplTemplate("tables.impl");
        var tplCtx = CreateTemplateContext(template);
        tplCtx.PushGlobal(CreateTablesContext(ctx, tables));
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

    private ScriptObject CreateTablesContext(GenerationContext ctx, List<DefTable> tables)
    {
        return new ScriptObject
        {
            { "__ctx", ctx },
            { "__name", ctx.Target.Manager },
            { "__namespace", ctx.Target.TopModule },
            { "__tables", tables },
            { "__code_style", CodeStyle },
        };
    }

    public override void Handle(GenerationContext ctx, OutputFileManifest manifest)
    {
        var tasks = new List<Task<OutputFile>>();

        tasks.Add(Task.Run(() =>
        {
            var writer = new CodeWriter();
            RenderTablesDef(ctx, ctx.ExportTables, writer);
            return CreateOutputFile($"{GetFileNameWithoutExtByTypeName(ctx.Target.Manager)}.def.{FileSuffixName}", writer.ToResult(FileHeader));
        }));
        tasks.Add(Task.Run(() =>
        {
            var writer = new CodeWriter();
            RenderTablesImpl(ctx, ctx.ExportTables, writer);
            return CreateOutputFile($"{GetFileNameWithoutExtByTypeName(ctx.Target.Manager)}.impl.{FileSuffixName}", writer.ToResult(FileHeader));
        }));

        foreach (var table in ctx.ExportTables)
        {
            tasks.Add(Task.Run(() =>
            {
                var writer = new CodeWriter();
                RenderTableDef(ctx, table, writer);
                return CreateOutputFile($"{GetFileNameWithoutExtByTypeName(table.FullName)}.def.{FileSuffixName}", writer.ToResult(FileHeader));
            }));
            tasks.Add(Task.Run(() =>
            {
                var writer = new CodeWriter();
                RenderTableImpl(ctx, table, writer);
                return CreateOutputFile($"{GetFileNameWithoutExtByTypeName(table.FullName)}.impl.{FileSuffixName}", writer.ToResult(FileHeader));
            }));
        }

        foreach (var bean in ctx.ExportBeans)
        {
            tasks.Add(Task.Run(() =>
            {
                var writer = new CodeWriter();
                RenderBeanDef(ctx, bean, writer);
                return CreateOutputFile($"{GetFileNameWithoutExtByTypeName(bean.FullName)}.def.{FileSuffixName}", writer.ToResult(FileHeader));
            }));
            tasks.Add(Task.Run(() =>
            {
                var writer = new CodeWriter();
                RenderBeanImpl(ctx, bean, writer);
                return CreateOutputFile($"{GetFileNameWithoutExtByTypeName(bean.FullName)}.impl.{FileSuffixName}", writer.ToResult(FileHeader));
            }));
        }

        foreach (var @enum in ctx.ExportEnums)
        {
            tasks.Add(Task.Run(() =>
            {
                var writer = new CodeWriter();
                GenerateEnum(ctx, @enum, writer);
                return CreateOutputFile($"{GetFileNameWithoutExtByTypeName(@enum.FullName)}.{FileSuffixName}", writer.ToResult(FileHeader));
            }));
        }

        Task.WaitAll(tasks.ToArray());
        foreach (var task in tasks)
        {
            manifest.AddFile(task.Result);
        }
    }
}
