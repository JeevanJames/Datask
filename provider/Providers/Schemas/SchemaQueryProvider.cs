// Copyright (c) 2021 Jeevan James
// This file is licensed to you under the MIT License.
// See the LICENSE file in the project root for more information.

using System.Data;
using System.Text.RegularExpressions;

namespace Datask.Providers.Schemas;

public abstract class SchemaQueryProvider<TConnection> : SubProviderBase<TConnection>, ISchemaQueryProvider
    where TConnection : IDbConnection
{
    protected SchemaQueryProvider(TConnection connection)
        : base(connection)
    {
    }

    public Task<TableDefinitionCollection> GetTables(GetTableOptions? options = null)
    {
        options ??= new GetTableOptions();
        if (options.IncludeForeignKeys)
            options.IncludeColumns = true;

        return GetTablesTask(options);
    }

    protected abstract Task<TableDefinitionCollection> GetTablesTask(GetTableOptions options);

    protected virtual IEnumerable<TableDefinition> FilterTables(IList<TableDefinition> allTables, GetTableOptions options)
    {
        IEnumerable<TableDefinition> tables = allTables;

        foreach (Regex pattern in options.IncludeTables)
            tables = tables.Where(t => pattern.IsMatch(t.FullName));

        foreach (Regex pattern in options.ExcludeTables)
            tables = tables.Where(t => !pattern.IsMatch(t.FullName));

        return tables;
    }

    public virtual string GetFullTableName(string schema, string table)
    {
        return schema + "." + table;
    }

}
