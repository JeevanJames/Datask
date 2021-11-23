using System.Data;

using Datask.Providers.Schemas;
using Datask.Providers.Scripts;

namespace Datask.Providers;

/// <summary>
///     Useful base class for database providers that does all the work to setup the properties.
/// </summary>
/// <typeparam name="TConnection">The type of the database ADO.NET connection.</typeparam>
/// <typeparam name="TSchemaQuery">
///     The type of the schema query provider (<see cref="ISchemaQueryProvider"/>).
/// </typeparam>
/// <typeparam name="TScriptGenerator">
///     The type of the script generator provider (<see cref="IScriptGeneratorProvider"/>).
/// </typeparam>
public abstract class ProviderBase<TConnection, TSchemaQuery, TScriptGenerator> : IProvider
    where TConnection: notnull, IDbConnection
    where TSchemaQuery : SchemaQueryProvider<TConnection>, ISchemaQueryProvider
    where TScriptGenerator : ScriptGeneratorProvider<TConnection>, IScriptGeneratorProvider
{
    private readonly TConnection _connection;
    private bool _disposedValue;

    private readonly Lazy<TSchemaQuery> _schemaQueryProvider;
    private readonly Lazy<TScriptGenerator> _scriptGeneratorProvider;

    protected ProviderBase(string connectionString, Func<string, TConnection> connectionFactory)
    {
        if (connectionString is null)
            throw new ArgumentNullException(nameof(connectionString));
        if (connectionFactory is null)
            throw new ArgumentNullException(nameof(connectionFactory));

        _connection = connectionFactory(connectionString);

        _schemaQueryProvider = new Lazy<TSchemaQuery>(InstantiateSubProvider<TSchemaQuery>);
        _scriptGeneratorProvider = new Lazy<TScriptGenerator>(InstantiateSubProvider<TScriptGenerator>);
    }

    public ISchemaQueryProvider SchemaQuery => _schemaQueryProvider.Value;

    public IScriptGeneratorProvider ScriptGenerator => _scriptGeneratorProvider.Value;

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
                _connection.Dispose();

            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private TSubProvider InstantiateSubProvider<TSubProvider>()
    {
        return (TSubProvider)Activator.CreateInstance(typeof(TSubProvider), _connection);
    }
}
