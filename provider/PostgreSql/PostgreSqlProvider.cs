// Copyright (c) 2021 Jeevan James
// This file is licensed to you under the MIT License.
// See the LICENSE file in the project root for more information.

using Npgsql;

namespace Datask.Providers.PostgreSql;

public sealed class PostgreSqlProvider : ProviderBase<NpgsqlConnection,
    PostgreSqlSchemaQueryProvider,
    PostgreSqlScriptGeneratorProvider>
{
    public PostgreSqlProvider(string connectionString)
        : base(connectionString, cs => new NpgsqlConnection(cs))
    {
    }
}
