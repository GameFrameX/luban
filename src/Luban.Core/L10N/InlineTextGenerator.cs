using System.IO.Compression;
using System.Text;
using System.Xml;

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

        string manifestPath = Path.Combine(outputPath, ".inline-text-manifest");
        var generatedFiles = new List<string>();
        var generatedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in collection.EntriesByTable.OrderBy(x => x.Key, StringComparer.Ordinal))
        {
            string fileName = fileNameFormat.Replace("{table}", SanitizeFileName(pair.Key));
            if (!generatedPaths.Add(fileName))
            {
                throw new Exception($"内联本地化表名生成文件名冲突。表:{pair.Key} 文件:{fileName}");
            }
            string fullPath = Path.Combine(outputPath, fileName);
            var rows = new List<string[]>
            {
                new[] { "##var", "key", language },
                new[] { "##var", "", "" },
                new[] { "##type", "string", "string" },
                new[] { "##group", "", "" },
                new[] { "##", "key", language },
                new[] { "##", "", "" },
            };
            rows.AddRange(pair.Value.OrderBy(x => x.Key, StringComparer.Ordinal).Select(x => new[] { "", x.Key, x.Value }));
            InlineTextXlsxWriter.Write(fullPath, rows);
            generatedFiles.Add(fileName);
        }

        if (File.Exists(manifestPath))
        {
            string fullOutputPath = Path.GetFullPath(outputPath) + Path.DirectorySeparatorChar;
            var currentFiles = generatedFiles.ToHashSet(StringComparer.OrdinalIgnoreCase);
            foreach (string oldFile in File.ReadAllLines(manifestPath).Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                string safePath = Path.Combine(outputPath, oldFile);
                if (!currentFiles.Contains(oldFile) && Path.GetFullPath(safePath).StartsWith(fullOutputPath, StringComparison.Ordinal) && File.Exists(safePath))
                {
                    File.Delete(safePath);
                }
            }
        }
        File.WriteAllLines(manifestPath, generatedFiles, new UTF8Encoding(false));
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

}

internal static class InlineTextXlsxWriter
{
    private const int FirstColumn = 1;
    private const string SpreadsheetNamespace = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
    private const string RelationshipNamespace = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
    private const string PackageRelationshipNamespace = "http://schemas.openxmlformats.org/package/2006/relationships";
    private const string ContentTypesNamespace = "http://schemas.openxmlformats.org/package/2006/content-types";

    public static void Write(string path, IReadOnlyList<string[]> rows)
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
        writer.WriteAttributeString("ref", $"A1:{GetCellReference(rows.Count, FirstColumn + rows.Max(row => row.Length) - 1)}");
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
