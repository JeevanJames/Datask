using ConsoleFx.CmdLine.Validators;

using static Spectre.Console.AnsiConsole;

namespace Datask.Tool.ExcelData.Excel;

[Command("details", ParentType = typeof(ExcelCommand))]
public sealed class DetailsCommand : Command
{
    [Argument(Order = 0)]
    [ArgumentHelp("excel file", "The path to the Excel workbook to create.")]
    [FileValidator("xlsx", ShouldExist = true)]
    public FileInfo ExcelFile { get; set; } = null!;

    protected override int HandleCommand()
    {
        DataExcelWorkbook workbook = new(ExcelFile.FullName);
        foreach (DataExcelTable table in workbook.EnumerateTables())
        {
            MarkupLine($"[cyan]{table.Name.Schema}.{table.Name.Name}[/]");

            foreach (DataExcelTableColumn column in table.Columns)
            {
                MarkupLine($"    [yellow]{column.Text}[/]");
                MarkupLine($"        [cyan]{column.Metadata?.ToString().EscapeMarkup() ?? "<No metadata>"}[/]");
            }

            foreach (object?[] row in table.EnumerateRows())
            {
                MarkupLine($"    [purple]{string.Join(',', row.Select(c => c?.ToString() ?? "<null>"))}[/]");
            }
        }

        return 0;
    }
}
