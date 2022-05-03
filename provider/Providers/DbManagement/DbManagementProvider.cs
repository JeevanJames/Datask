using System.Data;

namespace Datask.Providers.DbManagement;

public abstract class DbManagementProvider<TConnection> : SubProviderBase<TConnection>, IDbManagementProvider
    where TConnection : IDbConnection
{
    protected DbManagementProvider(TConnection connection)
        : base(connection)
    {
    }

    public virtual Task<bool> TryCreateDatabaseAsync()
    {
        return Task.FromResult(TryCreateDatabase());
    }

    protected virtual bool TryCreateDatabase()
    {
        throw new NotImplementedException();
    }

    public virtual Task DeleteDatabaseAsync()
    {
        DeleteDatabase();
        return Task.CompletedTask;
    }

    protected virtual void DeleteDatabase()
    {
        throw new NotImplementedException();
    }

    public virtual Task<bool> DatabaseExistsAsync()
    {
        return Task.FromResult(DatabaseExists());
    }

    protected virtual bool DatabaseExists()
    {
        throw new NotImplementedException();
    }

    public abstract Task ExecuteScriptsAsync(IAsyncEnumerable<string> scripts);

    public virtual Task ExecuteScriptAsync(string script)
    {
        return Task.FromResult(script);
    }

    protected virtual void ExecuteScript(string script)
    {
        throw new NotImplementedException();
    }
}
