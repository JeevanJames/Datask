using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using ConsoleFx.CmdLine;
using ConsoleFx.CmdLine.Help;

using Datask.Common.Cli;
using Datask.Tool.ExcelData.Core;

using Spectre.Console;

namespace Datask.Tool.ExcelData
{
    [Command("generate", "g")]
    [CommandHelp("Generates xlsx file with database table and column information.")]
    public class GenerateCommand : BaseCommand
    {
        [Argument(Order = 0)]
        [ArgumentHelp("connection string", "The connection string to the database to create the Excel file from.")]
        public string ConnectionString { get; set; } = null!;

        [Argument(Order = 1)]
        [ArgumentHelp("file name", "The name of the Excel file to create.")]
        public FileInfo ExcelFile { get; set; } = null!;

        [Option("include", "i", Optional = true, MultipleOccurrences = true)]
        [OptionHelp("One or more regular expressions specifying the tables to include.This should match the <schema>.<table> format.")]
        public IList<string> IncludeTables { get; } = new List<string>();

        [Option("exclude", "e", Optional = true, MultipleOccurrences = true)]
        [OptionHelp("One or more regular expressions specifying the tables to exclude.This should match the <schema>.<table> format.Tables are excluded after considering the tables to include.")]
        public IList<string> ExcludeTables { get; } = new List<string>();

        [Flag("force")]
        [FlagHelp("If the Excel file already exists, overwrite it.")]
        public bool Force { get; set; }

        protected override async Task<int> ExecuteAsync(StatusContext ctx, IParseResult parseResult)
        {
            if (File.Exists(ExcelFile.FullName))
            {
                ctx.Status($"File '{ExcelFile}' already exists. Overwriting.");
                ExcelFile.Delete();
            }

            DataConfiguration configuration = new() { ConnectionString = ConnectionString, FilePath = ExcelFile, };

            configuration.IncludeTables.AddRange(IncludeTables.Distinct());
            configuration.ExcludeTables.AddRange(ExcludeTables.Distinct());

            DataBuilder builder = new(configuration);
            builder.OnStatus += (_, args) =>
            {
                ctx.Status(args.Message ?? string.Empty);
                ctx.Refresh();
            };

            await builder.ExportExcel().ConfigureAwait(false);
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
}
