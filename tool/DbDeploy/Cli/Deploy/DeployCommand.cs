using ConsoleFx.CmdLine;
using ConsoleFx.CmdLine.Help;

namespace Datask.Tool.DbDeploy.Deploy;

[Command("deploy")]
[CommandHelp("Deploys a database.")]
public sealed class DeployCommand : Command
{
    [Argument(Order = 0)]
    [ArgumentHelp("connection string", "The server connection string (without database name) or the full connection string (without database name).")]
    public string ConnectionString { get; set; } = null!;

    [Option("database")]
    [OptionHelp("The name of the database, if a server connection string is specified. Optional if the full connection string is specified.")]
    public string? DatabaseName { get; set; }

    [Option("mode")]
    [OptionHelp("How to deploy the database. Migrate only runs the pending migration scripts. Full will drop the database if it exists and run all the deployment scripts. If the database doesn't exist, then specifying Migrate will be the same as Full.")]
    public DeployMode Mode { get; set; }

    [Option("directory")]
    [OptionHelp("The directory or base directory that contains the deployment scripts. Default: the current directory.")]
    public DirectoryInfo ScriptsDir { get; set; } = null!;

    public override Task<int> HandleCommandAsync(IParseResult parseResult)
    {
        return Task.FromResult(0);
    }
}

public enum DeployMode
{
    Migrate,
    Full
}
