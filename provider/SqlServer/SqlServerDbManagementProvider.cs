// Copyright (c) 2021 Jeevan James
// This file is licensed to you under the MIT License.
// See the LICENSE file in the project root for more information.

using Datask.Providers.DbManagement;

using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

namespace Datask.Providers.SqlServer;

/// <inheritdoc />
public sealed class SqlServerDbManagementProvider : DbManagementProvider<SqlConnection>, IDisposable
{
    private readonly SqlConnection _serverConnection;
    private readonly Server _server;
    private readonly string _databaseName;

    public SqlServerDbManagementProvider(SqlConnection connection)
        : base(connection)
    {
        SqlConnectionStringBuilder builder = new(connection.ConnectionString);

        _databaseName = builder.InitialCatalog;

        builder.InitialCatalog = string.Empty;
        _serverConnection = new SqlConnection(builder.ConnectionString);

        _server = new Server(new ServerConnection(_serverConnection));
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _serverConnection.Dispose();
    }

    /// <inheritdoc />
    protected override bool TryCreateDatabase()
    {
        Database database = _server.Databases[_databaseName];
        if (database is not null)
            return true;

        database = new Database(_server, _databaseName);
        database.Create();
        return true;
    }

    /// <inheritdoc />
    protected override void DeleteDatabase()
    {
        Database database = _server.Databases[_databaseName];
        if (database is not null)
            _server.KillDatabase(_databaseName);
    }

    /// <inheritdoc />
    protected override bool DatabaseExists()
    {
        return _server.Databases[_databaseName] is not null;
    }

    /// <inheritdoc />
    public override async Task ExecuteScriptsAsync(IAsyncEnumerable<string> scripts)
    {
        Database database = _server.Databases[_databaseName];
        await foreach (string script in scripts)
            database.ExecuteNonQuery(script);
    }

    /// <inheritdoc />
    protected override void ExecuteScript(string script)
    {
        Database database = _server.Databases[_databaseName];
        database.ExecuteNonQuery(script);
    }
}
