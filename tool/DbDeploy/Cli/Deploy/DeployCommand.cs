using ConsoleFx.CmdLine;
using ConsoleFx.CmdLine.Help;
using ConsoleFx.CmdLine.Validators;

using Datask.Providers.SqlServer;
using Datask.Tool.DbDeploy.Core;

namespace Datask.Tool.DbDeploy.Deploy;

[Command("deploy")]
[CommandHelp("Deploys a database.")]
public sealed class DeployCommand : Command
{
    [Argument(Order = 0)]
    [ArgumentHelp("connection string", "The server connection string (without database name) or the full connection string (without database name).")]
    public string ConnectionString { get; set; } = null!;

    [Option("database", Optional = true)]
    [OptionHelp("The name of the database, if a server connection string is specified. Optional if the full connection string is specified.")]
    public string? DatabaseName { get; set; }

    [Option("mode", Optional = true)]
    [OptionHelp("How to deploy the database. Migrate only runs the pending migration scripts. Full will drop the database if it exists and run all the deployment scripts. If the database doesn't exist, then specifying Migrate will be the same as Full.")]
    public DeployMode Mode { get; set; } = DeployMode.Migrate;

    [Option("directory", Optional = true)]
    [OptionHelp("The directory or base directory that contains the deployment scripts. Default: the current directory.")]
    [DirectoryValidator(shouldExist: true)]
    public DirectoryInfo ScriptsDir { get; set; } = null!;

    [Option("migration-table", Optional = true)]
    [OptionHelp("The name of the migration history table. Default: __DataskMigrationHistory.")]
    public string? MigrationTableName { get; set; }

    [Option("migration-schema", Optional = true)]
    [OptionHelp("The name of the migration history table schema.")]
    public string? MigrationTableSchema { get; set; }

    public override async Task<int> HandleCommandAsync(IParseResult parseResult)
    {
        using DbDeployer<SqlServerProvider> deployer = new(ConnectionString, DatabaseName);

        MigrationDeploymentOptions options = new()
        {
            Mode = Mode,
            HistoryTableSchema = MigrationTableSchema,
        };
        if (MigrationTableName is not null)
            options.HistoryTableName = MigrationTableName;
        
        await deployer.DeployMigrationsAsync(options).ConfigureAwait(false);

        return 0;
    }
}
