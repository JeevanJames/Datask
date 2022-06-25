namespace Datask.Providers.DbManagement;

/// <summary>
///     Provides capabilities to create, delete and run scripts on databases.
/// </summary>
public interface IDbManagementProvider
{
    /// <summary>
    ///     Attempts to cv
    /// </summary>
    /// <returns></returns>
    Task<bool> TryCreateDatabaseAsync();

    Task DeleteDatabaseAsync();

    Task<bool> DatabaseExistsAsync();

    Task ExecuteScriptsAsync(IAsyncEnumerable<string> scripts);

    Task ExecuteScriptAsync(string script);
}
