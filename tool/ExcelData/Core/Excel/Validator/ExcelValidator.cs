﻿using Datask.Providers;
using Datask.Providers.Schemas;

namespace Datask.Tool.ExcelData.Core.Excel.Validator;

public sealed class ExcelValidator : ProviderExecutor<ExcelValidatorOptions, StatusEvents>
{
    public ExcelValidator(ExcelValidatorOptions options)
        : base(options)
    {
    }

    public IReadOnlyList<ValidationDiff> Diffs { get; private set; } = null!;

    protected override async Task InternalExecuteAsync()
    {
        Diffs = await GetValidationDiffs().ToListAsync();
    }

    private async IAsyncEnumerable<ValidationDiff> GetValidationDiffs()
    {
        DataExcelWorkbook workbook = new(Options.ExcelFilePath.FullName);
        List<DataExcelTable> excelTables = workbook.EnumerateTables().ToList();

        // Get all tables from DB and sort by foreign key dependencies.
        TableDefinitionCollection dbTables = await Provider.SchemaQuery.GetTables(new GetTableOptions
        {
            IncludeColumns = true,
            IncludeForeignKeys = true,
            SortByForeignKeyDependencies = true,
            CustomizeTableName = (schema, table) => new DbObjectName(schema.Replace(" ", "__"), table.Replace(" ", "__")),
        });

        IEnumerable<SequenceDiff> tableDiffs = dbTables.GetSequenceDiffs(excelTables, (dbTable, excelTable) =>
            string.Equals(dbTable.Name.ToString(), excelTable.Name.ToString(), StringComparison.OrdinalIgnoreCase));

        foreach (SequenceDiff tableDiff in tableDiffs)
        {
            if (tableDiff is NewElementDiff<TableDefinition> newElementDiff)
                yield return new NewTableDiff(newElementDiff.Element.Name);
            else if (tableDiff is RemovedElementDiff<DataExcelTable> removedElementDiff)
                yield return new DeletedTableDiff(removedElementDiff.Element.Name);
        }

        // Go through unchanged tables and check the columns.
        foreach (UnchangedElementDiff<TableDefinition, DataExcelTable> unchangedTable in tableDiffs.OfType<UnchangedElementDiff<TableDefinition, DataExcelTable>>())
        {
            IEnumerable<SequenceDiff> columnDiffs = unchangedTable.PrimaryElement.Columns.GetSequenceDiffs(
                unchangedTable.ComparisonElement.Columns,
                (dbCol, excelCol) => string.Equals(dbCol.Name, excelCol.Metadata?.Name, StringComparison.OrdinalIgnoreCase),
                checkOrder: true);

            foreach (SequenceDiff columnDiff in columnDiffs)
            {
                if (columnDiff is NewElementDiff<ColumnDefinition> newColumn)
                    yield return new NewColumnDiff(unchangedTable.PrimaryElement.Name, newColumn.Element.Name);
                else if (columnDiff is RemovedElementDiff<DataExcelTableColumn> removedColumn)
                    yield return new DeletedColumnDiff(unchangedTable.PrimaryElement.Name, removedColumn.Element.Metadata?.Name!);
                else if (columnDiff is IndexChangedElementDiff<ColumnDefinition, DataExcelTableColumn> reorderedColumn)
                {
                    yield return new ReorderedColumnDiff(unchangedTable.PrimaryElement.Name,
                        reorderedColumn.PrimaryElement.Name, reorderedColumn.OldIndex, reorderedColumn.NewIndex);
                }
            }

            foreach (UnchangedElementDiff<ColumnDefinition, DataExcelTableColumn> unchangedColumn in columnDiffs.OfType<UnchangedElementDiff<ColumnDefinition, DataExcelTableColumn>>())
            {
                foreach (ColumnDiff columnDiff in GetColumnMetadataDiffs(unchangedTable.PrimaryElement.Name, unchangedColumn.PrimaryElement, unchangedColumn.ComparisonElement.Metadata!))
                    yield return columnDiff;
            }
        }
    }

    private static IEnumerable<ColumnDiff> GetColumnMetadataDiffs(DbObjectName tableName, ColumnDefinition dbColumn,
        DataExcelTableColumnMetadata excelMetadata)
    {
        if (dbColumn.IsPrimaryKey != excelMetadata.IsPrimaryKey)
        {
            yield return new ChangedColumnMetadataDiff<bool>(tableName, dbColumn.Name,
                ColumnMetadata.IsPrimaryKey, excelMetadata.IsPrimaryKey, dbColumn.IsPrimaryKey);
        }

        if (dbColumn.IsAutoGenerated != excelMetadata.IsAutoGenerated)
        {
            yield return new ChangedColumnMetadataDiff<bool>(tableName, dbColumn.Name,
                ColumnMetadata.IsAutoGenerated, excelMetadata.IsAutoGenerated, dbColumn.IsAutoGenerated);
        }

        if (dbColumn.IsForeignKey != excelMetadata.IsForeignKey)
        {
            yield return new ChangedColumnMetadataDiff<bool>(tableName, dbColumn.Name,
                ColumnMetadata.IsForeignKey, excelMetadata.IsForeignKey, dbColumn.IsForeignKey);
        }

        //TODO:
    }
}
