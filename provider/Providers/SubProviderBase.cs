using System.Data;

namespace Datask.Providers;

//TODO: Make subproviders disposable
public abstract class SubProviderBase<TConnection>
    where TConnection : IDbConnection
{
    protected SubProviderBase(TConnection connection)
    {
        Connection = connection ?? throw new ArgumentNullException(nameof(connection));
    }

    protected TConnection Connection { get; }
}
