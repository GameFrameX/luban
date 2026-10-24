// Copyright 2025 Code Philosophy

using System.IO.Compression;
using System.Text.Json;
using Luban.Tests.Helpers;
using Xunit;

namespace Luban.Tests.Integration;

/// <summary>
/// GameFrameX fork 自定义修复的回归测试：每条用例对应一个 fork 提交，防止上游同步时被冲突解决覆盖。
/// </summary>
[Collection("LubanPipeline")]
public class GameFrameXRegressionTests
{
    private const string Fixture = "gameframex-regression";

    // e1f39884：cs-dotnet-json 表类自持 _loadFunc，不再链到基类 JSON 构造
    [Fact]
    public void CsDotnetJson_TableOwnsLoadFunc()
    {
        using var workspace = new TempWorkspace();
        LubanRunner.Run(TestPaths.FixtureConf(Fixture), codeTargets: new[] { "cs-dotnet-json" }, outputCodeDir: workspace.CodeDir);

        foreach (var table in new[] { "TbItem", "TbNotice", "TbGlobal" })
        {
            var source = ReadCode(workspace, table + ".cs");
            Assert.Contains("private readonly System.Func<System.Threading.Tasks.Task<JsonElement>> _loadFunc;", source);
            Assert.DoesNotContain(": base(loadFunc)", source);
        }
    }

    // 27b318b8：cs-dotnet-json 非空 ref 字段取值改用 TryGet，悬空引用不抛 KeyNotFoundException
    [Fact]
    public void CsDotnetJson_NonNullableRef_UsesTryGet()
    {
        using var workspace = new TempWorkspace();
        LubanRunner.Run(TestPaths.FixtureConf(Fixture), codeTargets: new[] { "cs-dotnet-json" }, outputCodeDir: workspace.CodeDir);

        var source = ReadCode(workspace, "Item.cs");
        Assert.Contains("tables.TbShape.TryGet(ShapeId, out var shapeIdRef);", source);
        Assert.Contains("ShapeId_Ref = shapeIdRef;", source);
    }

    // ff138c33：cs-dotnet-bin 三种表模式 LoadAsync 均为 override；map 表重载前清空 LongDataMaps
    [Fact]
    public void CsDotnetBin_LoadAsync_OverridesForAllModes_AndMapClearsLongDataMaps()
    {
        using var workspace = new TempWorkspace();
        LubanRunner.Run(TestPaths.FixtureConf(Fixture), codeTargets: new[] { "cs-dotnet-bin" }, outputCodeDir: workspace.CodeDir);

        foreach (var table in new[] { "TbItem", "TbNotice", "TbGlobal" })
        {
            Assert.Contains("public override async System.Threading.Tasks.Task LoadAsync()", ReadCode(workspace, table + ".cs"));
        }
        Assert.Contains("LongDataMaps.Clear();", ReadCode(workspace, "TbItem.cs"));
    }

    // 24774645：TranslateText 按继承关系生成，抽象基类 virtual、子类 override
    [Theory]
    [InlineData("cs-dotnet-json")]
    [InlineData("cs-dotnet-bin")]
    [InlineData("cs-bin")]
    [InlineData("cs-simple-json")]
    public void TranslateText_FollowsInheritance(string codeTarget)
    {
        using var workspace = new TempWorkspace();
        LubanRunner.Run(TestPaths.FixtureConf(Fixture), codeTargets: new[] { codeTarget }, outputCodeDir: workspace.CodeDir);

        Assert.Contains("public virtual void TranslateText(", ReadCode(workspace, "Shape.cs"));
        Assert.Contains("public override void TranslateText(", ReadCode(workspace, "Circle.cs"));
    }

    // 95b08b0b：cs-bin-split 的 bean.def 不再引入 SimpleJSON（二进制目标无该依赖）
    [Fact]
    public void CsBinSplit_BeanDefinition_HasNoSimpleJsonUsing()
    {
        using var workspace = new TempWorkspace();
        LubanRunner.Run(TestPaths.FixtureConf(Fixture), codeTargets: new[] { "cs-bin-split" }, outputCodeDir: workspace.CodeDir);

        var definitionFiles = Directory.GetFiles(workspace.CodeDir, "*.Def.cs", SearchOption.AllDirectories);
        Assert.Contains(definitionFiles, file => Path.GetFileName(file) == "Circle.Def.cs");
        foreach (var file in definitionFiles)
        {
            Assert.DoesNotContain("using SimpleJSON;", File.ReadAllText(file));
        }
    }

