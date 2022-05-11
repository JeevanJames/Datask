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
            MarkupLine($"[cyan]{table.Schema}.{table.TableName}[/]");

            MarkupLine($"    [yellow]{string.Join(',', table.Columns.Select(c => c.Text))}[/]");

            foreach (object?[] row in table.EnumerateRows())
            {
                MarkupLine($"    [purple]{string.Join(',', row.Select(c => c?.ToString() ?? "<null>"))}[/]");
            }
        }

        return 0;
    }
}
