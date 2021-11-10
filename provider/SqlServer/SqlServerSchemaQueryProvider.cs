// Copyright (c) 2021 Jeevan James
// This file is licensed to you under the MIT License.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using Dapper;

using Datask.Providers.SqlServer.Scripts;

using Microsoft.Data.SqlClient;

namespace Datask.Providers.SqlServer
{
    public sealed class SqlServerSchemaQueryProvider : ISchemaQueryProvider
    {
        private readonly SqlConnection _connection;

        internal SqlServerSchemaQueryProvider(SqlConnection connection)
        {
            _connection = connection;
        }

        public async IAsyncEnumerable<TableDefinition> EnumerateTables(EnumerateTableOptions options)
        {
            // Get all table columns, if required.
            IEnumerable<dynamic>? allTableColumns = null;
            if (options.IncludeColumns)
            {
                string getAllTableColumnsScript = await Script.GetAllTableColumns().ConfigureAwait(false);
                allTableColumns = await _connection.QueryAsync(getAllTableColumnsScript).ConfigureAwait(false);
            }

            IEnumerable<dynamic>? allTableReferences = null;
            if (options.IncludeForeignKeys)
            {
                string getAllTableReferencesScript = await Script.GetAllTableReferences().ConfigureAwait(false);
                allTableReferences = await _connection.QueryAsync(getAllTableReferencesScript).ConfigureAwait(false);
            }

            // Get all tables.
            string getTablesScript = await Script.GetTables().ConfigureAwait(false);
            IEnumerable<dynamic> tables = await _connection.QueryAsync(getTablesScript).ConfigureAwait(false);

            foreach (dynamic table in tables)
            {
                TableDefinition tableDefn = new(table.Name, table.Schema);
                if (allTableColumns is not null)
                    AssignColumns(tableDefn, allTableColumns);
                if (allTableReferences is not null)
                    AssignReferences(tableDefn, allTableReferences);
                yield return tableDefn;
            }
        }

        private static void AssignColumns(TableDefinition tableDefn, IEnumerable<dynamic> columns)
        {
            IEnumerable<ColumnDefinition> columnDefns = columns
                .Where(c => tableDefn.Name.Equals((string)c.Table) && tableDefn.Schema.Equals((string)c.Schema))
                .Select(c =>
                {
                    //(Type Type, DbType DbType) mappings = TypeMappings.GetMappings(c.DbDataType);
                    (Type Type, DbType DbType) mappings = TypeMappings.GetMappings(c.DbDataType);
                    return new ColumnDefinition(c.Name)
                    {
                        DatabaseType = c.DbDataType,
                        Type = mappings.Type,
                        DbType = mappings.DbType,
                        IsNullable = c.IsNullable,
                        IsIdentity = c.IsIdentity,
                    };
                });

            foreach (ColumnDefinition columnDefn in columnDefns)
                tableDefn.Columns.Add(columnDefn);
        }

        private static void AssignReferences(TableDefinition tableDefn, IEnumerable<dynamic> references)
        {
            IEnumerable<ForeignKeyDefinition> fkDefns = references
                .Where(r => tableDefn.Name.Equals((string)r.ReferencingTable) && tableDefn.Schema.Equals((string)r.ReferencingSchema))
                .Select(r => new ForeignKeyDefinition(
                    (string)r.ReferencingColumn,
                    (string)r.ReferencedSchema,
                    (string)r.ReferencedTable,
                    (string)r.ReferencedColumn));

            foreach (ForeignKeyDefinition fkDefn in fkDefns)
                tableDefn.ForeignKeys.Add(fkDefn);
        }
    }

    internal static class TypeMappings
    {
        internal static (Type Type, DbType DbType) GetMappings(string dbType)
        {
            return _mappings.TryGetValue(dbType, out (Type Type, DbType DbType) mapping)
                ? mapping
                : (typeof(object), DbType.Object);
        }

        private static readonly Dictionary<string, (Type Type, DbType DbType)> _mappings = new(StringComparer.OrdinalIgnoreCase)
        {
            ["bigint"] = (typeof(long), DbType.Int64),
            ["binary"] = (typeof(byte[]), DbType.Binary),
            ["bit"] = (typeof(bool), DbType.Boolean),
            ["char"] = (typeof(string), DbType.AnsiStringFixedLength),
            ["date"] = (typeof(DateTime), DbType.Date),
            ["datetime"] = (typeof(DateTime), DbType.DateTime),
            ["datetime2"] = (typeof(DateTime), DbType.DateTime2),
            ["datetimeoffset"] = (typeof(DateTimeOffset), DbType.DateTimeOffset),
            ["decimal"] = (typeof(decimal), DbType.Decimal),
            ["float"] = (typeof(double), DbType.Double),
            ["image"] = (typeof(byte[]), DbType.Binary),
            ["int"] = (typeof(int), DbType.Int32),
            ["money"] = (typeof(decimal), DbType.Decimal),
            ["nchar"] = (typeof(string), DbType.StringFixedLength),
            ["ntext"] = (typeof(string), DbType.String),
            ["numeric"] = (typeof(decimal), DbType.Decimal),
            ["nvarchar"] = (typeof(string), DbType.String),
            ["real"] = (typeof(float), DbType.Single),
            ["rowversion"] = (typeof(byte[]), DbType.Binary),
            ["smalldatetime"] = (typeof(DateTime), DbType.DateTime),
            ["smallint"] = (typeof(short), DbType.Int16),
            ["smallmoney"] = (typeof(decimal), DbType.Decimal),
            ["sql_variant"] = (typeof(object), DbType.Object),
            ["text"] = (typeof(string), DbType.String),
            ["time"] = (typeof(TimeSpan), DbType.Time),
            ["timestamp"] = (typeof(byte[]), DbType.Binary),
            ["tinyint"] = (typeof(byte), DbType.Byte),
            ["uniqueidentifier"] = (typeof(Guid), DbType.Guid),
            ["varbinary"] = (typeof(byte[]), DbType.Binary),
            ["varchar"] = (typeof(string), DbType.AnsiString),
            ["xml"] = (typeof(string), DbType.Xml),
        };
    }
}
