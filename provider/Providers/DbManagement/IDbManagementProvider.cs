namespace Datask.Providers.DbManagement;

public interface IDbManagementProvider
{
    Task<bool> TryCreateDatabaseAsync();

    Task DeleteDatabaseAsync();

    Task<bool> DatabaseExistsAsync();
}
