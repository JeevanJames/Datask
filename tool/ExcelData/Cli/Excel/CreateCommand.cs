// Copyright (c) 2021 Jeevan James
// This file is licensed to you under the MIT License.
// See the LICENSE file in the project root for more information.

using Datask.Tool.ExcelData.Core.Excel.Creator;

namespace Datask.Tool.ExcelData.Excel;

[Command("create", ParentType = typeof(ExcelCommand))]
[CommandHelp("Creates an Excel workbook from the schema of a database.")]
public sealed class CreateCommand : BaseCommand
{
    [Argument(Order = 0)]
    [ArgumentHelp("excel file", "The path to the Excel workbook to create.")]
    public FileInfo ExcelFile { get; set; } = null!;

    [Argument(Order = 1)]
    [ArgumentHelp("connection string", "The connection string to the database to create the Excel file from.")]
    public string ConnectionString { get; set; } = null!;

    [Option("include", "i", Optional = true, MultipleOccurrences = true)]
    [OptionHelp("Tables to include in generation; should match the format <schema>.<table>. Use regular express syntax.")]
    public IList<string> IncludeTables { get; } = new List<string>();

    [Option("exclude", "e", Optional = true, MultipleOccurrences = true)]
    [OptionHelp("Tables to exclude in generation; should match the format <schema>.<table>. Use regular express syntax.")]
    public IList<string> ExcludeTables { get; } = new List<string>();

    [Flag("populate")]
    [FlagHelp("Populates the Excel workbook with data from the database.")]
    public bool Populate { get; set; }

    [Flag("force")]
    [FlagHelp("Overwrites the output file, if it already exists.")]
    public bool Force { get; set; }

    protected override async Task<int> ExecuteAsync(StatusContext ctx, IParseResult parseResult)
    {
        if (Populate)
            throw new InvalidOperationException("The populate option is not currently available.");

        if (ExcelFile.Exists)
        {
            ctx.Status($"File '{ExcelFile}' already exists. Overwriting.");
            ExcelFile.Delete();
        }

        ExcelCreatorOptions options = new(ConnectionString, ExcelFile);
        options.IncludeTables.AddRange(IncludeTables.Distinct(StringComparer.OrdinalIgnoreCase));
        options.ExcludeTables.AddRange(ExcludeTables.Distinct(StringComparer.OrdinalIgnoreCase));

        ExcelGenerator generator = new(options);
        generator.OnStatus += (_, args) =>
        {
            ctx.Status(args.Message ?? string.Empty);
            ctx.Refresh();
        };

        await generator.ExecuteAsync().ConfigureAwait(false);
        AnsiConsole.MarkupLine($"The file {ExcelFile.FullName} generated successfully.");

        return 0;
    }

    public override string? Validate(IParseResult parseResult)
    {
        if (parseResult.Group != 0)
            return null;

        if (File.Exists(ExcelFile.FullName) && !Force)
            return $"[red]The file {ExcelFile.FullName} already exists. Specify the --force option to overwrite it.[/]";

        return null;
    }
}
