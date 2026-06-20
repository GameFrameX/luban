using Luban.CodeTarget;
using Luban.Defs;
using Luban.Tmpl;
using Luban.Utils;
using Scriban;
using Scriban.Runtime;

namespace Luban.Cpp.CodeTarget;

public abstract class CppSplitCodeTargetBase : CppCodeTargetBase
{
    protected abstract string DefTemplateDir { get; }

    protected override string TemplateDir => DefTemplateDir;

    private Template GetDefTemplate(string name)
    {
        if (TemplateManager.Ins.TryGetTemplate($"{DefTemplateDir}/{name}", out var template))
        {
            return template;
        }
        throw new Exception($"def template:{name} not found in {DefTemplateDir}/");
    }

    private OutputFile GenerateSchemaHeader(GenerationContext ctx, string outputFileName)
    {
        var enumTasks = new List<Task<string>>();
        foreach (var @enum in ctx.ExportEnums)
        {
            enumTasks.Add(Task.Run(() =>
            {
                var writer = new CodeWriter();
                GenerateEnum(ctx, @enum, writer);
                return writer.ToResult(null);
            }));
        }

        var beanTasks = new List<Task<string>>();
        foreach (var bean in ctx.ExportBeans)
        {
            beanTasks.Add(Task.Run(() =>
            {
                var writer = new CodeWriter();
                GenerateBean(ctx, bean, writer);
                return writer.ToResult(null);
            }));
        }

        var tableTasks = new List<Task<string>>();
        foreach (var table in ctx.ExportTables)
        {
            tableTasks.Add(Task.Run(() =>
            {
                var writer = new CodeWriter();
                GenerateTable(ctx, table, writer);
                return writer.ToResult(null);
            }));
        }

        var tablesWriter = new CodeWriter();
        GenerateTables(ctx, ctx.ExportTables, tablesWriter);

        Task.WaitAll(enumTasks.ToArray());
        Task.WaitAll(beanTasks.ToArray());
        Task.WaitAll(tableTasks.ToArray());

        var template = GetTemplate("schema_h");
        var tplCtx = CreateTemplateContext(template);
        tplCtx.PushGlobal(new ScriptObject
        {
            { "__ctx", ctx },
            { "__top_module", ctx.Target.TopModule },
            { "__enum_codes", string.Join('\n', enumTasks.Select(t => t.Result)) },
            { "__bean_codes", string.Join('\n', beanTasks.Select(t => t.Result)) },
            { "__table_codes", string.Join('\n', tableTasks.Select(t => t.Result)) },
            { "__tables_code", tablesWriter.ToResult(null) },
            { "__beans", ctx.ExportBeans },
            { "__code_style", CodeStyle },
        });

        var schemaHeader = new CodeWriter();
        schemaHeader.Write(template.Render(tplCtx));
        return CreateOutputFile(outputFileName, schemaHeader.ToResult(FileHeader));
    }

    private OutputFile RenderBeanImpl(GenerationContext ctx, DefBean bean, string schemaHeaderFileName)
    {
        var template = GetDefTemplate("bean.impl");
        var tplCtx = CreateTemplateContext(template);
        tplCtx.PushGlobal(CreateBeanContext(ctx, bean));
        tplCtx.PushGlobal(new ScriptObject
        {
            { "__schema_header_file", schemaHeaderFileName },
        });

        var writer = new CodeWriter();
        writer.Write(template.Render(tplCtx));
        return CreateOutputFile($"{bean.FullName}.Impl.cpp", writer.ToResult(FileHeader));
    }

    private OutputFile RenderTableImpl(GenerationContext ctx, DefTable table, string schemaHeaderFileName)
    {
        var template = GetDefTemplate("table.impl");
        var tplCtx = CreateTemplateContext(template);
        tplCtx.PushGlobal(CreateTableContext(ctx, table));
        tplCtx.PushGlobal(new ScriptObject
        {
            { "__schema_header_file", schemaHeaderFileName },
        });

        var writer = new CodeWriter();
        writer.Write(template.Render(tplCtx));
        return CreateOutputFile($"{table.FullName}.Impl.cpp", writer.ToResult(FileHeader));
    }

    private OutputFile RenderTablesImpl(GenerationContext ctx, List<DefTable> tables, string schemaHeaderFileName)
    {
        var template = GetDefTemplate("tables.impl");
        var tplCtx = CreateTemplateContext(template);
        tplCtx.PushGlobal(new ScriptObject
        {
            { "__ctx", ctx },
            { "__name", ctx.Target.Manager },
            { "__namespace", ctx.Target.TopModule },
            { "__tables", tables },
            { "__schema_header_file", schemaHeaderFileName },
            { "__code_style", CodeStyle },
        });

        var writer = new CodeWriter();
        writer.Write(template.Render(tplCtx));
        return CreateOutputFile($"{ctx.Target.Manager}.Impl.cpp", writer.ToResult(FileHeader));
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
        string schemaFileNameWithoutExt = EnvManager.Current.GetOptionOrDefault(Name, "schemaFileNameWithoutExt", true, "schema");
        string schemaFileName = $"{schemaFileNameWithoutExt}.h";
        manifest.AddFile(GenerateSchemaHeader(ctx, schemaFileName));

        var tasks = new List<Task<OutputFile>>();
        foreach (var bean in ctx.ExportBeans)
        {
            tasks.Add(Task.Run(() =>
            {
                var writer = new CodeWriter();
                RenderBeanDef(ctx, bean, writer);
                return CreateOutputFile($"{bean.FullName}.Def.h", writer.ToResult(FileHeader));
            }));
            tasks.Add(Task.Run(() => RenderBeanImpl(ctx, bean, schemaFileName)));
        }

        foreach (var table in ctx.ExportTables)
        {
            tasks.Add(Task.Run(() =>
            {
                var writer = new CodeWriter();
                RenderTableDef(ctx, table, writer);
                return CreateOutputFile($"{table.FullName}.Def.h", writer.ToResult(FileHeader));
            }));
            tasks.Add(Task.Run(() => RenderTableImpl(ctx, table, schemaFileName)));
        }

        tasks.Add(Task.Run(() =>
        {
            var writer = new CodeWriter();
            RenderTablesDef(ctx, ctx.ExportTables, writer);
            return CreateOutputFile($"{ctx.Target.Manager}.Def.h", writer.ToResult(FileHeader));
        }));
        tasks.Add(Task.Run(() => RenderTablesImpl(ctx, ctx.ExportTables, schemaFileName)));

        Task.WaitAll(tasks.ToArray());
        foreach (var task in tasks)
        {
            manifest.AddFile(task.Result);
        }
    }
}
