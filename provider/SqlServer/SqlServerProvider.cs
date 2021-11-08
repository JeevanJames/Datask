// Copyright (c) 2021 Jeevan James
// This file is licensed to you under the MIT License.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading.Tasks;

using Microsoft.Data.SqlClient;

namespace Datask.Providers.SqlServer
{
    public sealed class SqlServerProvider : IProvider
    {
        private readonly SqlConnection _connection;
        private readonly Lazy<ISchemaQueryProvider> _schemaQueryProvider;

        public SqlServerProvider(string connectionString)
        {
            _connection = new SqlConnection(connectionString);
            _schemaQueryProvider = new Lazy<ISchemaQueryProvider>(InitializeSchemaQuery);
        }

        public ValueTask DisposeAsync()
        {
            return _connection.DisposeAsync();
        }

        public ISchemaQueryProvider SchemaQuery => _schemaQueryProvider.Value;

        private ISchemaQueryProvider InitializeSchemaQuery()
        {
            return new SqlServerSchemaQueryProvider(_connection);
        }
    }
}
