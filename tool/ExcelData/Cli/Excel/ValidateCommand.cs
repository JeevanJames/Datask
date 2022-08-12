// Copyright (c) 2021 Jeevan James
// This file is licensed to you under the MIT License.
// See the LICENSE file in the project root for more information.

using Datask.Providers.SqlServer;
using Datask.Tool.ExcelData.Core.Excel.Validator;

namespace Datask.Tool.ExcelData.Excel;

[Command("validate", ParentType = typeof(ExcelCommand))]
[CommandHelp("Validates an Excel workbook against a database")]
public sealed class ValidateCommand : BaseCommand
{
    [Argument(Order = 0)]
    [ArgumentHelp("excel file", "The path to the Excel workbook to create.")]
    public FileInfo ExcelFile { get; set; } = null!;

    [Argument(Order = 1)]
    [ArgumentHelp("connection string", "The connection string to the database to create the Excel file from.")]
    public string ConnectionString { get; set; } = null!;

    [Flag("fix")]
    [FlagHelp("Start an interactive session to fix validation issues.")]
    public bool Fix { get; set; }

    protected override Task<int> ExecuteAsync(StatusContext ctx, IParseResult parseResult)
    {
        AnsiConsole.MarkupLine($"Excel file path  : {ExcelFile.FullName}");
        AnsiConsole.MarkupLine($"Connection string: {ConnectionString}");
        AnsiConsole.MarkupLine($"Fix errors       : {Fix}");
        return Task.FromResult(0);
    }

    protected override async Task<int> PostExecuteAsync(int executeResult, IParseResult parseResult)
    {
        ExcelValidatorOptions options = new(typeof(SqlServerProvider), ConnectionString, ExcelFile);
        ExcelValidator validator = new(options);
        await validator.ExecuteAsync().ConfigureAwait(false);

        AnsiConsole.MarkupLine($"Differences found: {validator.Diffs.Count}");

        foreach (TableDiff tableDiff in validator.Diffs.OfType<TableDiff>())
        {
            if (tableDiff is NewTableDiff newTable)
                AnsiConsole.MarkupLine($"[green]{newTable.Table.ToString().EscapeMarkup()}[/]");
            else if (tableDiff is DeletedTableDiff deletedTable)
                AnsiConsole.MarkupLine($"[red]{deletedTable.Table.ToString().EscapeMarkup()}[/]");
        }

        foreach (IGrouping<DbObjectName, ColumnDiff> columnDiff in validator.Diffs.OfType<ColumnDiff>().GroupBy(cd => cd.Table))
        {
            AnsiConsole.MarkupLine($"[yellow]{columnDiff.Key.ToString().EscapeMarkup()}[/]");
            foreach (ColumnDiff diff in columnDiff)
            {
                (string Color, string? Message) output = diff switch
                {
                    NewColumnDiff => ("green", null),
                    DeletedColumnDiff => ("red", "Deleted"),
                    MissingColumnMetadataDiff => ("red", "Missing metadata"),
                    _ => ("yellow", "Metadata changed"),
                };
                AnsiConsole.MarkupLine($"    [{output.Color}]{diff.Column.EscapeMarkup()} ({output.Message.EscapeMarkup()})[/]");
            }
        }

        return validator.Diffs.Count;
    }
}
