namespace Datask.Providers.DbManagement;

public interface IDbManagementProvider
{
    Task<bool> TryCreateDatabaseAsync();

    Task DeleteDatabaseAsync();

    Task<bool> DatabaseExistsAsync();

    Task ExecuteScriptsAsync(IAsyncEnumerable<string> scripts);

    Task ExecuteScriptAsync(string script);
}
