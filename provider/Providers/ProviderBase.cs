using System.Data;

using Datask.Providers.DbManagement;
using Datask.Providers.Schemas;
using Datask.Providers.Scripts;
using Datask.Providers.Standards;

namespace Datask.Providers;

/// <summary>
///     Useful base class for database providers that does all the work to setup the properties.
/// </summary>
/// <typeparam name="TConnection">The type of the database ADO.NET connection.</typeparam>
/// <typeparam name="TDbManagement">
///     The type of the db management provider (<see cref="IDbManagementProvider"/>).
/// </typeparam>
/// <typeparam name="TSchemaQuery">
///     The type of the schema query provider (<see cref="ISchemaQueryProvider"/>).
/// </typeparam>
/// <typeparam name="TScriptGenerator">
///     The type of the script generator provider (<see cref="IScriptGeneratorProvider"/>).
/// </typeparam>
/// <typeparam name="TStandards">
///     The type of the standards provider (<see cref="IStandardsProvider"/>).
/// </typeparam>
public abstract class ProviderBase<TConnection, TDbManagement, TSchemaQuery, TScriptGenerator, TStandards> : IProvider
    where TConnection: notnull, IDbConnection
    where TDbManagement: DbManagementProvider<TConnection>, IDbManagementProvider
    where TSchemaQuery : SchemaQueryProvider<TConnection>, ISchemaQueryProvider
    where TScriptGenerator : ScriptGeneratorProvider<TConnection>, IScriptGeneratorProvider
    where TStandards : StandardsProvider<TConnection>, IStandardsProvider
{
    private readonly TConnection _connection;
    private bool _disposedValue;

    private readonly Lazy<TDbManagement> _dbManagement;
    private readonly Lazy<TSchemaQuery> _schemaQueryProvider;
    private readonly Lazy<TScriptGenerator> _scriptGeneratorProvider;
    private readonly Lazy<TStandards> _standardsProvider;

    protected ProviderBase(string connectionString, Func<string, TConnection> connectionFactory,
        Func<string, string?, string> connectionStringValidator,
        string? databaseName = null)
    {
        if (connectionString is null)
            throw new ArgumentNullException(nameof(connectionString));
        if (connectionFactory is null)
            throw new ArgumentNullException(nameof(connectionFactory));
        if (connectionStringValidator is null)
            throw new ArgumentNullException(nameof(connectionStringValidator));

        connectionString = connectionStringValidator(connectionString, databaseName);
        _connection = connectionFactory(connectionString);

        _dbManagement = new Lazy<TDbManagement>(InstantiateSubProvider<TDbManagement>);
        _schemaQueryProvider = new Lazy<TSchemaQuery>(InstantiateSubProvider<TSchemaQuery>);
        _scriptGeneratorProvider = new Lazy<TScriptGenerator>(InstantiateSubProvider<TScriptGenerator>);
        _standardsProvider = new Lazy<TStandards>(InstantiateSubProvider<TStandards>);
    }

    public IDbManagementProvider DbManagement => _dbManagement.Value;

    public ISchemaQueryProvider SchemaQuery => _schemaQueryProvider.Value;

    public IScriptGeneratorProvider ScriptGenerator => _scriptGeneratorProvider.Value;

    public IStandardsProvider Standards => _standardsProvider.Value;

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
                _connection.Dispose();

            if (DbManagement is IDisposable disposableDbManagement)
                disposableDbManagement.Dispose();

            if (SchemaQuery is IDisposable disposableSchemaQuery)
                disposableSchemaQuery.Dispose();

            if (ScriptGenerator is IDisposable disposableScriptGenerator)
                disposableScriptGenerator.Dispose();

            if (Standards is IDisposable disposableStandards)
                disposableStandards.Dispose();

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
