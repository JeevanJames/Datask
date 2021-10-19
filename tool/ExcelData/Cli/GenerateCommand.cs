using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using ConsoleFx.CmdLine;
using ConsoleFx.CmdLine.Help;
using ConsoleFx.Prompter;
using ConsoleFx.Prompter.Questions;

using Datask.Tool.ExcelData.Core;

using Spectre.Console;

namespace Datask.Tool.ExcelData
{
    [Command("generate", "g")]
    [CommandHelp("Generates xlsx file with database table and column information.")]
    public class GenerateCommand : Command
    {
        [Argument]
        [ArgumentHelp("connection string", "The connection string to the database to create the Excel file from.")]
        public string ConnectionString { get; set; } = null!;

        [Argument]
        [ArgumentHelp("file name", "The name of the Excel file to create.")]
        public FileInfo ExcelFile { get; set; } = null!;

        [Option("include", "i", Optional = true, MultipleOccurrences = true)]
        [OptionHelp("One or more regular expressions specifying the tables to include.This should match the<schema>.<table> format.")]
        public IList<string> IncludeSchema { get; } = new List<string>();

        [Option("exclude", "e", Optional = true, MultipleOccurrences = true)]
        [OptionHelp("One or more regular expressions specifying the tables to exclude.This should match the<schema>.<table> format.Tables are excluded after considering the tables to include.")]
        public IList<string> ExcludeTables { get; } = new List<string>();

        public override async Task<int> HandleCommandAsync(IParseResult parseResult)
        {
            if (File.Exists(ExcelFile.FullName))
            {
                PrompterFlow.Style = Styling.Terminal;
                var prompter = new PrompterFlow
                {
                    new ConfirmQuestion("ExcelFileName", $"Do you want to overwrite the existing file '{ExcelFile.FullName}' ? ", @default: false),
                    new InputQuestion("NewFilePath", "Enter the new xlsx file path: ")
                        .When(ans => !ans.ExcelFileName)
                        .ValidateWith(file => IsValidFilePath(file)),
                };

                dynamic answers = await prompter.Ask().ConfigureAwait(false);

                if (!answers.ExcelFileName)
                    ExcelFile = new FileInfo(answers.NewFilePath);
                else
                    File.Delete(ExcelFile.FullName);
            }

            await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots2)
                .SpinnerStyle(Style.Parse("cyan"))
                .StartAsync("Exporting database table details...", async ctx =>
                {
                    DataConfiguration configuration = new()
                    {
                        ConnectionString = ConnectionString,
                        FilePath = ExcelFile,
                    };

                    configuration.IncludeSchemas.AddRange(IncludeSchema.Distinct());

                    DataBuilder builder = new(configuration);
                    builder.OnStatus += (_, args) =>
                    {
                        ctx.Status(args.Message ?? string.Empty);
                        ctx.Refresh();
                    };
                    await builder.ExportExcel().ConfigureAwait(false);
                }).ConfigureAwait(false);

            AnsiConsole.MarkupLine($"The file {ExcelFile.FullName} generated successfully.");

            return 0;
        }

        private static bool IsValidFilePath(string path)
        {
            return path.Length > 0
                && path.IndexOfAny(Path.GetInvalidPathChars()) == -1
                && Path.IsPathRooted(path)
                && path.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase)
                && !File.Exists(path);
        }
    }
}
