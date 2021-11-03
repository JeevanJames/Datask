using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using CodeBits;

using Datask.Tool.ExcelData.Core.DbTableSorter;
using Datask.Tool.ExcelData.Core.Events;

using OfficeOpenXml;
using OfficeOpenXml.DataValidation;
using OfficeOpenXml.DataValidation.Contracts;
using OfficeOpenXml.Style;
using OfficeOpenXml.Table;

namespace Datask.Tool.ExcelData.Core
{
    public class DataBuilder
    {
#pragma warning disable S3264 // Events should be invoked
        public event EventHandler<StatusEventArgs<StatusEvents>> OnStatus = null!;
#pragma warning restore S3264 // Events should be invoked

        private readonly DataConfiguration _configuration;

        public DataBuilder(DataConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<bool> ExportExcel()
        {
            using ExcelPackage package = new(_configuration.FilePath);

            return await FillExcelData(package).ConfigureAwait(false);
        }

        private async Task<bool> FillExcelData(ExcelPackage package)
        {
            IEnumerable<TableData>? sortedTables = await TableInfoHelper.GetTableList(_configuration).ConfigureAwait(false);

            sortedTables = FilterSortedTables(sortedTables).ToList();

            if (!sortedTables.Any())
                return false;

            foreach (TableData? tableInfo in sortedTables)
            {
                OnStatus.Fire(StatusEvents.Generate,
                    new { Table = tableInfo.TableName },
                    $"Getting the database table {tableInfo.TableName} information...");

                //Check if the table is added already
                if (package.Workbook.Worksheets.Any(w => w.Tables.Any(t => t.Name == tableInfo.TableName)))
                    continue;

                Random random = new();

                string workSheetName = tableInfo.TableName.Length > 31
                    ? $"{tableInfo.TableName.Substring(0, 24)}...{random.Next(1, 100)}"
                    : tableInfo.TableName;

                ExcelWorksheet? worksheet = package.Workbook.Worksheets.Add(workSheetName);

                for (int i = 0; i < tableInfo.Columns.Count; i++)
                {
                    worksheet.Cells[1, i + 1].Value = tableInfo.Columns[i].Name;
                    worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                    worksheet.Cells[1, i + 1].AutoFitColumns();

                    ApplyDataValidations(tableInfo, i, worksheet, package);
                }

                //Defining the tables parameters
                CreateExcelTables(worksheet, tableInfo);
            }

            package.Save();
            return true;
        }

        private IEnumerable<TableData> FilterSortedTables(IEnumerable<TableData> sortedTables)
        {
            List<TableData> filteredData = new();

            //include the tables
            if (_configuration.IncludeTables.Count > 0)
            {
                foreach (string includeTable in _configuration.IncludeTables)
                {
                    Regex rg = new(includeTable);
                    filteredData.AddRange(sortedTables.Where(tableData => rg.IsMatch(tableData.TableName)));
                }

                return filteredData
                    .DistinctBy(y => y.TableName);

                //sortedTables = (from s in sortedTables
                //    from include in _configuration.IncludeTables
                //    where s.TableName == include
                //    select s).ToList();
            }

            //exclude the tables
            if (_configuration.ExcludeTables.Count > 0)
            {
                foreach (string excludeTable in _configuration.ExcludeTables)
                {
                    Regex rg = new(excludeTable);
                    IEnumerable<TableData> matchTables = sortedTables.Where(tableData => rg.IsMatch(tableData.TableName));

                    //Exclude the matching tables
                    filteredData.AddRange(sortedTables.Where(i => matchTables.All(e => i.TableName != e.TableName)));
                }

                return filteredData
                    .DistinctBy(y => y.TableName);

                //sortedTables = sortedTables.Where(i => _configuration.ExcludeTables.All(e => i.TableName != e));
            }

            return sortedTables;
        }

        private static void CreateExcelTables(ExcelWorksheet worksheet, TableData tableInfo)
        {
            ExcelRange tableRange = worksheet.Cells[1, 1, worksheet.Dimension.End.Row, worksheet.Dimension.End.Column];
            tableRange.Style.Border.Top.Style = ExcelBorderStyle.Medium;
            tableRange.Style.Border.Left.Style = ExcelBorderStyle.Medium;
            tableRange.Style.Border.Right.Style = ExcelBorderStyle.Medium;
            tableRange.Style.Border.Bottom.Style = ExcelBorderStyle.Medium;

            //Adding a table to a Range
            ExcelTable tab = worksheet.Tables.Add(tableRange, tableInfo.TableName);

            //Formatting the table style
            tab.TableStyle = TableStyles.Dark10;
        }

        private static void ApplyDataValidations(TableData tableInfo, int i, ExcelWorksheet worksheet, ExcelPackage package)
        {
            string columnDataRange = ExcelCellBase.GetAddress(2, i + 1, ExcelPackage.MaxRows, i + 1);

            //Primary Key data validation
            //string columnLetter = columnDataRange.Split(':').First().Substring(0, 1);

            //if (tableInfo.Columns[i].IsPrimaryKey && !tableInfo.Columns[i].IsForeignKey)
            //{
            //    var pkCustomDataValidation = worksheet.DataValidations.AddCustomValidation(columnDataRange);

            //    //string columnLetter = columnDataRange.Split(':').First().Substring(0, 1);

            //    string pkValidationFormula = $"=COUNTIF(${columnLetter}2:${columnLetter}{ExcelPackage.MaxRows},{columnLetter}2)=1";

            //    //string pkValidationFormula = $"=COUNTIF(${columnLetter}:${columnLetter}{ExcelPackage.MaxRows},{columnLetter})=1, ISNUMBER(${columnLetter})";
            //    pkCustomDataValidation.ShowErrorMessage = true;
            //    pkCustomDataValidation.Error =
            //        $"Duplicate values are not allowed, its Primary Key.";
            //    pkCustomDataValidation.Formula.ExcelFormula = pkValidationFormula;
            //}

            //Add column metadata into excel column comment section.
            AddColumnMetaData(tableInfo, i, worksheet);

            if (tableInfo.Columns[i].IsForeignKey && !tableInfo.Columns[i].IsPrimaryKey)
            {
                foreach (ExcelWorksheet sheet in package.Workbook.Worksheets)
                {
                    foreach (ExcelTable table in sheet.Tables)
                    {
                        if (table.Name != tableInfo.Columns[i].ReferenceTableName)
                            continue;

                        int? fkColumnPosition = table.Columns[tableInfo.Columns[i].ReferenceColumnName]?.Id;
                        if (fkColumnPosition is null)
                            continue;

                        //var fkCellRange = ExcelRange.GetAddress(2, i + 1, ExcelPackage.MaxRows, i + 1);
                        IExcelDataValidationList? fkDataValidation = worksheet.DataValidations.AddListValidation(columnDataRange);
                        string fkColumnLetter = ExcelCellAddress.GetColumnLetter((int)fkColumnPosition);
                        string validationFormula =
                            $"='{tableInfo.Columns[i].ReferenceTableName}'!${fkColumnLetter}$2:${fkColumnLetter}${ExcelPackage.MaxRows}";

                        fkDataValidation.ShowErrorMessage = true;
                        fkDataValidation.Error = $"The value cannot be empty.";
                        fkDataValidation.Formula.ExcelFormula = validationFormula;
                    }
                }
            }

            if (tableInfo.Columns[i].Type.Equals("datetime", StringComparison.OrdinalIgnoreCase))
            {
                worksheet.Column(i + 1).Style.Numberformat.Format = DateTimeFormatInfo.CurrentInfo.ShortDatePattern;
            }
            else if (tableInfo.Columns[i].Type.Equals("timestamp", StringComparison.OrdinalIgnoreCase))
            {
                worksheet.Column(i + 1).Style.Numberformat.Format = DateTimeFormatInfo.CurrentInfo.ShortTimePattern;
            }
            else if (tableInfo.Columns[i].Type.Equals("bit", StringComparison.OrdinalIgnoreCase))
            {
                //Add data validations for bit values 0/1
                IExcelDataValidationList? bitDataValidation = worksheet.DataValidations.AddListValidation(columnDataRange);

                bitDataValidation.Formula.Values.Add("0");
                bitDataValidation.Formula.Values.Add("1");
            }
            else if (tableInfo.Columns[i].Type.Contains("varchar", StringComparison.OrdinalIgnoreCase) &&
                     tableInfo.Columns[i].MaxLength > 0)
            {
                //Add data validations for maxstring length
                IExcelDataValidationInt? stringLenValidation = worksheet.DataValidations.AddTextLengthValidation(columnDataRange);
                stringLenValidation.ShowErrorMessage = true;
                stringLenValidation.ErrorStyle = ExcelDataValidationWarningStyle.stop;
                stringLenValidation.ErrorTitle = "The value you entered is not valid";
                stringLenValidation.Error =
                    $"This cell must be between 0 and {tableInfo.Columns[i].MaxLength} characters in length.";
                stringLenValidation.Formula.Value = 0;
                stringLenValidation.Formula2.Value = tableInfo.Columns[i].MaxLength;
            }
            else if (tableInfo.Columns[i].Type.Contains("int", StringComparison.OrdinalIgnoreCase) &&
                     !tableInfo.Columns[i].IsForeignKey)
            {
                //Add data validations for integer
                IExcelDataValidationInt? intDataValidation = worksheet.DataValidations.AddIntegerValidation(columnDataRange);

                intDataValidation.ShowErrorMessage = true;
                intDataValidation.Error = "The value must be an integer.";
                intDataValidation.Formula.Value = 0;
                intDataValidation.Formula2.Value = int.MaxValue;
            }
        }

        private static void AddColumnMetaData(TableData tableInfo, int i, ExcelWorksheet worksheet)
        {
            string columnMetadata =
                JsonSerializer.Serialize(tableInfo.Columns[i], new JsonSerializerOptions { WriteIndented = true });

            ExcelComment colComment = worksheet.Cells[1, i + 1].AddComment(columnMetadata, "Owner");
            colComment.AutoFit = true;
        }
    }
}
