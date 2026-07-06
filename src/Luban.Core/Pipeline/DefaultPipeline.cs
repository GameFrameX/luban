using Luban.CodeTarget;
using Luban.DataLoader;
using Luban.DataTarget;
using Luban.Defs;
using Luban.L10N;
using Luban.OutputSaver;
using Luban.PostProcess;
using Luban.RawDefs;
using Luban.Schema;
using Luban.TypeVisitors;
using Luban.Utils;
using Luban.Validator;
using NLog;

namespace Luban.Pipeline;

[Pipeline("default")]
public class DefaultPipeline : IPipeline
{
    private static readonly Logger s_logger = LogManager.GetCurrentClassLogger();

    private LubanConfig _config;

    private PipelineArguments _args;

    private RawAssembly _rawAssembly;

    private DefAssembly _defAssembly;

    private GenerationContext _genCtx;

    public DefaultPipeline()
    {
    }

    public void Run(PipelineArguments args)
    {
        _args = args;
        _config = args.Config;
        LoadSchema();
        PrepareGenerationContext();
        ProcessTargets();
    }

    protected void LoadSchema()
    {
        string schemaCollectorName = _args.SchemaCollector;
        s_logger.Info("load schema. collector: {}", schemaCollectorName);
        var schemaCollector = SchemaManager.Ins.CreateSchemaCollector(schemaCollectorName);
        schemaCollector.Load(_config);
        _rawAssembly = schemaCollector.CreateRawAssembly();
    }

    protected void PrepareGenerationContext()
    {
        s_logger.Debug("prepare generation context");
        _genCtx = new GenerationContext();
        _defAssembly = new DefAssembly(_rawAssembly, _args.Target, _args.OutputTables, _config.Groups, _args.Variants);

        CollectAutoExtendEnums();

        var generationCtxBuilder = new GenerationContextBuilder
        {
            Assembly = _defAssembly,
            IncludeTags = _args.IncludeTags,
            ExcludeTags = _args.ExcludeTags,
            TimeZone = _args.TimeZone,
        };
        _genCtx.Init(generationCtxBuilder);
    }

    // 对 autoExtend 枚举进行预扫描：在类型编译完成之后、代码生成与正式数据加载之前，
    // 加载引用了这些枚举的表数据，把未定义的值收集起来并固化为正式枚举项。
    // 这样生成的枚举代码会包含全部（预定义 + 自动收集）的枚举项。
    private void CollectAutoExtendEnums()
    {
        var autoExtendEnums = _defAssembly.TypeList.OfType<DefEnum>().Where(e => e.AutoExtend).ToList();
        if (autoExtendEnums.Count == 0)
        {
            return;
        }

        s_logger.Info("auto-extend enum collect begin: {}", string.Join(",", autoExtendEnums.Select(e => e.FullName)));

        var referencingTables = new List<DefTable>();
        foreach (var table in _defAssembly.GetAllTables())
        {
            var refTypes = new Dictionary<string, DefTypeBase>();
            table.ValueTType.Apply(RefTypeVisitor.Ins, refTypes);
            if (refTypes.Values.OfType<DefEnum>().Any(e => e.AutoExtend))
            {
                referencingTables.Add(table);
            }
        }

        foreach (var e in autoExtendEnums)
        {
            e.BeginAutoExtendCollect();
        }

        try
        {
            foreach (var table in referencingTables)
            {
                CollectTableForAutoExtend(table);
            }
        }
        finally
        {
            foreach (var e in autoExtendEnums)
            {
                e.EndAutoExtendCollectAndApply();
            }
        }

        s_logger.Info("auto-extend enum collect end");
    }

