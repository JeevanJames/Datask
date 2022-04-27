using System.Data;

using Datask.Providers.Schemas;

using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;

namespace Datask.Tool.ExcelData.Core;

/// <summary>
///     Provides an object model for an Excel workbook that contains database data.
/// </summary>
public sealed class DataExcelWorkbook : IDisposable
{
    private readonly IWorkbook _workbook;

    public DataExcelWorkbook(string filePath)
    {
        _workbook = new XSSFWorkbook(filePath);
    }

    public void Dispose()
    {
        _workbook.Close();
    }

    public IEnumerable<TableDefinition> EnumerateTables()
    {
        for (int i = 0; i < _workbook.NumberOfSheets; i++)
        {
            var worksheet = (XSSFSheet)_workbook.GetSheetAt(i);

            List<XSSFTable> wsTables = worksheet.GetTables();
            if (wsTables.Count == 0)
                throw new InvalidOperationException($"Worksheet {worksheet.SheetName} does not contain a table.");
            if (wsTables.Count > 1)
                throw new InvalidOperationException($"Worksheet {worksheet.SheetName} has more than one tables.");

            XSSFTable wsTable = wsTables[0];
            if (!TableDefinition.TryParse(wsTable.Name, out TableDefinition tableDefn))
            {
                throw new InvalidOperationException(
                    $"Worksheet {worksheet.SheetName} table {wsTable.Name} cannot be parsed as a table name.");
            }

            IEnumerable<ColumnDefinition> columnDefns = EnumerateColumns(worksheet, wsTable);
            foreach (ColumnDefinition columnDefn in columnDefns)
            {
                tableDefn.Columns.Add(columnDefn);
            }

            yield return tableDefn;
        }
    }

    private IEnumerable<ColumnDefinition> EnumerateColumns(XSSFSheet worksheet, XSSFTable wsTable)
    {
        CellReference startRef = wsTable.GetStartCellReference();
        CellReference endRef = wsTable.GetEndCellReference();

        IRow headerRow = worksheet.GetRow(startRef.Row);
        for (int colIdx = startRef.Col; colIdx < endRef.Col; colIdx++)
        {
            ICell headerCell = headerRow.GetCell(colIdx);
            yield return new ColumnDefinition(headerCell.StringCellValue)
            {
                ClrType = typeof(string), DatabaseType = "varchar", DbType = DbType.AnsiString,
            };
        }
    }
}
