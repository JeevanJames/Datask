// Copyright (c) 2021 Jeevan James
// This file is licensed to you under the MIT License.
// See the LICENSE file in the project root for more information.

using ConsoleFx.CmdLine;

using Spectre.Console;

namespace Datask.Common.Cli;

public abstract class BaseCommand : Command
{
    public sealed override async Task<int> HandleCommandAsync(IParseResult parseResult)
    {
        int result = await DataskCli.StartAsync("Processing...",
            async ctx => await ExecuteAsync(ctx, parseResult).ConfigureAwait(false));
        return await PostExecuteAsync(result, parseResult);
    }

    protected abstract Task<int> ExecuteAsync(StatusContext ctx, IParseResult parseResult);

    protected virtual Task<int> PostExecuteAsync(int executeResult, IParseResult parseResult)
    {
        return Task.FromResult(executeResult);
    }
}
