// Copyright (c) 2021 Jeevan James
// This file is licensed to you under the MIT License.
// See the LICENSE file in the project root for more information.

using Npgsql;

namespace Datask.Providers.PostgreSql;

public sealed class PostgreSqlProvider : IProvider
{
    private readonly NpgsqlConnection _connection;
    private readonly Lazy<ISchemaQueryProvider> _schemaProvider;

    public PostgreSqlProvider(string connectionString)
    {
        _connection = new NpgsqlConnection(connectionString);
        _schemaProvider = new(InitializeSchemaQuery);
    }

    public ValueTask DisposeAsync()
    {
        return _connection.DisposeAsync();
    }

    public ISchemaQueryProvider SchemaQuery => _schemaProvider.Value;

    private ISchemaQueryProvider InitializeSchemaQuery()
    {
        return new PostgreSqlSchemaQueryProvider(_connection);
    }
}
