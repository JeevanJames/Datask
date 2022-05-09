// Copyright (c) 2021 Jeevan James
// This file is licensed to you under the MIT License.
// See the LICENSE file in the project root for more information.

namespace Datask.Tool.ExcelData;

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
