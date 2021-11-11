// Copyright (c) 2021 Jeevan James
// This file is licensed to you under the MIT License.
// See the LICENSE file in the project root for more information.

using Npgsql;

namespace Datask.Providers.PostgreSql;

public sealed class PostgreSqlSchemaQueryProvider : SchemaQueryProvider<NpgsqlConnection>
{
    public PostgreSqlSchemaQueryProvider(NpgsqlConnection connection)
        : base(connection)
    {
    }

    protected override IAsyncEnumerable<TableDefinition> GetTables(EnumerateTableOptions options)
    {
        throw new NotImplementedException();
    }
}
