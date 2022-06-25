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

    public Task DeployAsync(DeployOptions options)
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

        return DeployTask(options);
    }

    private async Task DeployTask(DeployOptions options)
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

public abstract class ScriptsSpecBase
{
    protected ScriptsSpecBase(string directory)
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

    public static implicit operator string(ScriptsSpecBase spec) => spec.Directory;
}

public sealed class ScriptsSpec : ScriptsSpecBase
{
    public ScriptsSpec(string directory, bool recursive = false)
        : base(directory)
    {
        Recursive = recursive;
    }

    public bool Recursive { get; }

    public static implicit operator ScriptsSpec(string directory) => new(directory);
}

public sealed class MigrationScriptsSpec : ScriptsSpecBase
{
    private MigrationScriptsSpec(string directory)
        : base(directory)
    {
    }

    public static implicit operator MigrationScriptsSpec(string directory) => new(directory);
}

public enum DeployMode
{
    Migrate,
    Full,
}
