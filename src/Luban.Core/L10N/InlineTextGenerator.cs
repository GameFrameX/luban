using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using ExcelDataReader;

namespace Luban.L10N;

internal static class InlineTextGenerator
{
    public static bool Enabled => EnvManager.Current.GetBoolOptionOrDefault(BuiltinOptionNames.L10NFamily, BuiltinOptionNames.L10NInlineEnabled, false, false);

    public static void Generate(InlineTextCollection collection)
    {
        var env = EnvManager.Current;
        if (!Enabled)
        {
            return;
        }

        string language = env.GetOptionOrDefault(BuiltinOptionNames.L10NFamily, BuiltinOptionNames.L10NInlineLanguageFieldName, false, "");
        if (string.IsNullOrWhiteSpace(language))
        {
            throw new Exception($"'-x {BuiltinOptionNames.L10NFamily}.{BuiltinOptionNames.L10NInlineLanguageFieldName}=zh_CN' missing");
        }

        string outputPath = env.GetOptionOrDefault(BuiltinOptionNames.L10NFamily, BuiltinOptionNames.L10NInlineOutputPath, false, "Localization/Generated");
        if (string.IsNullOrWhiteSpace(outputPath))
        {
            throw new Exception($"'-x {BuiltinOptionNames.L10NFamily}.{BuiltinOptionNames.L10NInlineOutputPath}=<path>' cannot be empty");
        }
        Directory.CreateDirectory(outputPath);

        string fileNameFormat = env.GetOptionOrDefault(BuiltinOptionNames.L10NFamily, BuiltinOptionNames.L10NInlineOutputFileNameFormat, false, "{table}.xlsx");
        if (string.IsNullOrWhiteSpace(fileNameFormat))
        {
            throw new Exception($"'-x {BuiltinOptionNames.L10NFamily}.{BuiltinOptionNames.L10NInlineOutputFileNameFormat}=<format>' cannot be empty");
        }

        foreach (var pair in collection.EntriesByTable.OrderBy(x => x.Key, StringComparer.Ordinal))
        {
            // 表内按 source 部件分组（源文件/sheet 维度的分表）；文件名模板未区分的组归并到同一输出文件
            var fileGroups = new Dictionary<string, List<InlineTextEntry>>(StringComparer.Ordinal);
            foreach (var group in pair.Value
                         .Select(e => (Entry: e, Parts: SplitGameFrameXSource(e.Source)))
                         .GroupBy(x => x.Parts))
            {
                var parts = group.Key;
                if (string.IsNullOrEmpty(parts.Source))
                {
                    parts.Source = pair.Key;
                }
                string fileName = fileNameFormat
                    .Replace("{table}", SanitizeFileName(pair.Key))
                    .Replace("{source}", SanitizeFileName(parts.Source))
                    .Replace("{sourceDir}", SanitizeFileName(parts.SourceDir))
                    .Replace("{rawName}", SanitizeFileName(parts.RawName))
                    .Replace("{comment}", SanitizeFileName(parts.Comment))
                    .Replace("{sheet}", SanitizeFileName(parts.Sheet));
                if (!fileGroups.TryGetValue(fileName, out var entries))
                {
                    entries = new List<InlineTextEntry>();
                    fileGroups.Add(fileName, entries);
                }
                entries.AddRange(group.Select(x => x.Entry));
            }

            foreach (var filePair in fileGroups)
            {
                string fullPath = Path.Combine(outputPath, filePair.Key);

                if (File.Exists(fullPath))
                {
                    MergeIntoExisting(fullPath, filePair.Value, language);
                }
                else
                {
                    WriteFresh(fullPath, filePair.Value, language);
                }
            }
        }
    }

    /// <summary>
    /// 把 inline entries 写入新 xlsx。
    /// </summary>
    private static void WriteFresh(string fullPath, List<InlineTextEntry> entries, string language)
    {
        var rows = BuildBaseRows(language);
        rows.AddRange(entries.OrderBy(x => x.Key, StringComparer.Ordinal).Select(x => new[] { "", x.Key, x.Value }));
        InlineTextXlsxWriter.Write(fullPath, rows);
    }

