// Copyright (c) 2021 Jeevan James
// This file is licensed to you under the MIT License.
// See the LICENSE file in the project root for more information.

namespace Datask.Tool.ExcelData.Populate;

[Command("populate")]
[CommandHelp("Populates an empty database with data from an Excel workbook.", Order = 3)]
public sealed class PopulateCommand : BaseCommand
{
    [Argument(Order = 0)]
    [ArgumentHelp("excel file", "The path to the Excel workbook to create.")]
    public FileInfo ExcelFile { get; set; } = null!;

    [Argument(Order = 1)]
    [ArgumentHelp("connection string", "The connection string to the database to create the Excel file from.")]
    public string ConnectionString { get; set; } = null!;

    [Flag("validate")]
    [FlagHelp("Validates the Excel workbook against the database schema before attempting to populate.")]
    public bool ValidateFirst { get; set; }

    protected override Task<int> ExecuteAsync(StatusContext ctx, IParseResult parseResult)
    {
        throw new NotImplementedException();
    }
}
