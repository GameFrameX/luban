using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Luban.RawDefs;
using Luban.Schema;
using Luban.Utils;

namespace Luban;

public class GlobalConfigLoader : IConfigLoader
{
    private static readonly NLog.Logger s_logger = NLog.LogManager.GetCurrentClassLogger();

    private string _curDir;

    public GlobalConfigLoader()
    {
    }


    private class Group
    {
        public List<string> Names { get; set; }

        public bool Default { get; set; }
    }

    private class SchemaFile
    {
        public string FileName { get; set; }

        public string Type { get; set; }
    }

    private class Target
    {
        public string Name { get; set; }

        public string Manager { get; set; }

        public List<string> Groups { get; set; }

        public string TopModule { get; set; }
    }

    private class LubanConf
    {
        public List<Group> Groups { get; set; }

        public List<SchemaFile> SchemaFiles { get; set; }

        public string DataDir { get; set; }

        public List<Target> Targets { get; set; }

        public List<string> Xargs { get; set; }
    }

    public LubanConfig Load(string fileName)
    {
        s_logger.Debug("load config file:{}", fileName);
        _curDir = Directory.GetParent(fileName).FullName;

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, AllowTrailingCommas = true, ReadCommentHandling = JsonCommentHandling.Skip, };
        // var globalConf = JsonSerializer.Deserialize<LubanConf>(File.ReadAllText(fileName, Encoding.UTF8), options);
        //Json中的字符串支持换行符 Add by XuToWei
        var textContent = File.ReadAllText(fileName, Encoding.UTF8).Replace("\r\n", " ").Replace("\n", " ").Replace("\u0009", " ");
        var globalConf = JsonSerializer.Deserialize<LubanConf>(textContent, options);

        var configFileName = Path.GetFileName(fileName);
        var dataInputDir = Path.Combine(_curDir, globalConf.DataDir);
        List<RawGroup> groups = globalConf.Groups.Select(g => new RawGroup() { Names = g.Names, IsDefault = g.Default, }).ToList();
        List<RawTarget> targets = globalConf.Targets.Select(t => new RawTarget() { Name = t.Name, Manager = t.Manager, Groups = t.Groups, TopModule = t.TopModule, }).ToList();

        List<SchemaFileInfo> importFiles = new List<SchemaFileInfo>();
        foreach (var schemaFile in globalConf.SchemaFiles)
        {
            string fileOrDirectory = Path.Combine(_curDir, schemaFile.FileName);
            if (string.IsNullOrEmpty(schemaFile.Type))
            {
                if (!Directory.Exists(fileOrDirectory) && !File.Exists(fileOrDirectory))
                {
                    throw new Exception($"failed to load schema file:'{fileOrDirectory}': directory or file doesn't exists!");
                }
            }

            var directoryList = FileUtil.GetFileOrDirectory(_curDir, fileOrDirectory);
            foreach (var subFile in directoryList)
            {
                importFiles.Add(new SchemaFileInfo() { FileName = subFile, Type = schemaFile.Type });
            }

            DirectoryInfo directoryInfo = new DirectoryInfo(fileOrDirectory);

            string[] extensions = [".xlsx", ".csv", ".xls", ".xlsm",];

            foreach (var extensionName in extensions)
            {
                var fileInfos = directoryInfo.GetFiles($"*{extensionName}", SearchOption.AllDirectories);
                foreach (var fileInfo in fileInfos)
                {
                    var typeList = fileInfo.Name.Split("__", StringSplitOptions.RemoveEmptyEntries);
                    if (typeList.Length <= 1)
                    {
                        continue;
                    }

                    var type = typeList[0];
                    var newType = string.Empty;
                    if (type.StartsWith("table"))
                    {
                        newType = "table";
                    }
                    else if (type.StartsWith("bean"))
                    {
                        newType = "bean";
                    }
                    else if (type.StartsWith("enum"))
                    {
                        newType = "enum";
                    }

                    if (string.IsNullOrEmpty(newType))
                    {
                        continue;
                    }

                    importFiles.Add(new SchemaFileInfo() { FileName = fileInfo.FullName, Type = newType, });
                }
            }
        }

        return new LubanConfig()
        {
            ConfigFileName = configFileName,
            InputDataDir = dataInputDir,
            Groups = groups,
            Targets = targets,
            Imports = importFiles,
            Xargs = globalConf.Xargs,
        };
    }
}
