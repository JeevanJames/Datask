namespace Datask.Tool.ExcelData.Excel;

[Command("create", ParentType = typeof(ExcelCommand))]
[CommandHelp("Creates an Excel file from the schema of a database.")]
public sealed class GenerateCommand : BaseCommand
{
    [Argument(Order = 0)]
    [ArgumentHelp("connection string", "The connection string to the database to create the Excel file from.")]
    public string ConnectionString { get; set; } = null!;

    [Argument(Order = 1)]
    [ArgumentHelp("file name", "The path to the Excel file to create.")]
    public FileInfo ExcelFile { get; set; } = null!;

    [Option("include", "i", Optional = true, MultipleOccurrences = true)]
    [OptionHelp("Tables to include in generation; should match the format <schema>.<table>. Use regular express syntax.")]
    public IList<string> IncludeTables { get; } = new List<string>();

    [Option("exclude", "e", Optional = true, MultipleOccurrences = true)]
    [OptionHelp("Tables to exclude in generation; should match the format <schema>.<table>. Use regular express syntax.")]
    public IList<string> ExcludeTables { get; } = new List<string>();

    [Flag("force")]
    [FlagHelp("Overwrites the output file, if it already exists.")]
    public bool Force { get; set; }

    protected override async Task<int> ExecuteAsync(StatusContext ctx, IParseResult parseResult)
    {
        if (File.Exists(ExcelFile.FullName))
        {
            ctx.Status($"File '{ExcelFile}' already exists. Overwriting.");
            ExcelFile.Delete();
        }

        ExcelGeneratorOptions options = new(ConnectionString, ExcelFile);
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
