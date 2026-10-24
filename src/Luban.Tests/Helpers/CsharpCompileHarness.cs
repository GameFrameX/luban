// Copyright 2025 Code Philosophy

using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Text.Json;
using Xunit;

namespace Luban.Tests.Helpers;

public static class CsharpCompileHarness
{
    public static object LoadTables(string codeDir, string dataDir, string codeTarget)
    {
        var id = Guid.NewGuid().ToString("N");
        var workDir = Path.Combine(Path.GetTempPath(), "luban-csharp-harness", id);
        Directory.CreateDirectory(workDir);
        // Keep collectible ALC alive for the returned object lifetime; unique AssemblyName avoids LoadFrom conflicts.
        var alc = new AssemblyLoadContext($"luban-test-{id}", isCollectible: true);
        CopyDirectory(codeDir, Path.Combine(workDir, "Gen"));
        CopyRuntime(workDir);

        var csprojPath = Path.Combine(workDir, "GeneratedConsumer.csproj");
        File.WriteAllText(csprojPath, BuildCsproj(codeTarget, id), Encoding.UTF8);

        var build = RunDotnet(["build", csprojPath, "-c", "Release", "-v", "q"]);
        Assert.True(build.ExitCode == 0, $"dotnet build failed:\n{build.Output}");

        var dll = Path.Combine(workDir, "bin", "Release", "net8.0", $"GeneratedConsumer_{id}.dll");
        Assert.True(File.Exists(dll), $"Built DLL missing: {dll}");

        var asm = alc.LoadFromAssemblyPath(dll);
        var tablesType = asm.GetTypes().FirstOrDefault(t => t.Name == "Tables")
            ?? throw new InvalidOperationException("Generated Tables type not found");

        var ctor = tablesType.GetConstructors().Single();
        // Hold ALC on the returned instance via conditional weak table isn't needed for tests;
        // GC roots the tables object which roots the assembly which roots the ALC.
        GC.KeepAlive(alc);

        if (ctor.GetParameters().Length == 0)
        {
            // GameFrameX fork 模板形态：无参构造 + Init(ConfigComponent) + LoadAsync(loader)
            var tables = ctor.Invoke(Array.Empty<object>());
            var init = tablesType.GetMethod("Init", BindingFlags.Instance | BindingFlags.Public);
            if (init != null && init.GetParameters().Length == 1)
            {
                var configComponent = Activator.CreateInstance(init.GetParameters()[0].ParameterType);
                init.Invoke(tables, new[] { configComponent });
            }
            var loadAsync = tablesType.GetMethod("LoadAsync", BindingFlags.Instance | BindingFlags.Public)
                ?? throw new InvalidOperationException("Tables.LoadAsync(loader) not found");
            var loaderType = loadAsync.GetParameters().Single().ParameterType;
            object loader;
            var loaderArgType = loaderType.GetGenericArguments()[1];
            if (loaderArgType.IsGenericType && loaderArgType.GetGenericTypeDefinition() == typeof(System.Threading.Tasks.Task<>))
            {
                // fork 模板 LoadAsync 形态：Func<string, Task<TPayload>>；用同步 loader 包装为 Task
                var payloadType = loaderArgType.GetGenericArguments()[0];
                var syncFuncType = typeof(Func<,>).MakeGenericType(typeof(string), payloadType);
                var syncLoader = CreateLoader(syncFuncType, dataDir);
                var nameParam = Expression.Parameter(typeof(string), "name");
                var toTask = typeof(CsharpCompileHarness).GetMethod(nameof(ToTask), BindingFlags.NonPublic | BindingFlags.Static)!.MakeGenericMethod(payloadType);
                var body = Expression.Call(toTask, Expression.Invoke(Expression.Constant(syncLoader), nameParam));
                loader = Expression.Lambda(loaderType, body, nameParam).Compile();
            }
            else
            {
                loader = CreateLoader(loaderType, dataDir);
            }
            var task = (System.Threading.Tasks.Task)loadAsync.Invoke(tables, new object[] { loader });
            task.GetAwaiter().GetResult();
            return tables;
        }