    // dd520088 + a93ab40f：整型字段、分隔符 map key、多列标题 map key 均支持 0x/0X 十六进制字面量
    [Fact]
    public void HexLiteral_IntFieldAndMapKeys_Parsed()
    {
        using var workspace = new TempWorkspace();
        LubanRunner.Run(TestPaths.FixtureConf(Fixture), dataTargets: new[] { "json" }, outputDataDir: workspace.DataDir);

        using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(workspace.DataDir, "tbhex.json")));
        var first = document.RootElement[0];
        Assert.Equal(16, first.GetProperty("flag").GetInt32());
        Assert.Equal(255, document.RootElement[1].GetProperty("flag").GetInt32());
        Assert.Equal(new[] { (1, 10), (2, 20) }, ReadIntMap(first.GetProperty("weights")));
        Assert.Equal(new[] { (1, 5), (2, 6) }, ReadIntMap(first.GetProperty("bonus")));
    }

    // b2a3e7b7 + 421349aa：内联本地化 xlsx 内容无变化时不重写；源语言 value 变化时同步更新
    [Fact]
    public void InlineL10n_SkipsRewriteWhenUnchanged_AndSyncsChangedValue()
    {
        using var workspace = new TempWorkspace();
        var fixtureDirectory = CopyFixture("gameframex-inline-l10n", workspace);
        var generatedDirectory = Path.Combine(workspace.Root, "generated");
        var options = new Dictionary<string, string>
        {
            ["l10n.inline.enabled"] = "true",
            ["l10n.inline.languageFieldName"] = "zh_CN",
            ["l10n.inline.outputPath"] = generatedDirectory,
        };
        void Generate() => LubanRunner.Run(Path.Combine(fixtureDirectory, "luban.conf"), dataTargets: new[] { "json" }, outputDataDir: workspace.DataDir, extraOptions: options);

        Generate();
        var workbook = Path.Combine(generatedDirectory, "TbItem.xlsx");
        var original = File.ReadAllBytes(workbook);
        var stamp = DateTime.UtcNow.AddHours(-1);
        File.SetLastWriteTimeUtc(workbook, stamp);

        Generate();
        Assert.Equal(stamp, File.GetLastWriteTimeUtc(workbook));
        Assert.Equal(original, File.ReadAllBytes(workbook));

        var dataFile = Path.Combine(fixtureDirectory, "Data", "item.csv");
        File.WriteAllText(dataFile, File.ReadAllText(dataFile).Replace(",剑", ",长剑"));
        Generate();
        var sheet = ReadSheetXml(workbook);
        Assert.Contains("长剑", sheet);
        Assert.Contains("盾", sheet);
    }

    // 74d27f66：不同模块的短表名重复只记录 Fatal 日志，不中断生成
    [Fact]
    public void DuplicateShortTableName_LogsWithoutAborting()
    {
        using var workspace = new TempWorkspace();
        var result = LubanRunner.Run(TestPaths.FixtureConf("gameframex-duplicate-table"), dataTargets: new[] { "json" }, outputDataDir: workspace.DataDir, throwOnError: false);

        Assert.True(result.Succeeded, result.Exception?.ToString());
        Assert.True(File.Exists(Path.Combine(workspace.DataDir, "alpha_tbduplicate.json")));
        Assert.True(File.Exists(Path.Combine(workspace.DataDir, "beta_tbduplicate.json")));
    }

    // 97ab82ec：Rust bean 类型引用按小写模块路径生成，与 `pub mod battle;` 声明一致
    [Fact]
    public void Rust_BeanReference_UsesLowercaseModulePath()
    {
        using var workspace = new TempWorkspace();
        LubanRunner.Run(TestPaths.FixtureConf("gameframex-rust"), codeTargets: new[] { "rust-bin" }, outputCodeDir: workspace.CodeDir);

        var source = File.ReadAllText(Path.Combine(workspace.CodeDir, "cfg", "src", "battle.rs"));
        Assert.Contains("pub pos: crate::battle::Pos,", source);
        Assert.DoesNotContain("crate::Battle::", source);
    }

    private static string ReadCode(TempWorkspace workspace, string fileName)
    {
        return File.ReadAllText(Directory.GetFiles(workspace.CodeDir, fileName, SearchOption.AllDirectories).Single());
    }

    private static (int Key, int Value)[] ReadIntMap(JsonElement map)
    {
        return map.EnumerateArray().Select(pair => (pair[0].GetInt32(), pair[1].GetInt32())).ToArray();
    }

    private static string CopyFixture(string fixtureName, TempWorkspace workspace)
    {
        var source = TestPaths.Fixture(fixtureName);
        var destination = Path.Combine(workspace.Root, fixtureName);
        foreach (var file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
        {
            var target = Path.Combine(destination, Path.GetRelativePath(source, file));
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            File.Copy(file, target);
        }
        return destination;
    }

    private static string ReadSheetXml(string workbookPath)
    {
        using var archive = ZipFile.OpenRead(workbookPath);
        using var reader = new StreamReader(archive.GetEntry("xl/worksheets/sheet1.xml")!.Open());
        return reader.ReadToEnd();
    }
}
