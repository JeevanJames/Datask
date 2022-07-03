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

    public async Task<TableDefinitionCollection> GetTables(GetTableOptions? options = null)
    {
        options ??= new GetTableOptions();
        if (options.IncludeForeignKeys)
            options.IncludeColumns = true;

        TableDefinitionCollection tables = await GetTablesTask(options);

        if (options.SortByForeignKeyDependencies)
            tables.SortByForeignKeyDependencies();

        return tables;
    }

    protected abstract Task<TableDefinitionCollection> GetTablesTask(GetTableOptions options);

    /// <summary>
    ///     Helper method to filter a list of tables using the <see cref="GetTableOptions.IncludeTables"/>
    ///     and <see cref="GetTableOptions.ExcludeTables"/> criteria.
    /// </summary>
    /// <param name="allTables">The list of tables to filter.</param>
    /// <param name="options">The <see cref="GetTableOptions"/> instance that contains the filter criteria.</param>
    /// <returns>A sequence of filtered tables.</returns>
    protected virtual IEnumerable<TableDefinition> FilterTables(IList<TableDefinition> allTables, GetTableOptions options)
    {
        IEnumerable<TableDefinition> tables = allTables;

        if (options.HasIncludeTables)
        {
            foreach (Regex pattern in options.IncludeTables)
                tables = tables.Where(t => pattern.IsMatch(t.Name.ToString()));
        }

        if (options.HasExcludeTables)
        {
            foreach (Regex pattern in options.ExcludeTables)
                tables = tables.Where(t => !pattern.IsMatch(t.Name.ToString()));
        }

        return tables;
    }
}
