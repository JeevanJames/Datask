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
        [Option("connection-string", "cs", Optional = false)]
        [OptionHelp("The sql database connection string.")]
        public string ConnectionString { get; set; } = null!;

        [Option("xlsx", "f", Optional = false)]
        [OptionHelp("The xlsx file path.")]
        public FileInfo ExcelFile { get; set; } = null!;

        [Option("s", Optional = true, MultipleOccurrences = true)]
        [OptionHelp("Include Schemas.")]
        public IList<string> IncludeSchema { get; } = new List<string>();

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
                        .ValidateWith(file => file.Length > 0 && IsValidFilePath(file)),
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
            return path.IndexOfAny(Path.GetInvalidPathChars()) == -1
                   && Path.IsPathRooted(path)
                   && path.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase)
                   && !File.Exists(path);
        }
    }
}
