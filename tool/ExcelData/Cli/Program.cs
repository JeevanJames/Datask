//TODO: Correct namespace

using System.Threading.Tasks;

using ConsoleFx.CmdLine;
using ConsoleFx.CmdLine.ErrorHandling;
using ConsoleFx.CmdLine.Help;

namespace Datask.Tool.ExcelData
{
    public sealed class Program : ConsoleProgram
    {
        public static async Task<int> Main()
        {
            var program = new Program();
            program.WithHelpBuilder(() => new DefaultColorHelpBuilder("help", "h"));
            program.HandleErrorsWith<DefaultErrorHandler>();
            program.ScanEntryAssemblyForCommands();
#if DEBUG_TOOL || DEBUG
            return await program.RunDebugAsync(condition: () => true).ConfigureAwait(false);
#else
            return await program.RunWithCommandLineArgsAsync().ConfigureAwait(false);
#endif
        }
    }
}
