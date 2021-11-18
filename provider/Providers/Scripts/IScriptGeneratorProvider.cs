using System.Data;

namespace Datask.Providers.Scripts;

public interface IScriptGeneratorProvider
{
}

public abstract class ScriptGeneratorProvider<TConnection> : SubProviderBase<TConnection>, IScriptGeneratorProvider
    where TConnection : IDbConnection
{
    protected ScriptGeneratorProvider(TConnection connection)
        : base(connection)
    {
    }
}
