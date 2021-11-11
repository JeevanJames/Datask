using System.Threading.Tasks;

using ConsoleFx.CmdLine;
using ConsoleFx.CmdLine.Help;

using Spectre.Console;

namespace Datask.Tool.ExcelData
{
    public sealed class Program : ConsoleProgram
    {
        public static async Task<int> Main()
        {
            var program = new Program();
            program.WithHelpBuilder(() => new DefaultColorHelpBuilder("help", "h"));
            program.HandleErrorsWith(ex =>
            {
                AnsiConsole.WriteException(ex);
                return 1;
            });
            program.ScanEntryAssemblyForCommands();
#if DEBUG_TOOL || DEBUG
            return await program.RunDebugAsync(condition: () => true).ConfigureAwait(false);
#else
            return await program.RunWithCommandLineArgsAsync().ConfigureAwait(false);
#endif
        }
    }
}
