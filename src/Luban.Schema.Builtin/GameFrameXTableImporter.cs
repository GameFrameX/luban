using Luban.Defs;
using Luban.RawDefs;
using Luban.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Luban.Schema.Builtin;

[TableImporter("gameframex")]
public class GameFrameXTableImporter : ITableImporter
{
    private static readonly NLog.Logger s_logger = NLog.LogManager.GetCurrentClassLogger();

    public List<RawTable> LoadImportTables()
    {
        string dataDir = GenerationContext.GlobalConf.InputDataDir;

        string fileNamePatternStr = EnvManager.Current.GetOptionOrDefault("tableImporter", "filePattern", false, "([a-zA-Z0-9]-.+)");
        string tableNamespaceFormatStr = EnvManager.Current.GetOptionOrDefault("tableImporter", "tableNamespaceFormat", false, "{0}");
        string tableNameFormatStr = EnvManager.Current.GetOptionOrDefault("tableImporter", "tableNameFormat", false, "Tb{0}");
        string valueTypeNameFormatStr = EnvManager.Current.GetOptionOrDefault("tableImporter", "valueTypeNameFormat", false, "{0}");
        var fileNamePattern = new Regex(fileNamePatternStr);
        var excelExts = new HashSet<string> { "xlsx", "xls", "xlsm", "csv" };

        var tables = new List<RawTable>();
        foreach (string file in Directory.GetFiles(dataDir, "*", SearchOption.AllDirectories))
        {
            if (FileUtil.IsIgnoreFile(dataDir, file))
            {
                continue;
            }

            string fileName = Path.GetFileName(file);
            string ext = Path.GetExtension(fileName).TrimStart('.');
            if (!excelExts.Contains(ext))
            {
                continue;
            }

            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
            var match = fileNamePattern.Match(fileNameWithoutExt);
            if (!match.Success || match.Groups.Count <= 1)
            {
                continue;
            }

            string relativePath = file.Substring(dataDir.Length + 1).TrimStart('\\').TrimStart('/');
            string namespaceFromRelativePath = Path.GetDirectoryName(relativePath).Replace('/', '.').Replace('\\', '.');

            string rawTableFullName = match.Groups[1].Value;
            var split = rawTableFullName.Split(['-', '_',], StringSplitOptions.RemoveEmptyEntries);
            if (split.Length > 1)
            {
                // 代表有首字母的排序, 不管后面有多少都只要第二个切片
                // 获取中间的值
                rawTableFullName = split[1];
            }

            if (IsContainsZhCn(rawTableFullName))
            {
                throw new Exception($"不支持中文表名:[{rawTableFullName}] 文件名:[{fileName}] 表名称定义规范为: 排序编号-导出表名-中文标识名称");
            }

            string rawTableNamespace = TypeUtil.GetNamespace(rawTableFullName);
            string rawTableName = TypeUtil.GetName(rawTableFullName);
            string tableNamespace = TypeUtil.MakeFullName(namespaceFromRelativePath, string.Format(tableNamespaceFormatStr, rawTableNamespace));
            string tableName = string.Format(tableNameFormatStr, rawTableName);
            string valueTypeFullName = TypeUtil.MakeFullName(tableNamespace, string.Format(valueTypeNameFormatStr, rawTableName));

            bool isExist = false;
            foreach (var rawTable in tables)
            {
                if (rawTable.Namespace == tableNamespace && rawTable.Name == tableName)
                {
                    rawTable.InputFiles.Add(relativePath);
                    isExist = true;
                    break;
                }
            }

            if (isExist)
            {
                continue;
            }

            var table = new RawTable()
            {
                Namespace = tableNamespace,
                Name = tableName,
                Index = "",
                ValueType = valueTypeFullName,
                ReadSchemaFromFile = true,
                Mode = TableMode.MAP,
                Comment = "",
                Groups = new List<string> { },
                InputFiles = new List<string> { relativePath },
                OutputFile = "",
            };
            s_logger.Debug("import table file:{@}", table);
            tables.Add(table);
        }


        return tables;
    }

    /// <summary>
    /// 匹配中文正则表达式
    /// </summary>
    private static readonly Regex CnReg = new Regex(@"[\u4e00-\u9fa5]");

    /// <summary>
    /// 判断是否有中文
    /// </summary>
    /// <param name="self">原始字符串</param>
    /// <returns></returns>
    private static bool IsContainsZhCn(string self)
    {
        return CnReg.IsMatch(self);
    }
}
