// Copyright (c) 2021 Jeevan James
// This file is licensed to you under the MIT License.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Datask.Providers
{
    public interface ISchemaQueryProvider
    {
        IAsyncEnumerable<TableDefinition> EnumerateTables(EnumerateTableOptions? options = null);
    }

    public abstract class SchemaQueryProvider<TConnection> : ISchemaQueryProvider
        where TConnection : IDbConnection
    {
        protected SchemaQueryProvider(TConnection connection)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        public IAsyncEnumerable<TableDefinition> EnumerateTables(EnumerateTableOptions? options = null)
        {
            options ??= new EnumerateTableOptions();
            if (options.IncludeForeignKeys && !options.IncludeColumns)
                options = options with { IncludeColumns = true };

            return GetTables(options);
        }

        protected abstract IAsyncEnumerable<TableDefinition> GetTables(EnumerateTableOptions options);

        protected TConnection Connection { get; }
    }

    public sealed record EnumerateTableOptions
    {
        public bool IncludeColumns { get; init; }

        public bool IncludeForeignKeys { get; init; }
    }

    [DebuggerDisplay("{Schema,nq}.{Name,nq}")]
    public sealed record TableDefinition : IEquatable<TableDefinition?>, IEquatable<ForeignKeyDefinition?>
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly Lazy<IList<ColumnDefinition>> _columns = new(() => new List<ColumnDefinition>());

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly Lazy<IList<ForeignKeyDefinition>> _foreignKeys = new(() => new List<ForeignKeyDefinition>());

        public TableDefinition(string name, string schema)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Schema = schema ?? throw new ArgumentNullException(nameof(schema));
        }

        public string Name { get; }

        public string Schema { get; }

        public string FullName => $"{Schema}.{Name}";

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public IList<ColumnDefinition> Columns => _columns.Value;

        public IList<ForeignKeyDefinition> ForeignKeys => _foreignKeys.Value;

        public bool Equals(TableDefinition? other)
        {
            if (other is null)
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return Name == other.Name && Schema == other.Schema;
        }

        public bool Equals(ForeignKeyDefinition? other)
        {
            if (other is null)
                return false;
            return Name == other.Table && Schema == other.Schema;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Schema);
        }
    }

    [DebuggerDisplay("{Name,nq} : {DatabaseType,nq}")]
    public sealed record ColumnDefinition
    {
        public ColumnDefinition(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public string DatabaseType { get; init; } = null!;

        public Type Type { get; init; } = null!;

        public int MaxLength { get; init; }

        public DbType DbType { get; init; }

        public bool IsNullable { get; init; }

        public bool IsIdentity { get; init; }

        public bool IsPrimaryKey { get; init; }

        [MemberNotNullWhen(true, nameof(ForeignKey))]
        public bool IsForeignKey => ForeignKey is not null;

        public ForeignKeyDefinition? ForeignKey { get; set; }
    }

    [DebuggerDisplay("{Schema,nq}.{Table,nq}.{Column,nq}")]
    public sealed record ForeignKeyDefinition(string Schema, string Table, string Column);

    public sealed class TableForeignKeyComparer : Comparer<TableDefinition>
    {
        public override int Compare(TableDefinition? x, TableDefinition? y)
        {
            if (x is null && y is null)
                return 0;
            if (x is null)
                return -1;
            if (y is null)
                return 1;

            if (x.ForeignKeys.Count == 0 && y.ForeignKeys.Count == 0)
                return 0;

            if (x.ForeignKeys.Any(y.Equals))
                return 1;
            if (y.ForeignKeys.Any(x.Equals))
                return -1;
            if (x.ForeignKeys.Any(x.Equals) || y.ForeignKeys.Any(y.Equals))
                return 0;

            return 0;
        }
    }
}
