using ConsoleFx.CmdLine;
using ConsoleFx.CmdLine.Help;

namespace Datask.Tool.DbDeploy.Drop;

[Command("drop")]
[CommandHelp("Drops a database.")]
public sealed class DropCommand : Command
{
    [Argument(Order = 0)]
    [ArgumentHelp("connection string", "The server connection string (without database name) or the full connection string (without database name).")]
    public string ConnectionString { get; set; } = null!;

    [Option("database")]
    [OptionHelp("The name of the database, if a server connection string is specified. Optional if the full connection string is specified.")]
    public string? DatabaseName { get; set; }

    public override Task<int> HandleCommandAsync(IParseResult parseResult)
    {
        return Task.FromResult(0);
    }
}
