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

        // Get all tables from DB and sort by foreign key dependencies.
        TableDefinitionCollection dbTables = await Provider.SchemaQuery.GetTables(new GetTableOptions
        {
            IncludeColumns = true,
            IncludeForeignKeys = true,
            SortByForeignKeyDependencies = true,
            CustomizeTableName = (schema, table) => new DbObjectName(schema.Replace(" ", "__"), table.Replace(" ", "__")),
        });

        var excelTables = workbook.EnumerateTables().ToList();

        // Go through each database table and validate.
        foreach (TableDefinition dbTable in dbTables)
        {
            // Find the corresponding table in the Excel file.
            DataExcelTable? excelTable = excelTables.FirstOrDefault(et => string.Equals(dbTable.Name.ToString(),
                et.Name.ToString(), StringComparison.OrdinalIgnoreCase));

            // If the table is not found in the Excel file, then it's probably a newly-added table.
            // No need to process the table further.
            if (excelTable is null)
            {
                yield return new NewTableDiff(dbTable.Name);
                continue;
            }

            // Check if any Excel column has missing or invalid metadata.
            foreach (DataExcelTableColumn excelColumn in excelTable.Columns)
            {
                if (excelColumn.Metadata is null)
                    yield return new MissingColumnMetadataDiff(dbTable.Name, excelColumn.Text);
            }

            // Now process the columns.
            foreach (ColumnDefinition dbColumn in dbTable.Columns)
            {
                // Find the corresponding column in the Excel file
                DataExcelTableColumn? excelColumn = excelTable.Columns.FirstOrDefault(
                    etc => string.Equals(dbColumn.Name, etc.Metadata?.Name, StringComparison.OrdinalIgnoreCase));

                // If the column is not found in the Excel file, then it's probably a newly-added column.
                // No need to process the table further.
                if (excelColumn is null)
                {
                    yield return new NewColumnDiff(dbTable.Name, dbColumn.Name);
                    continue;
                }

                if (excelColumn.Metadata is null)
                    continue;

                // Compare the metadata
                foreach (ColumnDiff columnDiff in GetColumnMetadataDiffs(dbTable.Name, dbColumn, excelColumn.Metadata))
                    yield return columnDiff;
            }
        }

        IEnumerable<DataExcelTable> newExcelTables = excelTables.Where(et => !dbTables.Any(dbt =>
            string.Equals(et.Name.ToString(), dbt.Name.ToString(), StringComparison.OrdinalIgnoreCase)));
        foreach (DataExcelTable newExcelTable in newExcelTables)
            yield return new DeletedTableDiff(newExcelTable.Name);
    }

    private IEnumerable<ColumnDiff> GetColumnMetadataDiffs(DbObjectName tableName, ColumnDefinition dbColumn,
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
