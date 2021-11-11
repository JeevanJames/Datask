// Copyright (c) 2021 Jeevan James
// This file is licensed to you under the MIT License.
// See the LICENSE file in the project root for more information.

using Spectre.Console;

namespace Datask.Common.Cli;

public static class DataskCli
{
    public static Task<int> StartAsync(string initialStatus, Func<StatusContext, Task<int>> action)
    {
        return AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots2)
            .SpinnerStyle(Style.Parse("cyan"))
            .StartAsync(initialStatus, action);
    }
}
