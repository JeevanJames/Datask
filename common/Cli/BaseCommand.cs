// Copyright (c) 2021 Jeevan James
// This file is licensed to you under the MIT License.
// See the LICENSE file in the project root for more information.

using System.Threading.Tasks;

using ConsoleFx.CmdLine;

using Spectre.Console;

namespace Datask.Common.Cli
{
    public abstract class BaseCommand : Command
    {
        public sealed override Task<int> HandleCommandAsync(IParseResult parseResult)
        {
            return DataskCli.StartAsync("Processing...",
                async ctx => await ExecuteAsync(ctx, parseResult).ConfigureAwait(false));
        }

        protected abstract Task<int> ExecuteAsync(StatusContext ctx, IParseResult parseResult);
    }
}
