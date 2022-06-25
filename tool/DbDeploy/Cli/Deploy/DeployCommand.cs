using ConsoleFx.CmdLine;
using ConsoleFx.CmdLine.Help;
using ConsoleFx.CmdLine.Validators;

using Datask.Providers.SqlServer;
using Datask.Tool.DbDeploy.Core;

using Spectre.Console;

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

    [Option("pre-migration-scripts-dir", Optional = true, MultipleOccurrences = true)]
    [OptionHelp("One or more script directories to execute before running the migrations.")]
    public IList<DirectoryInfo> PreMigrationScriptDirs { get; set; } = null!;

    [Option("directory", Optional = true, MultipleOccurrences = true)]
    [OptionHelp("The directory or base directory that contains the deployment scripts. Default: the current directory.")]
    [DirectoryValidator(shouldExist: true)]
    public IList<DirectoryInfo> MigrationScriptsDirs { get; set; } = null!;

    [Option("migration-table", Optional = true)]
    [OptionHelp("The name of the migration history table. Default: __DataskMigrationHistory.")]
    public string? MigrationTableName { get; set; }

    [Option("migration-schema", Optional = true)]
    [OptionHelp("The name of the migration history table schema.")]
    public string? MigrationTableSchema { get; set; }

    public override async Task<int> HandleCommandAsync(IParseResult parseResult)
    {
        using DbDeployer<SqlServerProvider> deployer = new(ConnectionString, DatabaseName);

        DeployOptions options = new()
        {
            Mode = Mode,
            HistoryTableSchema = MigrationTableSchema,
        };

        if (MigrationTableName is not null)
            options.HistoryTableName = MigrationTableName;

        if (PreMigrationScriptDirs.Count > 0)
        {
            options.PreMigrationScriptDirs.AddRange(
                PreMigrationScriptDirs.Select(s => new ScriptsSpec(s.FullName, recursive: false)));
        }

        if (MigrationScriptsDirs.Count > 0)
        {
            options.MigrationScriptDirs.AddRange(
                MigrationScriptsDirs.Select(di => (MigrationScriptsSpec)di.FullName));
        }

        foreach (DirectoryInfo dir in PreMigrationScriptDirs)
            AnsiConsole.MarkupLine($"Pre-migration script dir: {dir}");

        foreach (DirectoryInfo dir in MigrationScriptsDirs)
            AnsiConsole.MarkupLine($"Migration script dir: {dir}");

        await deployer.DeployAsync(options).ConfigureAwait(false);

        return 0;
    }
}
