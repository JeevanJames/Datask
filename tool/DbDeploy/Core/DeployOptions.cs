namespace Datask.Tool.DbDeploy.Core;

public sealed class DeployOptions
{
    private readonly Lazy<IList<ScriptsSpec>> _preMigrationScriptDirs = new(() => new List<ScriptsSpec>());

    private readonly Lazy<IList<MigrationScriptsSpec>> _migrationScriptDirs =
        new(() => new List<MigrationScriptsSpec>());

    private readonly Lazy<IList<ScriptsSpec>> _postMigrationScriptDirs = new(() => new List<ScriptsSpec>());

    /// <summary>
    ///     Gets or sets the deployment mode.
    /// </summary>
    public DeployMode Mode { get; set; } = DeployMode.Migrate;

    /// <summary>
    ///     Gets or sets the name of the migration history table. Defaults to __DataskMigrationHistory.
    /// </summary>
    public string HistoryTableName { get; set; } = "__DataskMigrationHistory";

    /// <summary>
    ///     Gets or sets the schema name of the migration history table. If not set, defaults to the
    ///     default schema of the underlying database.
    /// </summary>
    public string? HistoryTableSchema { get; set; }

    /// <summary>
    ///     Gets the collection of script to be executed before the migration scripts are executed.
    /// </summary>
    public IList<ScriptsSpec> PreMigrationScriptDirs => _preMigrationScriptDirs.Value;

    /// <summary>
    ///     Gets the collection of migration script to be executed.
    /// </summary>
    public IList<MigrationScriptsSpec> MigrationScriptDirs => _migrationScriptDirs.Value;

    /// <summary>
    ///     Gets the collection of scripts to be executed after the migration scripts are executed.
    /// </summary>
    public IList<ScriptsSpec> PostMigrationScriptDirs => _postMigrationScriptDirs.Value;

    /// <summary>
    ///     Gets a value indicating whether any pre-migration scripts have been specified.
    /// </summary>
    public bool HasPreMigrationScripts =>
        _preMigrationScriptDirs.IsValueCreated && _preMigrationScriptDirs.Value.Count > 0;

    /// <summary>
    ///     Gets a value indicating whether anymigration scripts have been specified.
    /// </summary>
    public bool HasMigrationScripts => _migrationScriptDirs.IsValueCreated && _migrationScriptDirs.Value.Count > 0;

    /// <summary>
    ///     Gets a value indicating whether any post-migration scripts have been specified.
    /// </summary>
    public bool HasPostMigrationScripts =>
        _postMigrationScriptDirs.IsValueCreated && _postMigrationScriptDirs.Value.Count > 0;
}
