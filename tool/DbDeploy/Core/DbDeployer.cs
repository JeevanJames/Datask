using Datask.Providers;

namespace Datask.Tool.DbDeploy.Core;

public sealed class DbDeployer<TProvider> : IDisposable
    where TProvider : class, IProvider
{
    private readonly IProvider _provider;

    public DbDeployer(string connectionString, string? databaseName)
    {
        _provider = ProviderFactory.Create<TProvider>(connectionString, databaseName);
    }

    public Task DeployMigrationsAsync(MigrationDeploymentOptions options)
    {
        if (options is null)
            throw new ArgumentNullException(nameof(options));

        // Validate options
        if (string.IsNullOrWhiteSpace(options.HistoryTableSchema))
            options.HistoryTableSchema = _provider.Standards.GetDefaultSchemaName();

        if (!options.HasPreMigrationScripts && !options.HasMigrationScripts && !options.HasPostMigrationScripts)
            throw new ArgumentException("No scripts directories specified.", nameof(options));

        if (string.IsNullOrWhiteSpace(options.HistoryTableName))
            throw new ArgumentException("Invalid migration history table name.", nameof(options));

        if (string.IsNullOrWhiteSpace(options.HistoryTableSchema))
            throw new ArgumentException("Invalid migration history table schema.", nameof(options));

        return DeployMigrationsTask(options);
    }

    private async Task DeployMigrationsTask(MigrationDeploymentOptions options)
    {
        MigrationsDeployer deployer = new(_provider, options);
        await deployer.DeployAsync();
    }

    public async Task DropAsync()
    {
        await _provider.DbManagement.DeleteDatabaseAsync().ConfigureAwait(false);
    }

    public void Dispose()
    {
        _provider.Dispose();
    }
}

public sealed class MigrationDeploymentOptions
{
    private readonly Lazy<IList<ScriptsDirectory>> _preMigrationScriptDirs = new(() => new List<ScriptsDirectory>());

    private readonly Lazy<IList<MigrationScriptsDirectory>> _migrationScriptDirs =
        new(() => new List<MigrationScriptsDirectory>());

    private readonly Lazy<IList<ScriptsDirectory>> _postMigrationScriptDirs = new(() => new List<ScriptsDirectory>());

    public DeployMode Mode { get; set; } = DeployMode.Migrate;

    public string HistoryTableName { get; set; } = "__DataskMigrationHistory";

    public string? HistoryTableSchema { get; set; }

    public IList<ScriptsDirectory> PreMigrationScriptDirs => _preMigrationScriptDirs.Value;

    public IList<MigrationScriptsDirectory> MigrationScriptDirs => _migrationScriptDirs.Value;

    public IList<ScriptsDirectory> PostMigrationScriptDirs => _postMigrationScriptDirs.Value;

    public bool HasPreMigrationScripts =>
        _preMigrationScriptDirs.IsValueCreated && _preMigrationScriptDirs.Value.Count > 0;

    public bool HasMigrationScripts => _migrationScriptDirs.IsValueCreated && _migrationScriptDirs.Value.Count > 0;

    public bool HasPostMigrationScripts =>
        _postMigrationScriptDirs.IsValueCreated && _postMigrationScriptDirs.Value.Count > 0;
}

public sealed class ScriptsDirectory
{
    public ScriptsDirectory(string directory, bool recursive = false)
    {
        if (directory is null)
            throw new ArgumentNullException(nameof(directory));

        if (directory.Trim().Length == 0)
            directory = ".";

        Directory = Path.GetFullPath(directory);

        if (!System.IO.Directory.Exists(Directory))
            throw new DirectoryNotFoundException($"The scripts directory '{Directory}' does not exist.");

        Recursive = recursive;
    }

    public string Directory { get; }

    public bool Recursive { get; }
}

public sealed class MigrationScriptsDirectory
{
    private MigrationScriptsDirectory(string directory)
    {
        if (directory is null)
            throw new ArgumentNullException(nameof(directory));

        if (directory.Trim().Length == 0)
            directory = ".";

        Directory = Path.GetFullPath(directory);

        if (!System.IO.Directory.Exists(Directory))
            throw new DirectoryNotFoundException($"The scripts directory '{Directory}' does not exist.");
    }

    public string Directory { get; }

    public override string ToString()
    {
        return Directory;
    }

    public static implicit operator string(MigrationScriptsDirectory dir) => dir.Directory;

    public static implicit operator MigrationScriptsDirectory(string directory) => new(directory);
}

public enum DeployMode
{
    Migrate,
    Full,
}
