using System.Data;
using System.Reflection;
using System.Text.Json;

using CodeBits;

using Datask.Providers;
using Datask.Providers.SqlServer;

using DotLiquid;

using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace Datask.Tool.ExcelData.Core;

public sealed class DataExtensionBuilder
{
#pragma warning disable S3264 // Events should be invoked
    public event EventHandler<StatusEventArgs<StatusEvents>> OnStatus = null!;
#pragma warning restore S3264 // Events should be invoked

    private readonly DataHelperConfiguration _configuration;

    public DataExtensionBuilder(DataHelperConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task BuildDataExtensionAsync()
    {
        if (_configuration.Flavors is null)
            return;

        string filePath = _configuration.FilePath;
        RegisterTypes();
        File.WriteAllText(filePath, await RenderTemplate("PopulateDataTemplate", _configuration).ConfigureAwait(false));

        foreach (Flavors? flavour in _configuration.Flavors)
        {
            OnStatus.Fire(StatusEvents.Generate,
                new { Flavor = flavour.Name },
                "Generating data helper for {Flavor} information.");

            File.AppendAllText(filePath, await RenderTemplate("PopulateFlavorDataTemplate", flavour.Name).ConfigureAwait(false));
            using FileStream fs = new(flavour.ExcelPath, FileMode.Open, FileAccess.Read);
            IWorkbook xssWorkbook = new XSSFWorkbook(fs);

            int noOfWorkSheets = xssWorkbook.NumberOfSheets;

            for (int index = 0; index < noOfWorkSheets; index++)
            {
                var sheet = (XSSFSheet)xssWorkbook.GetSheetAt(index);

                List<XSSFTable> xssfTables = sheet.GetTables();
                if (!xssfTables.Any())
                    continue;

                string[] tableName = xssfTables.First().DisplayName.Split('.');
                TableDefinition td = new(tableName.Skip(1).First(), tableName.Take(1).First());

                FillTableData(sheet, td, out List<int> timestampCols, out int cellCount);

                IList<List<string?>> dataRows = FillDataRows(sheet, td, timestampCols, cellCount);

                //Remove timestamp columns
                foreach (int cols in timestampCols)
                {
                    td.Columns.RemoveAt(cols);
                }

                flavour.TableDefinitions.Add(td);

                File.AppendAllText(filePath, await RenderTemplate("PopulateTableDataTemplate", new
                {
                    table = td,
                    dr = dataRows,
                    ic = td.Columns.Any(c => c.IsIdentity),
                }).ConfigureAwait(false));
            }

            File.AppendAllText(filePath, await RenderTemplate("PopulateConsolidatedDataTemplate", flavour.TableDefinitions
                .Select(t => $"{t.Schema}{t.Name}")
                .ToList()).ConfigureAwait(false));
        }

        File.AppendAllText(filePath, "}");
    }

    private static void FillTableData(XSSFSheet sheet, TableDefinition td, out List<int> timestampCols, out int cellCount)
    {
        IRow headerRow = sheet.GetRow(0);
        timestampCols = new();
        cellCount = headerRow.LastCellNum;
        for (int j = 0; j < cellCount; j++)
        {
            ICell cell = headerRow.GetCell(j);
            if (cell == null || string.IsNullOrWhiteSpace(cell.ToString()))
                continue;

            string? cellComment = cell.CellComment.String.ToString();
            if (cellComment is null)
                continue;

            Dictionary<string, object>? columnMetaData = JsonSerializer.Deserialize<Dictionary<string, object>>(cellComment, new JsonSerializerOptions
            {
                WriteIndented = true,
            });

            if (columnMetaData is null)
                continue;

            SqlDbType sqlDbType = DbTypeMapping.GetSqlDbTypeMapping(columnMetaData["NativeType"].ToString());

            //Store Timestamp columns and remove at last
            if (sqlDbType == SqlDbType.Timestamp)
                timestampCols.Add(j);

            td.Columns.Add(new ColumnDefinition(cell.ToString()!)
            {
                DatabaseType = $"SqlDbType.{sqlDbType.ToString()}",
                //Type = valueTuple.CSharpType,
                CSharpType = columnMetaData["Type"].ToString(),
                IsPrimaryKey = Convert.ToBoolean(columnMetaData["IsPrimaryKey"].ToString()),
                IsNullable = Convert.ToBoolean(columnMetaData["IsNullable"].ToString()),
                IsIdentity = Convert.ToBoolean(columnMetaData["IsIdentity"].ToString()),
                MaxLength = Convert.ToInt32(columnMetaData["MaxLength"].ToString()),
            });
        }
    }

    private static IList<List<string?>> FillDataRows(XSSFSheet sheet, TableDefinition td, List<int> timestampCols, int cellCount)
    {
        IList<List<string?>> dataRows = new List<List<string?>>();

        for (int i = sheet.FirstRowNum + 1; i <= sheet.LastRowNum; i++)
        {
            List<string?> rowList = new();
            IRow row = sheet.GetRow(i);
            if (row == null)
                continue;
            if (row.Cells.All(d => d.CellType == CellType.Blank))
                continue;

            for (int j = row.FirstCellNum; j < cellCount; j++)
            {
                //Skip timestamp column data
                if (timestampCols.Contains(j))
                    continue;

                rowList.Add(row.GetCell(j) == null ? "string.Empty" :
                    ConvertObjectValToCSharpType(row.GetCell(j), td.Columns[j].DatabaseType));
            }

            if (rowList.Count > 0)
                dataRows.Add(rowList);
        }

        return dataRows;
    }

    private static string ConvertObjectValToCSharpType(object rowValue, string colType)
    {
        return colType switch
        {
            "SqlDbType.Binary" => $"BitConverter.GetBytes(Convert.ToUInt64({rowValue}))",
            "SqlDbType.Image" => $"BitConverter.GetBytes(Convert.ToUInt64({rowValue}))",
            "SqlDbType.Timestamp" => $"BitConverter.GetBytes(Convert.ToUInt64({rowValue}))",
            "SqlDbType.VarBinary" => $"BitConverter.GetBytes(Convert.ToUInt64({rowValue}))",
            "SqlDbType.Bit" => rowValue.ToString() == "0" ? "false" : "true",
            "SqlDbType.Char" => rowValue.ToString(),
            "SqlDbType.NChar" => rowValue.ToString(),
            "SqlDbType.NText" => rowValue.ToString(),
            "SqlDbType.NVarChar" => rowValue.ToString(),
            "SqlDbType.Text" => rowValue.ToString(),
            "SqlDbType.VarChar" => rowValue.ToString(),
            "SqlDbType.Xml" => rowValue.ToString(),
            "SqlDbType.DateTime" => $"DateTime.Parse(\"{rowValue}\")",
            "SqlDbType.SmallDateTime" => $"DateTime.Parse(\"{rowValue}\")",
            "SqlDbType.Date" => $"DateTime.Parse(\"{rowValue}\")",
            "SqlDbType.Time" => $"DateTime.Parse(\"{rowValue}\")",
            "SqlDbType.DateTime2" => $"DateTime.Parse(\"{rowValue}\")",
            "SqlDbType.Decimal" => $"{rowValue}",
            "SqlDbType.BigInt" => $"{rowValue}",
            "SqlDbType.Money" => $"{rowValue}",
            "SqlDbType.SmallMoney" => $"{rowValue}",
            "SqlDbType.Float" => $"{rowValue}",
            "SqlDbType.Int" => $"{rowValue}",
            "SqlDbType.Real" => $"{rowValue}",
            "SqlDbType.SmallInt" => $"{rowValue}",
            "SqlDbType.TinyInt" => $"{rowValue}",
            "SqlDbType.UniqueIdentifier" => $"new Guid((string){rowValue})",
            "SqlDbType.DateTimeOffset" => $"DateTimeOffset.Parse((string){rowValue})",
            _ => $"\"{rowValue}\"",
        };
    }

    private async Task<string> RenderTemplate(string templateName, object modelData)
    {
        Template template = await ParseTemplate(templateName, Assembly.GetExecutingAssembly(), GetType());
        return template.Render(Hash.FromAnonymousObject(new
        {
            model = modelData,
        }));
    }

    private static void RegisterTypes()
    {
        Template.RegisterSafeType(typeof(Type),
                    typeof(Type).GetProperties().Select(p => p.Name).ToArray());
        Template.RegisterSafeType(typeof(DataHelperConfiguration),
                    typeof(DataHelperConfiguration).GetProperties().Select(p => p.Name).ToArray());
        Template.RegisterSafeType(typeof(Flavors),
            typeof(Flavors).GetProperties().Select(p => p.Name).ToArray());
        Template.RegisterSafeType(typeof(TableDefinition),
            typeof(TableDefinition).GetProperties().Select(p => p.Name).ToArray());
        Template.RegisterSafeType(typeof(ColumnDefinition),
            typeof(ColumnDefinition).GetProperties().Select(p => p.Name).ToArray());
    }

    /// <summary>
    /// Parse Template.
    /// </summary>
    /// <param name="templateName">Template name.</param>
    /// <param name="assembly">Executing assembly.</param>
    /// <param name="type">object type.</param>
    /// <returns>Template.</returns>
    private static async Task<Template> ParseTemplate(string templateName, Assembly assembly, Type type)
    {
        Stream resourceStream = assembly.GetManifestResourceStream(type, $"Templates.{templateName}.liquid");
        using StreamReader reader = new(resourceStream);
        string modelTemplate = await reader.ReadToEndAsync();
        return Template.Parse(modelTemplate);
    }
}
