// Copyright (c) 2021 Jeevan James
// This file is licensed to you under the MIT License.
// See the LICENSE file in the project root for more information.

using Datask.Providers.DbManagement;

using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

namespace Datask.Providers.SqlServer;

public sealed class SqlServerDbManagementProvider : DbManagementProvider<SqlConnection>
{
    public SqlServerDbManagementProvider(SqlConnection connection)
        : base(connection)
    {
    }

    protected override bool TryCreateDatabase()
    {
        (Server server, string databaseName) = GetServerAndDatabaseName();

        Database database = server.Databases[databaseName];
        if (database is not null)
            return true;

        database = new Database(server, databaseName);
        database.Create();
        return true;
    }

    protected override void DeleteDatabase()
    {
        (Server server, string databaseName) = GetServerAndDatabaseName();

        Database database = server.Databases[databaseName];
        if (database is not null)
            server.KillDatabase(databaseName);
    }

    protected override bool DatabaseExists()
    {
        (Server server, string databaseName) = GetServerAndDatabaseName();
        return server.Databases[databaseName] is not null;
    }

    private (Server, string) GetServerAndDatabaseName()
    {
        return (new(new ServerConnection(Connection)),
            new SqlConnectionStringBuilder(Connection.ConnectionString).InitialCatalog);
    }
}
