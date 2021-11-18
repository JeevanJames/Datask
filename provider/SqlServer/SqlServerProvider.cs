// Copyright (c) 2021 Jeevan James
// This file is licensed to you under the MIT License.
// See the LICENSE file in the project root for more information.

using Microsoft.Data.SqlClient;

namespace Datask.Providers.SqlServer;

public sealed class SqlServerProvider : ProviderBase<SqlConnection,
    SqlServerSchemaQueryProvider,
    SqlServerScriptGeneratorProvider>
{
    public SqlServerProvider(string connectionString)
        : base(connectionString, cs => new SqlConnection(cs))
    {
    }
}
