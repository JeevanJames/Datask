// Copyright (c) 2021 Jeevan James
// This file is licensed to you under the MIT License.
// See the LICENSE file in the project root for more information.

using Datask.Providers.Schemas;

using Npgsql;

namespace Datask.Providers.PostgreSql;

public sealed class PostgreSqlSchemaQueryProvider : SchemaQueryProvider<NpgsqlConnection>
{
    public PostgreSqlSchemaQueryProvider(NpgsqlConnection connection)
        : base(connection)
    {
    }

    protected override Task<TableDefinitionCollection> GetTablesTask(GetTableOptions options)
    {
        throw new NotImplementedException();
    }

    public override string GetFullTableName(string schema, string table)
    {
        return $@"""{schema}"".""{table}""";
    }
}
