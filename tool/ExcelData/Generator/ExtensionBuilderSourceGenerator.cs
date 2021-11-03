using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Datask.Tool.ExcelData.Generator.Extensions;
using DotLiquid;
using ExcelDataReader;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace Datask.Tool.ExcelData.Generator
{
    [Generator]
    public class ExtensionBuilderSourceGenerator : ISourceGenerator
    {
#pragma warning disable RS2008 // Enable analyzer release tracking
        private static readonly DiagnosticDescriptor _dataBuilderExtensionErrorDescriptor = new(id: "DBE001",
#pragma warning restore RS2008 // Enable analyzer release tracking
            title: "Error in creating data builder extension methods",
            messageFormat: "Error in creating data builder extension methods : '{0}'",
            category: "DataBuilderExtension",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public void Initialize(GeneratorInitializationContext context)
        {
            //Initialization not required.
        }

        public void Execute(GeneratorExecutionContext context)
        {
#if DEBUG
            if (!System.Diagnostics.Debugger.IsAttached)
            {
                System.Diagnostics.Debugger.Launch();
            }
#endif
            try
            {
                ReadExcelData(context);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception exception)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                context.ReportDiagnostic(Diagnostic.Create(_dataBuilderExtensionErrorDescriptor, Location.None, $"{exception.Message} {exception.InnerException}"));
            }
        }

        private void ReadExcelData(GeneratorExecutionContext context)
        {
            //Read Excel data
            foreach (AdditionalText? file in context.AdditionalFiles)
            {
                if (!context.TryReadAdditionalFilesOption(file, "Type", out string? type) || type != "DataBuilderConfiguration")
                    continue;

                //Parse config file and fill the data model
                string configurationJson = File.ReadAllText(file.Path);
                DataModel? dataSetup = JsonSerializer.Deserialize<DataModel>(configurationJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    AllowTrailingCommas = true,
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    Converters =
                    {
                        new JsonStringEnumConverter(JsonNamingPolicy.CamelCase),
                    },
                });

                if (dataSetup?.Flavours is null)
                    return;

                foreach (Flavours? flavour in dataSetup.Flavours)
                {
                    using (FileStream fs = new(flavour.ExcelPath, FileMode.Open, FileAccess.Read))
                    {
                        IWorkbook xssWorkbook = new XSSFWorkbook(fs);

                        int noOfWorkSheets = xssWorkbook.NumberOfSheets;

                        for (int index = 0; index < noOfWorkSheets; index++)
                        {
                            TableData td = new();

                            var sheet = (XSSFSheet)xssWorkbook.GetSheetAt(index);

                            List<XSSFTable> xssfTables = sheet.GetTables();
                            if (xssfTables.Any())
                            {
                                string[] tableName = xssfTables.First().DisplayName.Split('.');
                                td.Name = tableName.Skip(1).First();
                                td.Schema = tableName.Take(1).First();
                            }

                            IRow headerRow = sheet.GetRow(0);
                            int cellCount = headerRow.LastCellNum;
                            for (int j = 0; j < cellCount; j++)
                            {
                                ICell cell = headerRow.GetCell(j);
                                if (cell == null || string.IsNullOrWhiteSpace(cell.ToString()))
                                    continue;

                                string cellComment = sheet.GetCellComment(cell.Address).String.ToString();
                                TableColumns? columnMetaData = JsonSerializer.Deserialize<TableColumns>(cellComment, new JsonSerializerOptions()
                                {
                                    AllowTrailingCommas = true,
                                    PropertyNameCaseInsensitive = true,
                                });

                                td.TableColumns.Add(new TableColumns()
                                {
                                    Name = cell.ToString(),
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                                    DbType = ConvertToSqlDbType(columnMetaData.Type),
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                                    CSharpType = GetCSharpType(ConvertToSqlDbType(columnMetaData.Type)),
                                    Type = columnMetaData.Type,
                                    IsIdentity = columnMetaData.IsIdentity,
                                });

                                if (columnMetaData.IsIdentity)
                                    td.ContainsIdentityColumn = true;
                            }

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
                                    rowList.Add(row.GetCell(j) == null ? "string.Empty" : ConvertObjectValToCSharpType(row.GetCell(j).ToString(), td.TableColumns[j].Type));

                                    //rowList.Add(row.GetCell(j)?.ToString());

                                    //if (td.TableColumns[j].DbType == Contains("varchar") && string.IsNullOrEmpty(row.GetCell(j)?.ToString()))
                                    //    rowList.Add(string.Empty);
                                    //else
                                    //    rowList.Add(row.GetCell(j));
                                }

                                if (rowList.Count > 0)
                                    td.DataRows.Add(rowList);
                            }

                            flavour.TableData.Add(td);
                        }
                    }

//                    using FileStream? stream = File.Open(flavour.ExcelPath, FileMode.Open, FileAccess.Read);
//                    using IExcelDataReader? reader = ExcelReaderFactory.CreateReader(stream);
//                    DataSet result = reader.AsDataSet(new ExcelDataSetConfiguration()
//                    {
//                        ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
//                        {
//                            UseHeaderRow = true,
//                        },
//                    });
//                    foreach (DataTable table in result.Tables)
//                    {
//                        TableData tableData = new()
//                        {
//                            Name = table.TableName.Split('.').Skip(1).First(),
//                        };

//                        for (int index = 0; index < table.Columns.Count; index++)
//                        {
//                            TableColumns tableColumns = new()
//                            {
//                                Name = table.Columns[index].ColumnName,

//                                //Type = table.Columns[index].ty,
//                            };

//#pragma warning disable S3267 // Loops should be simplified with "LINQ" expressions
//                            foreach (DataRow row in table.Rows)
//#pragma warning restore S3267 // Loops should be simplified with "LINQ" expressions
//                            {
//                                object? rowValue = row.ItemArray[index].GetType().FullName == "System.DBNull"
//                                    ? "null"
//                                    : row.ItemArray[index];
//                                tableColumns.ColumnRows.Add(rowValue);

//                                //tableColumns.ColumnRows.Add(ConvertObjectValToCSharpType(rowValue, sqlColType));
//                            }

//                            tableData.TableColumns.Add(tableColumns);
//                        }

//                        flavour.TableData.Add(tableData);
//                    }
                }

                RenderTemplate(context, dataSetup);
            }
        }

        public static string GetCSharpType(string sqlType)
        {
            switch (sqlType)
            {
                case "SqlDbType.BigInt":
                    return "long";

                case "SqlDbType.Binary":
                case "SqlDbType.Image":
                case "SqlDbType.Timestamp":
                case "SqlDbType.VarBinary":
                    return "byte[]";

                case "SqlDbType.Bit":
                    return "bool";

                case "SqlDbType.Char":
                case "SqlDbType.NChar":
                case "SqlDbType.NText":
                case "SqlDbType.NVarChar":
                case "SqlDbType.Text":
                case "SqlDbType.VarChar":
                case "SqlDbType.Xml":
                    return "string";

                case "SqlDbType.DateTime":
                case "SqlDbType.SmallDateTime":
                case "SqlDbType.Date":
                case "SqlDbType.Time":
                case "SqlDbType.DateTime2":
                    return "DateTime";

                case "SqlDbType.Decimal":
                case "SqlDbType.Money":
                case "SqlDbType.SmallMoney":
                    return "decimal";

                case "SqlDbType.Float":
                    return "double";

                case "SqlDbType.Int":
                    return "int";

                case "SqlDbType.Real":
                    return "float";

                case "SqlDbType.UniqueIdentifier":
                    return "Guid";

                case "SqlDbType.SmallInt":
                    return "short";

                case "SqlDbType.TinyInt":
                    return "byte";

                case "SqlDbType.Variant":
                case "SqlDbType.Udt":
                    return "object";

                case "SqlDbType.Structured":
                    return "DataTable";

                case "SqlDbType.DateTimeOffset":
                    return "DateTimeOffset";

                default:
                    throw new ArgumentOutOfRangeException("sqlType");
            }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502", Justification = "To be seperated later.")]
        private static string ConvertObjectValToCSharpType(object rowValue, string colType)
        {
#pragma warning disable CA1305 // Specify IFormatProvider
            switch (colType)
            {
                case "binary":
                case "image":
                case "timestamp":
                case "varBinary":
                    return $"BitConverter.GetBytes(Convert.ToUInt64({rowValue}))";

                case "bit":
                    return rowValue.ToString() == "0" ? "false" : "true";

                case "char":
                case "nchar":
                case "ntext":
                case "nvarchar":
                case "text":
                case "varchar":
                case "xml":
                    return rowValue.ToString();

                case "datetime":
                case "smalldatetime":
                case "date":
                case "time":
                case "datetime2":
                    return $"DateTime.Parse(\"{rowValue}\")";

                case "decimal":
                case "bigint":
                case "money":
                case "smallmoney":
                case "float":
                case "int":
                case "real":
                case "smallint":
                case "tinyint":
                    return $"{rowValue}";

                case "uniqueidentifier":
                    return $"new Guid((string){rowValue})";

                case "datetimeoffset":
                    return $"DateTimeOffset.Parse((string){rowValue})";

                default:
                    return $"\"{rowValue}\"";
            }
#pragma warning restore CA1305 // Specify IFormatProvider
        }

        //[SuppressMessage("Microsoft.Maintainability", "CA1502", Justification = "To be seperated later.")]
        //private static string ConvertToCSharpType(string colType)
        //{
        //    switch (colType)
        //    {
        //        case "bigint":
        //            return typeof(long?).Name;

        //        case "binary":
        //        case "image":
        //        case "timestamp":
        //        case "varBinary":
        //            return typeof(byte[]).Name;

        //        case "bit":
        //            return nameof(Boolean);

        //        case "char":
        //        case "nchar":
        //        case "ntext":
        //        case "nvarchar":
        //        case "text":
        //        case "varchar":
        //        case "xml":
        //            return nameof(String);

        //        case "datetime":
        //        case "smalldatetime":
        //        case "date":
        //        case "time":
        //        case "datetime2":
        //            return nameof(DateTime);

        //        case "decimal":
        //        case "money":
        //        case "smallmoney":
        //            return nameof(Decimal);

        //        case "float":
        //        case "real":
        //            return nameof(Single);

        //        case "int":
        //            return nameof(Int32);

        //        case "uniqueidentifier":
        //            return nameof(Guid);

        //        case "smallint":
        //            return nameof(Int16);

        //        case "tinyint":
        //            return nameof(Byte);

        //        case "datetimeoffset":
        //            return nameof(DateTimeOffset);

        //        default:
        //            return nameof(String);
        //    }
        //}

        [SuppressMessage("Microsoft.Maintainability", "CA1502", Justification = "To be seperated later.")]
        private static string ConvertToSqlDbType(string colType)
        {
            switch (colType)
            {
                case "bigint":
                    return "SqlDbType.BigInt";

                case "binary":
                case "image":
                case "varbinary":
                    return "SqlDbType.VarBinary";

                case "bit":
                    return "SqlDbType.Bit";

                case "char":
                    return "SqlDbType.Char";

                case "date":
                    return "SqlDbType.Date";

                case "datetime":
                case "smalldatetime":
                    return "SqlDbType.DateTime";

                case "datetime2":
                    return "SqlDbType.DateTime2";

                case "datetimeoffset":
                    return "SqlDbType.DateTimeOffset";

                case "decimal":
                case "numeric":
                    return "SqlDbType.Decimal";

                case "float":
                    return "SqlDbType.Float";

                case "int":
                    return "SqlDbType.Int";

                case "money":
                    return "SqlDbType.Money";

                case "nchar":
                    return "SqlDbType.NChar";

                case "ntext":
                    return "SqlDbType.NText";

                case "nvarchar":
                    return "SqlDbType.NVarChar";

                case "real":
                    return "SqlDbType.Real";

                case "rowversion":
                case "timestamp":
                    return "SqlDbType.Timestamp";

                case "smallint":
                    return "SqlDbType.SmallInt";

                case "smallmoney":
                    return "SqlDbType.SmallMoney";

                case "sql_variant":
                    return "SqlDbType.Variant";

                case "text":
                    return "SqlDbType.Text";

                case "time":
                    return "SqlDbType.Time";

                case "tinyint":
                    return "SqlDbType.TinyInt";

                case "uniqueidentifier":
                    return "SqlDbType.UniqueIdentifier";

                case "varchar":
                    return "SqlDbType.VarChar";

                case "xml":
                    return "SqlDbType.Xml";

                default:
                    return "SqlDbType.NVarChar";
            }
        }

        private static byte[] ObjectToByteArray(object obj)
        {
            if (obj == null)
                return default!;

            BinaryFormatter bf = new();
            using MemoryStream ms = new();
            bf.Serialize(ms, obj);
            return ms.ToArray();
        }

        private void RenderTemplate(GeneratorExecutionContext context, DataModel dataModel)
        {
            Template.RegisterSafeType(typeof(DataModel),
                typeof(DataModel).GetProperties().Select(p => p.Name).ToArray());
            Template.RegisterSafeType(typeof(Flavours),
                typeof(Flavours).GetProperties().Select(p => p.Name).ToArray());
            Template.RegisterSafeType(typeof(TableData),
                typeof(TableData).GetProperties().Select(p => p.Name).ToArray());
            Template.RegisterSafeType(typeof(TableColumns),
                typeof(TableColumns).GetProperties().Select(p => p.Name).ToArray());

            Template template = ParseTemplate("DataExtensionTemplate", Assembly.GetExecutingAssembly(), GetType())
                .GetAwaiter().GetResult();
            string response = template.Render(Hash.FromAnonymousObject(new
            {
                d = dataModel,
            }));

            context.AddSource($"{dataModel.ClassName}.cs", SourceText.From(response, Encoding.UTF8));
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
            string modelTemplate = await assembly.LoadResourceAsTextAsync(type, $"Templates.{templateName}.liquid").ConfigureAwait(false);
            var template = Template.Parse(modelTemplate);
            return template;
        }
    }
}
