// Copyright (c) 2021 Jeevan James
// This file is licensed to you under the MIT License.
// See the LICENSE file in the project root for more information.

namespace Datask.Tool.ExcelData.Generation;

[Command("scripts", ParentType = typeof(GenerateCommand))]
[CommandHelp("Generates SQL INSERT scripts from the Excel workbook.")]
public sealed class ScriptsCommand : BaseCommand
{
    [Argument(Order = 0)]
    [ArgumentHelp("excel file", "The path to the Excel workbook to generate from.")]
    public FileInfo ExcelFile { get; set; } = null!;

    [Argument(Order = 1)]
    [ArgumentHelp("output", "The path to the scripts file to create.")]
    public FileInfo ScriptsFile { get; set; } = null!;

    protected override Task<int> ExecuteAsync(StatusContext ctx, IParseResult parseResult)
    {
        AnsiConsole.MarkupLine($"Excel workbook: {ExcelFile}");
        AnsiConsole.MarkupLine($"Scripts file  : {ScriptsFile}");
        return Task.FromResult(0);
    }
}
