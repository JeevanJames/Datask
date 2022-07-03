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

    protected override async Task<int> ExecuteAsync(StatusContext ctx, IParseResult parseResult)
    {
        AnsiConsole.MarkupLine($"Excel file path  : {ExcelFile.FullName}");
        AnsiConsole.MarkupLine($"Connection string: {ConnectionString}");
        AnsiConsole.MarkupLine($"Fix errors       : {Fix}");

        ExcelValidatorOptions options = new(typeof(SqlServerProvider), ConnectionString, ExcelFile);
        ExcelValidator validator = new(options);
        await validator.ExecuteAsync().ConfigureAwait(false);

        AnsiConsole.MarkupLine($"Differences found: {validator.Diffs.Count}");
        foreach (ValidationDiff diff in validator.Diffs)
            AnsiConsole.MarkupLine(diff.ToString());

        return validator.Diffs.Count;
    }
}
