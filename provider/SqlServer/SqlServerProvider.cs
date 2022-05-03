// Copyright (c) 2021 Jeevan James
// This file is licensed to you under the MIT License.
// See the LICENSE file in the project root for more information.

using Microsoft.Data.SqlClient;

namespace Datask.Providers.SqlServer;

public sealed class SqlServerProvider : ProviderBase<SqlConnection,
    SqlServerDbManagementProvider,
    SqlServerSchemaQueryProvider,
    SqlServerScriptGeneratorProvider,
    SqlServerStandardsProvider>
{
    public SqlServerProvider(string connectionString, string? databaseName = null)
        : base(connectionString, cs => new SqlConnection(cs), ValidateConnectionString, databaseName)
    {
    }

    private static string ValidateConnectionString(string connectionString, string? databaseName)
    {
        SqlConnectionStringBuilder builder = new(connectionString);
        if (string.IsNullOrWhiteSpace(builder.InitialCatalog))
        {
            if (databaseName is null)
                throw new ProviderException($"The connection string '{connectionString}' does not specify a database name.");
            builder.InitialCatalog = databaseName;
        }

        return builder.ConnectionString;
    }
}
