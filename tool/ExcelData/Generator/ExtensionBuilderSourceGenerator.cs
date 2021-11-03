using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

using CodeBits;

using Datask.Tool.ExcelData.Generator.Extensions;

using DotLiquid;

using ExcelDataReader;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

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
            //if (!System.Diagnostics.Debugger.IsAttached)
            //{
            //    System.Diagnostics.Debugger.Launch();
            //}
#endif
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
                if (!context.TryReadAdditionalFilesOption(file, "Type", out string? type) || type != "DataBuilder")
                    continue;

                using FileStream? stream = File.Open(file.Path, FileMode.Open, FileAccess.Read);
                using IExcelDataReader? reader = ExcelReaderFactory.CreateReader(stream);
                DataSet result = reader.AsDataSet(new ExcelDataSetConfiguration()
                {
                    ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                    {
                        UseHeaderRow = true,
                    },
                });

                DataModel dataSetup = new();

                //Read testdata namespace
                if (context.TryReadGlobalOption("RootNamespace", out string? testDataNamespace))
                {
                    dataSetup.Namespace = string.IsNullOrEmpty(testDataNamespace) ? "TestData" : testDataNamespace!;
                }

                if (context.TryReadGlobalOption("DataFlavours", out string? dataflavours))
                {
                    if (!string.IsNullOrEmpty(dataflavours))
                    {
                        var flavours = dataflavours!.Split(',').ToList();
                        ((List<string>)dataSetup.DataFlavours).AddRange(flavours.Select(s => s.Trim()));
                    }
                    else
                        ((List<string>)dataSetup.DataFlavours).AddRange(new List<string>() { "Seed", "IntegrationTesting", "Demo" });
                }

                if (context.TryReadGlobalOption("TestDataClassName", out string? className))
                {
                    dataSetup.ClassName = string.IsNullOrEmpty(className) ? dataSetup.Namespace : className!;
                }

                if (context.TryReadGlobalOption("DbContextName", out string? contextName))
                {
                    dataSetup.ContextName = string.IsNullOrEmpty(contextName) ? dataSetup.ContextName : contextName!;
                }

                foreach (DataTable table in result.Tables)
                {
                    TableData tableData = new()
                    {
                        Name = table.TableName.Split('.').Skip(1).First(),
                    };

                    var columnNames = table.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToList();

                    for (int index = 0; index < columnNames.Count; index++)
                    {
                        string[] columnNameType = columnNames[index].Split(' ');
                        string sqlColType = columnNameType.Skip(1).First().Split('(', ')')[1];
                        TableColumns tableColumns = new()
                        {
                            Name = columnNameType.Take(1).First(),
                            Type = ConvertToCSharpType(sqlColType),
                        };

#pragma warning disable S3267 // Loops should be simplified with "LINQ" expressions
                        foreach (DataRow row in table.Rows)
#pragma warning restore S3267 // Loops should be simplified with "LINQ" expressions
                        {
                            object? rowValue = row.ItemArray[index].GetType().FullName == "System.DBNull"
                                ? "null"
                                : row.ItemArray[index];
                            tableColumns.ColumnRows.Add(ConvertObjectValToCSharpType(rowValue, sqlColType));
                        }

                        tableData.TableColumns.Add(tableColumns);
                    }

                    dataSetup.TableData.Add(tableData);
                }

                RenderTemplate(context, dataSetup);
            }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502", Justification = "To be seperated later.")]
        private static object ConvertObjectValToCSharpType(object rowValue, string colType)
        {
#pragma warning disable CA1305 // Specify IFormatProvider
            switch (colType)
            {
                case "bigint":
                    return (long?)rowValue;

                case "binary":
                case "image":
                case "timestamp":
                case "varBinary":
                    return ObjectToByteArray(rowValue);

                case "bit":
                    return Convert.ToBoolean(rowValue);

                case "char":
                case "nchar":
                case "ntext":
                case "nvarchar":
                case "text":
                case "varchar":
                case "xml":
                    return (string)rowValue;

                case "datetime":
                case "smalldatetime":
                case "date":
                case "time":
                case "datetime2":
                    return DateTime.Parse((string)rowValue);

                case "decimal":
                case "money":
                case "smallmoney":
                    return Convert.ToDecimal(rowValue);

                case "float":
                    return Convert.ToDouble(rowValue);

                case "int":
                    return Convert.ToInt32(rowValue);

                case "real":
                    return (float?)rowValue;

                case "uniqueidentifier":
                    return new Guid((string)rowValue);

                case "smallint":
                    return (short?)rowValue;

                case "tinyint":
                    return Convert.ToByte(rowValue);

                case "datetimeoffset":
                    return DateTimeOffset.Parse((string)rowValue);

                default:
                    return (string)rowValue;
            }
#pragma warning restore CA1305 // Specify IFormatProvider
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502", Justification = "To be seperated later.")]
        private static string ConvertToCSharpType(string colType)
        {
            switch (colType)
            {
                case "bigint":
                    return typeof(long?).Name;

                case "binary":
                case "image":
                case "timestamp":
                case "varBinary":
                    return typeof(byte[]).Name;

                case "bit":
                    return nameof(Boolean);

                case "char":
                case "nchar":
                case "ntext":
                case "nvarchar":
                case "text":
                case "varchar":
                case "xml":
                    return nameof(String);

                case "datetime":
                case "smalldatetime":
                case "date":
                case "time":
                case "datetime2":
                    return nameof(DateTime);

                case "decimal":
                case "money":
                case "smallmoney":
                    return nameof(Decimal);

                case "float":
                case "real":
                    return nameof(Single);

                case "int":
                    return nameof(Int32);

                case "uniqueidentifier":
                    return nameof(Guid);

                case "smallint":
                    return nameof(Int16);

                case "tinyint":
                    return nameof(Byte);

                case "datetimeoffset":
                    return nameof(DateTimeOffset);

                default:
                    return nameof(String);
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