    /// <summary>
    /// 把 inline entries 合并进现有 xlsx：保留所有现有 cell（含用户翻译），仅追加新 key 到末尾，
    /// 并把 <paramref name="language"/> 列补到 ##var/##type/##group 中（如缺失）。
    /// </summary>
    private static void MergeIntoExisting(string fullPath, List<InlineTextEntry> entries, string language)
    {
        var existing = InlineTextXlsxReader.Read(fullPath);

        // 现有数据行的 key（B 列）作为保留集合
        var existingKeys = new HashSet<string>(StringComparer.Ordinal);
        foreach (var row in existing.DataRows)
        {
            if (row.Count > 1 && !string.IsNullOrEmpty(row[1]))
            {
                existingKeys.Add(row[1]);
            }
        }

        // 计算 language 列的整行列索引（含 A 列）；如果 metadata 里没有则扩展
        int languageColumnIndex = existing.FieldNames.IndexOf(language);
        if (languageColumnIndex < 0)
        {
            // 新增 language 列到 metadata（##var/##type/##group 同步）
            existing.FieldNames.Add(language);
            existing.Types.Add("string");
            existing.Groups.Add("");
            languageColumnIndex = existing.FieldNames.Count - 1;
        }
        languageColumnIndex += 1; // +1 因为 A 列占 0 索引

        // 收集需要追加的新行（key 不在现有）
        var newRows = new List<string[]>();
        foreach (var entry in entries.OrderBy(x => x.Key, StringComparer.Ordinal))
        {
            if (existingKeys.Contains(entry.Key))
            {
                continue;
            }
            existingKeys.Add(entry.Key);
            var row = BuildDataRow(existing.FieldNames.Count + 1, languageColumnIndex, entry.Key, entry.Value);
            newRows.Add(row);
        }

        InlineTextXlsxWriter.WriteMerge(fullPath, existing, newRows);
    }

    /// <summary>
    /// 生成 metadata header 行（##var / ##var 占位 / ##type / ##group / ## / ## 占位）。
    /// </summary>
    private static List<string[]> BuildBaseRows(string language)
    {
        return new List<string[]>
        {
            new[] { "##var", "key", language },
            new[] { "##var", "", "" },
            new[] { "##type", "string", "string" },
            new[] { "##group", "", "" },
            new[] { "##", "key", language },
            new[] { "##", "", "" },
        };
    }

    /// <summary>
    /// 生成数据行：A 列空，B 列 key，languageColumnIndex 列放 value，其余空字符串。
    /// </summary>
    private static string[] BuildDataRow(int totalColumns, int languageColumnIndex, string key, string value)
    {
        var row = new string[totalColumns];
        row[0] = "";
        row[1] = key;
        row[languageColumnIndex] = value;
        return row;
    }

    private static string SanitizeFileName(string value)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var builder = new StringBuilder(value.Length);
        foreach (char c in value)
        {
            builder.Append(invalidChars.Contains(c) || c == '/' || c == '\\' ? '_' : c);
        }
        return builder.ToString();
    }

    /// <summary>
    /// 按 luban Record.Source 的实际格式（<c>{sheet}@{file}</c>，见 RowColumnSheet.UrlWithParams；无 sheet 时为纯文件路径）
    /// 把 source 拆成 (源文件名, 所在目录最后一段, split[1] 导出表名, split[2..] 注释, sheet 名)。
    /// 源文件名部分再按 GameFrameX 命名约定（<c>[a-zA-Z0-9]-.+</c>，split by <c>-</c>/<c>_</c>）拆出 rawName 与 comment。
    /// </summary>
    private static (string Source, string SourceDir, string RawName, string Comment, string Sheet) SplitGameFrameXSource(string source)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            return ("", "", "", "", "");
        }
        string sheet = "";
        string filePart = source;
        int atIndex = source.IndexOf('@');
        if (atIndex >= 0)
        {
            sheet = source[..atIndex];
            filePart = source[(atIndex + 1)..];
        }
        string sourceDir = Path.GetFileName(Path.GetDirectoryName(filePart)) ?? "";
        string fileName = Path.GetFileName(filePart);
        string baseName = Path.GetFileNameWithoutExtension(fileName);
        if (string.IsNullOrEmpty(baseName))
        {
            return ("", "", "", "", "");
        }
        var split = baseName.Split(new[] { '-', '_' }, StringSplitOptions.RemoveEmptyEntries);
        string rawName = split.Length >= 2 ? split[1] : "";
        string comment = split.Length >= 3 ? string.Join("-", split.Skip(2)) : "";
        return (baseName, sourceDir, rawName, comment, sheet);
    }

}

internal sealed class InlineTextXlsxReader
{
    /// <summary>
    /// 现有 sheet 的内容：B+ 列的字段名、类型、组分组，以及所有数据行（A 列为空，B 列为 key）。
    /// </summary>
    public sealed class SheetData
    {
        public List<string> FieldNames { get; set; } = new();
        public List<string> Types { get; set; } = new();
        public List<string> Groups { get; set; } = new();
        public List<List<string>> DataRows { get; set; } = new();
    }

    public static SheetData Read(string path)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var reader = ExcelReaderFactory.CreateReader(stream);

        // 第一行 ##var 是字段名行（A 列 ##var，B+ 列字段名）
        List<string> fieldNames = new();
        List<string> types = new();
        List<string> groups = new();
        var dataRows = new List<List<string>>();
        bool headerParsed = false;

