using Datask.Providers;

namespace Datask.Tool.DbDeploy.Core;

internal sealed class MigrationsDeployer
{
    private readonly IProvider _provider;
    private readonly MigrationDeploymentOptions _options;

    internal MigrationsDeployer(IProvider provider, MigrationDeploymentOptions options)
    {
        _provider = provider;
        _options = options;
    }

    internal async Task DeployAsync()
    {
        await DropAndCreateDatabase().ConfigureAwait(false);

        await foreach (ScriptFile scriptFile in EnumerateScripts())
            await _provider.DbManagement.ExecuteScriptAsync(scriptFile.Content);
    }

    private async Task DropAndCreateDatabase()
    {
        bool databaseExists = await _provider.DbManagement.DatabaseExistsAsync().ConfigureAwait(false);

        // For Full deployments, drop the database if it exists.
        if (_options.Mode == DeployMode.Full && databaseExists)
        {
            await _provider.DbManagement.DeleteDatabaseAsync().ConfigureAwait(false);
            databaseExists = false;
        }

        // Create the database, if it does not exist.
        if (!databaseExists)
            await _provider.DbManagement.TryCreateDatabaseAsync().ConfigureAwait(false);
    }

    private async IAsyncEnumerable<ScriptFile> EnumerateScripts()
    {
        if (_options.HasPreMigrationScripts)
        {
            foreach (ScriptsDirectory scriptsDir in _options.PreMigrationScriptDirs)
            {
                foreach (string scriptFile in EnumerateScriptFiles(scriptsDir.Directory, scriptsDir.Recursive))
                {
                    yield return new ScriptFile(scriptFile,
                        await File.ReadAllTextAsync(scriptFile).ConfigureAwait(false),
                        isMigration: false);
                }
            }
        }

        if (_options.HasMigrationScripts)
        {
            foreach (string scriptsDir in _options.MigrationScriptDirs)
            {
                foreach (string scriptFile in EnumerateScriptFiles(scriptsDir, recursive: false))
                {
                    yield return new ScriptFile(scriptFile,
                        await File.ReadAllTextAsync(scriptFile).ConfigureAwait(false),
                        isMigration: true);
                }
            }
        }
    }

    private static IEnumerable<string> EnumerateScriptFiles(string directory, bool recursive)
    {
        return Directory
            .EnumerateFiles(directory, "*.sql", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
            .OrderBy(file => file, StringComparer.OrdinalIgnoreCase);
    }
}

internal sealed class ScriptFile
{
    internal ScriptFile(string filePath, string content, bool isMigration)
    {
        FilePath = filePath;
        Content = content;
        IsMigration = isMigration;
    }

    internal string FilePath { get; }

    internal string Content { get; }

    internal bool IsMigration { get; }

    internal string DirectoryName => Path.GetDirectoryName(FilePath)
                                     ?? throw new InvalidOperationException($"Script file '{FilePath}' directory could not be retrieved.");
}
