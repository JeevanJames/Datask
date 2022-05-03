using ConsoleFx.CmdLine;
using ConsoleFx.CmdLine.Help;

using Datask.Providers.SqlServer;
using Datask.Tool.DbDeploy.Core;

using Spectre.Console;

namespace Datask.Tool.DbDeploy.Drop;

[Command("drop")]
[CommandHelp("Drops a database.")]
public sealed class DropCommand : Command
{
    [Argument(Order = 0)]
    [ArgumentHelp("connection string", "The server connection string (without database name) or the full connection string (without database name).")]
    public string ConnectionString { get; set; } = null!;

    [Option("database", Optional = true)]
    [OptionHelp("The name of the database, if a server connection string is specified. Optional if the full connection string is specified.")]
    public string? DatabaseName { get; set; }

    public override async Task<int> HandleCommandAsync(IParseResult parseResult)
    {
        DbDeployer<SqlServerProvider> deployer = new(ConnectionString, DatabaseName);
        await deployer.DropAsync().ConfigureAwait(false);

        AnsiConsole.MarkupLine("Successfully dropped the database.");

        return 0;
    }
}