        int rowIndex = 0;
        while (reader.Read())
        {
            int fieldCount = reader.FieldCount;
            string tag = fieldCount > 0 ? (reader.GetValue(0)?.ToString() ?? "").Trim() : "";

            if (!headerParsed)
            {
                if (tag == "##var")
                {
                    // 占位行（B+ 全空）跳过；字段名行才收集
                    bool allEmpty = true;
                    for (int i = 1; i < fieldCount; i++)
                    {
                        if (!string.IsNullOrEmpty(reader.GetValue(i)?.ToString()))
                        {
                            allEmpty = false;
                            break;
                        }
                    }
                    if (!allEmpty)
                    {
                        for (int i = 1; i < fieldCount; i++)
                        {
                            fieldNames.Add(reader.GetValue(i)?.ToString() ?? "");
                        }
                    }
                }
                else if (tag == "##type")
                {
                    for (int i = 1; i < fieldCount; i++)
                    {
                        types.Add(reader.GetValue(i)?.ToString() ?? "");
                    }
                }
                else if (tag == "##group")
                {
                    for (int i = 1; i < fieldCount; i++)
                    {
                        groups.Add(reader.GetValue(i)?.ToString() ?? "");
                    }
                    headerParsed = true;
                }
            }
            else if (!string.IsNullOrEmpty(tag) && tag.StartsWith("##"))
            {
                continue;
            }
            else if (string.IsNullOrEmpty(tag) && fieldCount > 1 && !string.IsNullOrEmpty(reader.GetValue(1)?.ToString()))
            {
                var row = new List<string>();
                for (int i = 0; i < fieldCount; i++)
                {
                    row.Add(reader.GetValue(i)?.ToString() ?? "");
                }
                dataRows.Add(row);
            }
            rowIndex++;
        }

        return new SheetData
        {
            FieldNames = fieldNames,
            Types = types,
            Groups = groups,
            DataRows = dataRows,
        };
    }
}

internal static class InlineTextXlsxWriter
{
    private const int FirstColumn = 1;
    private const string SpreadsheetNamespace = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
    private const string RelationshipNamespace = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
    private const string PackageRelationshipNamespace = "http://schemas.openxmlformats.org/package/2006/relationships";
    private const string ContentTypesNamespace = "http://schemas.openxmlformats.org/package/2006/content-types";

    /// <summary>
    /// 全新写入 xlsx（含 metadata + 数据行）。
    /// </summary>
    public static void Write(string path, IReadOnlyList<string[]> rows)
    {
        WriteSheetXml(path, rows);
    }

    /// <summary>
    /// 把 <paramref name="newRows"/> 追加到现有 <paramref name="existing"/> 之后，重写整张 sheet。
    /// metadata 行（##var/##type/##group）由 <paramref name="existing"/> 提供；新行按现有列数布局。
    /// </summary>
    public static void WriteMerge(string path, InlineTextXlsxReader.SheetData existing, IReadOnlyList<string[]> newRows)
    {
        var allRows = new List<string[]>();
        // metadata：第一列放 tag，后续 B+ 列放值
        allRows.Add(AppendTag("##var", existing.FieldNames));
        allRows.Add(AppendTag("##var", Enumerable.Range(0, existing.FieldNames.Count).Select(_ => "")));
        allRows.Add(AppendTag("##type", existing.Types));
        allRows.Add(AppendTag("##group", existing.Groups));

        // 现有数据行：保持原列数和原值
        foreach (var dataRow in existing.DataRows)
        {
            var row = new string[existing.FieldNames.Count + 1];
            for (int i = 0; i < row.Length; i++)
            {
                row[i] = i < dataRow.Count ? dataRow[i] : "";
            }
            allRows.Add(row);
        }

        // 追加新行：长度对齐现有 FieldNames.Count + 1
        foreach (var newRow in newRows)
        {
            var row = new string[existing.FieldNames.Count + 1];
            for (int i = 0; i < row.Length; i++)
            {
                row[i] = i < newRow.Length ? newRow[i] : "";
            }
            allRows.Add(row);
        }

        WriteSheetXml(path, allRows);
    }

    private static string[] AppendTag(string tag, IEnumerable<string> values)
    {
        var list = new List<string> { tag };
        list.AddRange(values);
        return list.ToArray();
    }

    private static void WriteSheetXml(string path, IReadOnlyList<string[]> rows)
    {
        using var archive = new ZipArchive(File.Create(path), ZipArchiveMode.Create);
        WriteXml(archive, "[Content_Types].xml", WriteContentTypes);
        WriteXml(archive, "_rels/.rels", WritePackageRelationships);
        WriteXml(archive, "xl/workbook.xml", WriteWorkbook);
        WriteXml(archive, "xl/_rels/workbook.xml.rels", WriteWorkbookRelationships);
        WriteXml(archive, "xl/worksheets/sheet1.xml", writer => WriteWorksheet(writer, rows));
    }

