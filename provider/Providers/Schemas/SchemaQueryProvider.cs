// Copyright (c) 2021 Jeevan James
// This file is licensed to you under the MIT License.
// See the LICENSE file in the project root for more information.

using System.Data;

namespace Datask.Providers.Schemas;

public abstract class SchemaQueryProvider<TConnection> : SubProviderBase<TConnection>, ISchemaQueryProvider
    where TConnection : IDbConnection
{
    protected SchemaQueryProvider(TConnection connection)
        : base(connection)
    {
    }

    public Task<IList<TableDefinition>> EnumerateTables(EnumerateTableOptions? options = null)
    {
        options ??= new EnumerateTableOptions();
        if (options.IncludeForeignKeys)
            options.IncludeColumns = true;

        return GetTables(options);
    }

    protected abstract Task<IList<TableDefinition>> GetTables(EnumerateTableOptions options);
}
