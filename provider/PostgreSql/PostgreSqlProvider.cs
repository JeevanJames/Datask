// Copyright (c) 2021 Jeevan James
// This file is licensed to you under the MIT License.
// See the LICENSE file in the project root for more information.

using Npgsql;

namespace Datask.Providers.PostgreSql;

public sealed class PostgreSqlProvider : ProviderBase<NpgsqlConnection,
    PostgreSqlDbManagementProvider,
    PostgreSqlSchemaQueryProvider,
    PostgreSqlScriptGeneratorProvider>
{
    public PostgreSqlProvider(string connectionString, string? databaseName = null)
        : base(connectionString, cs => new NpgsqlConnection(cs), ValidateConnectionString, databaseName)
    {
    }

    private static string ValidateConnectionString(string connectionString, string? databaseName)
    {
        NpgsqlConnectionStringBuilder builder = new(connectionString);
        if (string.IsNullOrWhiteSpace(builder.Database))
        {
            if (databaseName is null)
                throw new InvalidOperationException($"The connection string '{connectionString}' does not specify a database name.");
            builder.Database = databaseName;
        }

        return builder.ConnectionString;
    }
}