        var officialLoaderType = ctor.GetParameters().Single().ParameterType;
        var officialLoader = CreateLoader(officialLoaderType, dataDir);
        return ctor.Invoke(new object[] { officialLoader });
    }

    private static System.Threading.Tasks.Task<T> ToTask<T>(T value)
    {
        return System.Threading.Tasks.Task.FromResult(value);
    }

    public static T GetProp<T>(object obj, string name)
    {
        var type = obj.GetType();
        var prop = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase);
        if (prop != null)
        {
            return (T)prop.GetValue(obj);
        }

        var field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase);
        Assert.True(field != null, $"Member '{name}' not found on {type.FullName}. Members: "
            + string.Join(", ", type.GetMembers(BindingFlags.Instance | BindingFlags.Public).Select(m => m.Name)));
        return (T)field.GetValue(obj);
    }

    public static object Invoke(object obj, string methodName, params object[] args)
    {
        var methods = obj.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .Where(m => m.Name == methodName && m.GetParameters().Length == args.Length)
            .ToList();
        Assert.True(methods.Count > 0, $"Method '{methodName}' not found on {obj.GetType().FullName}");
        var exact = methods.FirstOrDefault(m => m.GetParameters().Select(p => p.ParameterType).SequenceEqual(args.Select(a => a.GetType())));
        var method = exact ?? methods.Single(m => m.GetParameters().All(p => args.All(a => IsArgumentCompatible(p.ParameterType, a))));
        return method.Invoke(obj, args);
    }

    private static bool IsArgumentCompatible(Type parameterType, object arg)
    {
        if (parameterType.IsInstanceOfType(arg))
        {
            return true;
        }
        if (arg == null)
        {
            return !parameterType.IsValueType;
        }
        var argCode = Type.GetTypeCode(arg.GetType());
        var paramCode = Type.GetTypeCode(parameterType);
        return argCode is >= TypeCode.SByte and <= TypeCode.Double
            && paramCode is >= TypeCode.SByte and <= TypeCode.Double;
    }

    private static Delegate CreateLoader(Type funcType, string dataDir)
    {
        // Func<string, TPayload>
        var payloadType = funcType.GetGenericArguments()[1];
        var nameParam = Expression.Parameter(typeof(string), "name");

        Expression body;
        if (payloadType == typeof(JsonElement))
        {
            var method = typeof(CsharpCompileHarness).GetMethod(nameof(LoadJsonElement), BindingFlags.NonPublic | BindingFlags.Static)!;
            body = Expression.Call(method, Expression.Constant(dataDir), nameParam);
        }
        else if (payloadType.Name == "ByteBuf")
        {
            var method = typeof(CsharpCompileHarness).GetMethod(nameof(LoadByteBuf), BindingFlags.NonPublic | BindingFlags.Static)!
                .MakeGenericMethod(payloadType);
            body = Expression.Call(method, Expression.Constant(dataDir), nameParam);
        }
        else if (payloadType.Name == "JSONNode" || (payloadType.Namespace?.Contains("SimpleJSON") ?? false))
        {
            var method = typeof(CsharpCompileHarness).GetMethod(nameof(LoadJsonNode), BindingFlags.NonPublic | BindingFlags.Static)!
                .MakeGenericMethod(payloadType);
            body = Expression.Call(method, Expression.Constant(dataDir), Expression.Constant(payloadType.Assembly), nameParam);
        }
        else if (payloadType.FullName == "Newtonsoft.Json.Linq.JArray")
        {
            var method = typeof(CsharpCompileHarness).GetMethod(nameof(LoadJArray), BindingFlags.NonPublic | BindingFlags.Static)!;
            body = Expression.Call(method, Expression.Constant(dataDir), nameParam);
        }
        else
        {
            throw new NotSupportedException($"Unsupported Tables loader payload type: {payloadType.FullName}");
        }

        return Expression.Lambda(funcType, body, nameParam).Compile();
    }

    private static JsonElement LoadJsonElement(string dataDir, string name)
    {
        var path = ResolveDataFile(dataDir, name, ".json");
        using var doc = JsonDocument.Parse(File.ReadAllText(path, Encoding.UTF8));
        return doc.RootElement.Clone();
    }

    private static TByteBuf LoadByteBuf<TByteBuf>(string dataDir, string name)
    {
        var path = ResolveDataFile(dataDir, name, ".bytes");
        return (TByteBuf)Activator.CreateInstance(typeof(TByteBuf), File.ReadAllBytes(path))!;
    }

    private static TNode LoadJsonNode<TNode>(string dataDir, Assembly asm, string name)
    {
        var path = ResolveDataFile(dataDir, name, ".json");
        var text = File.ReadAllText(path, Encoding.UTF8);
        var jsonType = asm.GetType("SimpleJSON.JSON")
            ?? throw new InvalidOperationException("SimpleJSON.JSON not found in generated assembly");
        var parse = jsonType.GetMethod("Parse", BindingFlags.Public | BindingFlags.Static, new[] { typeof(string) })
            ?? throw new InvalidOperationException("JSON.Parse(string) not found");
        return (TNode)parse.Invoke(null, new object[] { text })!;
    }

    private static Newtonsoft.Json.Linq.JArray LoadJArray(string dataDir, string name)
    {
        var path = ResolveDataFile(dataDir, name, ".json");
        return Newtonsoft.Json.Linq.JArray.Parse(File.ReadAllText(path, Encoding.UTF8));
    }

    private static string ResolveDataFile(string dataDir, string name, string preferredExt)
    {
        var preferred = Path.Combine(dataDir, name + preferredExt);
        if (File.Exists(preferred))
        {
            return preferred;
        }

        var matches = Directory.GetFiles(dataDir, name + ".*", SearchOption.TopDirectoryOnly);
        Assert.True(matches.Length > 0, $"Data file not found for table '{name}' under {dataDir}");
        return matches[0];
    }

    private static string BuildCsproj(string codeTarget, string assemblyId)
    {
        var packages = codeTarget.Contains("newtonsoft", StringComparison.OrdinalIgnoreCase)
            ? """    <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />"""
            : "";

        return $"""
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net8.0</TargetFramework>
                <ImplicitUsings>enable</ImplicitUsings>
                <Nullable>disable</Nullable>
                <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
                <OutputType>Library</OutputType>
                <AssemblyName>GeneratedConsumer_{assemblyId}</AssemblyName>
              </PropertyGroup>
              <ItemGroup>
            {packages}
              </ItemGroup>
            </Project>
            """;
    }

    private static void CopyRuntime(string workDir)
    {
        var runtimeSrc = TestPaths.RuntimeSupportRoot;
        foreach (var file in Directory.GetFiles(runtimeSrc, "*.cs", SearchOption.AllDirectories))
        {
            if (file.Contains("Unity", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var relative = Path.GetRelativePath(runtimeSrc, file);
            var dest = Path.Combine(workDir, "Runtime", relative);
            Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
            File.Copy(file, dest, overwrite: true);
        }
    }

    private static void CopyDirectory(string source, string destination)
    {
        Directory.CreateDirectory(destination);
        foreach (var file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
        {
            var dest = Path.Combine(destination, Path.GetRelativePath(source, file));
            Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
            File.Copy(file, dest, overwrite: true);
        }
    }

    private static (int ExitCode, string Output) RunDotnet(string[] args)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "dotnet",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        foreach (var arg in args)
        {
            psi.ArgumentList.Add(arg);
        }

        using var proc = Process.Start(psi)!;
        var stdout = proc.StandardOutput.ReadToEnd();
        var stderr = proc.StandardError.ReadToEnd();
        proc.WaitForExit();
        return (proc.ExitCode, stdout + stderr);
    }
}