    private void CollectTableForAutoExtend(DefTable table)
    {
        string inputDataDir = GenerationContext.GetInputDataPath();
        var options = new Dictionary<string, string>();
        foreach (var inputFile in table.InputFiles)
        {
            var (actualFile, subAssetName) = FileUtil.SplitFileAndSheetName(FileUtil.Standardize(inputFile));
            foreach (var atomFile in FileUtil.GetFileOrDirectory(inputDataDir, Path.Combine(inputDataDir, actualFile)))
            {
                try
                {
                    // 仅利用加载过程中的副作用（DEnum 构造会记录未定义值），记录本身丢弃。
                    DataLoaderManager.Ins.LoadTableFile(table, atomFile, subAssetName, options);
                }
                catch (Exception e)
                {
                    throw new Exception($"auto-extend collect 加载失败. table:{table.FullName} file:{atomFile}", e);
                }
            }
        }
    }

    protected void LoadDatas()
    {
        _genCtx.LoadDatas();
        DoValidate();
        ProcessL10N();
    }

    protected void DoValidate()
    {
        s_logger.Info("validation begin");
        var v = new DataValidatorContext(_defAssembly);
        v.ValidateTables(_genCtx.Tables);
        s_logger.Info("validation end");
    }

    protected void ProcessL10N()
    {
        if (_genCtx.TextProvider != null)
        {
            _genCtx.TextProvider.ProcessDatas();
        }
    }

    protected void ProcessTargets()
    {
        var tasks = new List<Task>();
        tasks.Add(Task.Run(() =>
        {
            foreach (string target in _args.CodeTargets)
            {
                // code target doesn't support run in parallel
                ICodeTarget m = CodeTargetManager.Ins.CreateCodeTarget(target);
                ProcessCodeTarget(target, m);
            }
        }));

        if (_args.ForceLoadTableDatas || _args.DataTargets.Count > 0)
        {
            LoadDatas();
        }

        if (_args.DataTargets.Count > 0)
        {
            string dataExporterName = EnvManager.Current.GetOptionOrDefault("", BuiltinOptionNames.DataExporter, true, "default");
            s_logger.Debug("dataExporter: {}", dataExporterName);
            IDataExporter dataExporter = DataTargetManager.Ins.CreateDataExporter(dataExporterName);
            foreach (string mission in _args.DataTargets)
            {
                IDataTarget dataTarget = DataTargetManager.Ins.CreateDataTarget(mission);
                tasks.Add(Task.Run(() => ProcessDataTarget(mission, dataExporter, dataTarget)));
            }
        }
        Task.WaitAll(tasks.ToArray());
    }

    protected void ProcessCodeTarget(string name, ICodeTarget codeTarget)
    {
        s_logger.Info("process code target:{} begin", name);
        var outputManifest = new OutputFileManifest(name, OutputType.Code);
        GenerationContext.CurrentCodeTarget = codeTarget;
        codeTarget.ValidateDefinition(_genCtx);
        codeTarget.Handle(_genCtx, outputManifest);

        outputManifest = PostProcess(BuiltinOptionNames.CodePostprocess, outputManifest);
        Save(outputManifest);
        s_logger.Info("process code target:{} end", name);
    }

    protected OutputFileManifest PostProcess(string familyName, OutputFileManifest manifest)
    {
        string name = manifest.TargetName;
        if (EnvManager.Current.TryGetOption(name, familyName, true, out string postProcessName))
        {
            var newManifest = new OutputFileManifest(name, manifest.OutputType);
            PostProcessManager.Ins.GetPostProcess(postProcessName).PostProcess(manifest, newManifest);
            return newManifest;
        }
        return manifest;
    }

    protected void ProcessDataTarget(string name, IDataExporter mission, IDataTarget dataTarget)
    {
        s_logger.Info("process data target:{} begin", name);
        var outputManifest = new OutputFileManifest(name, OutputType.Data);
        mission.Handle(_genCtx, dataTarget, outputManifest);

        var newManifest = PostProcess(BuiltinOptionNames.DataPostprocess, outputManifest);
        Save(newManifest);
        s_logger.Info("process data target:{} end", name);
    }

    private void Save(OutputFileManifest manifest)
    {
        string name = manifest.TargetName;
        string outputSaverName = EnvManager.Current.GetOptionOrDefault(name, BuiltinOptionNames.OutputSaver, true, "local");
        var saver = OutputSaverManager.Ins.GetOutputSaver(outputSaverName);
        saver.Save(manifest);
    }

}
