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

using CodeBits;

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
//#if DEBUG
//            if (!System.Diagnostics.Debugger.IsAttached)
//            {
//                System.Diagnostics.Debugger.Launch();
//            }
//#endif
            try
            {
                ReadExcelData(context);
            }
#pragma warning disable RCS1075 // Avoid empty catch clause that catches System.Exception.
            catch (Exception)
#pragma warning restore RCS1075 // Avoid empty catch clause that catches System.Exception.
            {
                //context.ReportDiagnostic(Diagnostic.Create(_dataBuilderExtensionErrorDescriptor, Location.None, $"{exception.Message} {exception.InnerException}"));
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
                    using FileStream fs = new(flavour.ExcelPath, FileMode.Open, FileAccess.Read);
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
                        List<int> timestampCols = new();
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

                            (string CSharpAliasType, string DbType) valueTuple = TypeMappings.GetMappings(columnMetaData!.Type);

                            //Store Timestamp columns and remove at last
                            if (valueTuple.DbType == "SqlDbType.Timestamp")
                                timestampCols.Add(j);

                            td.TableColumns.Add(new TableColumns()
                            {
                                Name = cell.ToString(),
                                DbType = valueTuple.DbType,
                                CSharpType = valueTuple.CSharpAliasType,
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
                                //Skip timestamp column data
                                if (timestampCols.Contains(j))
                                    continue;

                                rowList.Add(row.GetCell(j) == null ? "string.Empty" : ConvertObjectValToCSharpType(row.GetCell(j), td.TableColumns[j].Type));
                            }

                            if (rowList.Count > 0)
                                td.DataRows.Add(rowList);
                        }

                        //Remove timestamp columns
                        foreach (int cols in timestampCols)
                        {
                            td.TableColumns.RemoveAt(cols);
                        }

                        flavour.TableData.Add(td);
                    }
                }

                RenderTemplate(context, dataSetup);
            }
        }

        private static string ConvertObjectValToCSharpType(object rowValue, string colType)
        {
            return colType switch
            {
                "binary" => $"BitConverter.GetBytes(Convert.ToUInt64({rowValue}))",
                "image" => $"BitConverter.GetBytes(Convert.ToUInt64({rowValue}))",
                "timestamp" => $"BitConverter.GetBytes(Convert.ToUInt64({rowValue}))",
                "varBinary" => $"BitConverter.GetBytes(Convert.ToUInt64({rowValue}))",
                "bit" => rowValue.ToString() == "0" ? "false" : "true",
                "char" => rowValue.ToString(),
                "nchar" => rowValue.ToString(),
                "ntext" => rowValue.ToString(),
                "nvarchar" => rowValue.ToString(),
                "text" => rowValue.ToString(),
                "varchar" => rowValue.ToString(),
                "xml" => rowValue.ToString(),
                "datetime" => $"DateTime.Parse(\"{rowValue}\")",
                "smalldatetime" => $"DateTime.Parse(\"{rowValue}\")",
                "date" => $"DateTime.Parse(\"{rowValue}\")",
                "time" => $"DateTime.Parse(\"{rowValue}\")",
                "datetime2" => $"DateTime.Parse(\"{rowValue}\")",
                "decimal" => $"{rowValue}",
                "bigint" => $"{rowValue}",
                "money" => $"{rowValue}",
                "smallmoney" => $"{rowValue}",
                "float" => $"{rowValue}",
                "int" => $"{rowValue}",
                "real" => $"{rowValue}",
                "smallint" => $"{rowValue}",
                "tinyint" => $"{rowValue}",
                "uniqueidentifier" => $"new Guid((string){rowValue})",
                "datetimeoffset" => $"DateTimeOffset.Parse((string){rowValue})",
                _ => $"\"{rowValue}\"",
            };
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
            string modelTemplate = await assembly.LoadResourceAsString(type, $"Templates.{templateName}.liquid").ConfigureAwait(false);
            var template = Template.Parse(modelTemplate);
            return template;
        }
    }
}