    private static void WriteXml(ZipArchive archive, string entryName, Action<XmlWriter> write)
    {
        var entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
        using var stream = entry.Open();
        using var writer = XmlWriter.Create(stream, new XmlWriterSettings
        {
            Encoding = new UTF8Encoding(false),
            OmitXmlDeclaration = false,
            Indent = false,
        });
        writer.WriteStartDocument();
        write(writer);
        writer.WriteEndDocument();
    }

    private static void WriteContentTypes(XmlWriter writer)
    {
        writer.WriteStartElement("Types", ContentTypesNamespace);
        WriteElement(writer, "Default", ContentTypesNamespace, "Extension", "rels", "ContentType", "application/vnd.openxmlformats-package.relationships+xml");
        WriteElement(writer, "Default", ContentTypesNamespace, "Extension", "xml", "ContentType", "application/xml");
        WriteElement(writer, "Override", ContentTypesNamespace, "PartName", "/xl/workbook.xml", "ContentType", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml");
        WriteElement(writer, "Override", ContentTypesNamespace, "PartName", "/xl/worksheets/sheet1.xml", "ContentType", "application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml");
        writer.WriteEndElement();
    }

    private static void WritePackageRelationships(XmlWriter writer)
    {
        writer.WriteStartElement("Relationships", PackageRelationshipNamespace);
        WriteElement(writer, "Relationship", PackageRelationshipNamespace, "Id", "rId1", "Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument", "Target", "xl/workbook.xml");
        writer.WriteEndElement();
    }

    private static void WriteWorkbook(XmlWriter writer)
    {
        writer.WriteStartElement("workbook", SpreadsheetNamespace);
        writer.WriteAttributeString("xmlns", "r", null, RelationshipNamespace);
        writer.WriteStartElement("sheets", SpreadsheetNamespace);
        writer.WriteStartElement("sheet", SpreadsheetNamespace);
        writer.WriteAttributeString("name", "Localization");
        writer.WriteAttributeString("sheetId", "1");
        writer.WriteAttributeString("r", "id", RelationshipNamespace, "rId1");
        writer.WriteEndElement();
        writer.WriteEndElement();
        writer.WriteEndElement();
    }

    private static void WriteWorkbookRelationships(XmlWriter writer)
    {
        writer.WriteStartElement("Relationships", PackageRelationshipNamespace);
        WriteElement(writer, "Relationship", PackageRelationshipNamespace, "Id", "rId1", "Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet", "Target", "worksheets/sheet1.xml");
        writer.WriteEndElement();
    }

    private static void WriteWorksheet(XmlWriter writer, IReadOnlyList<string[]> rows)
    {
        writer.WriteStartElement("worksheet", SpreadsheetNamespace);
        writer.WriteStartElement("dimension", SpreadsheetNamespace);
        int maxColumn = rows.Count > 0 ? rows.Max(row => row.Length) : 1;
        writer.WriteAttributeString("ref", $"A1:{GetCellReference(rows.Count, FirstColumn + maxColumn - 1)}");
        writer.WriteEndElement();
        writer.WriteStartElement("sheetData", SpreadsheetNamespace);
        for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
        {
            writer.WriteStartElement("row", SpreadsheetNamespace);
            writer.WriteAttributeString("r", (rowIndex + 1).ToString());
            string[] row = rows[rowIndex];
            for (int columnIndex = 0; columnIndex < row.Length; columnIndex++)
            {
                string value = row[columnIndex] ?? "";
                writer.WriteStartElement("c", SpreadsheetNamespace);
                writer.WriteAttributeString("r", GetCellReference(rowIndex + 1, FirstColumn + columnIndex));
                writer.WriteAttributeString("t", "inlineStr");
                writer.WriteStartElement("is", SpreadsheetNamespace);
                writer.WriteStartElement("t", SpreadsheetNamespace);
                if (value.Length > 0 && (char.IsWhiteSpace(value[0]) || char.IsWhiteSpace(value[^1])))
                {
                    writer.WriteAttributeString("xml", "space", "http://www.w3.org/XML/1998/namespace", "preserve");
                }
                writer.WriteString(value);
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }
        writer.WriteEndElement();
        writer.WriteEndElement();
    }

    private static string GetCellReference(int row, int column)
    {
        var columnName = new StringBuilder();
        while (column > 0)
        {
            column--;
            columnName.Insert(0, (char)('A' + column % 26));
            column /= 26;
        }
        return columnName.Append(row).ToString();
    }

    private static void WriteElement(XmlWriter writer, string name, string ns, params string[] attributes)
    {
        writer.WriteStartElement(name, ns);
        for (int i = 0; i < attributes.Length; i += 2)
        {
            writer.WriteAttributeString(attributes[i], attributes[i + 1]);
        }
        writer.WriteEndElement();
    }
}
