// Copyright (c) 2021 Jeevan James
// This file is licensed to you under the MIT License.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;

using Npgsql;

namespace Datask.Providers.PostgreSql
{
    public sealed class PostgreSqlSchemaQueryProvider : ISchemaQueryProvider
    {
        private readonly NpgsqlConnection _connection;

        internal PostgreSqlSchemaQueryProvider(NpgsqlConnection connection)
        {
            _connection = connection;
        }

        public IAsyncEnumerable<TableDefinition> EnumerateTables(EnumerateTableOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
