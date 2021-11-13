// Copyright (c) 2021 Jeevan James
// This file is licensed to you under the MIT License.
// See the LICENSE file in the project root for more information.

using System.Data;
using System.Diagnostics;

namespace Datask.Providers;

public interface ISchemaQueryProvider
{
    Task<IList<TableDefinition>> EnumerateTables(EnumerateTableOptions? options = null);
}

public abstract class SchemaQueryProvider<TConnection> : ISchemaQueryProvider
    where TConnection : IDbConnection
{
    protected SchemaQueryProvider(TConnection connection)
    {
        Connection = connection ?? throw new ArgumentNullException(nameof(connection));
    }

    public Task<IList<TableDefinition>> EnumerateTables(EnumerateTableOptions? options = null)
    {
        options ??= new EnumerateTableOptions();
        if (options.IncludeForeignKeys)
            options.IncludeColumns = true;

        return GetTables(options);
    }

    protected abstract Task<IList<TableDefinition>> GetTables(EnumerateTableOptions options);

    protected TConnection Connection { get; }
}

public sealed class EnumerateTableOptions
{
    public bool IncludeColumns { get; set; }

    public bool IncludeForeignKeys { get; set; }
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
        return $"{Schema}.{Name}".GetHashCode();
    }
}

[DebuggerDisplay("{Name,nq} : {DatabaseType,nq}")]
public sealed class ColumnDefinition
{
    public ColumnDefinition(string name)
    {
        Name = name;
    }

    public string Name { get; }

    public string DatabaseType { get; set; } = null!;

    public string CSharpType { get; set; } = null!;

    public Type Type { get; set; } = null!;

    public int MaxLength { get; set; }

    public DbType DbType { get; set; }

    public bool IsNullable { get; set; }

    public bool IsIdentity { get; set; }

    public bool IsPrimaryKey { get; set; }

    //[MemberNotNullWhen(true, nameof(ForeignKey))]
    public bool IsForeignKey => ForeignKey is not null;

    public ForeignKeyDefinition? ForeignKey { get; set; }
}

[DebuggerDisplay("{Schema,nq}.{Table,nq}.{Column,nq}")]
public sealed class ForeignKeyDefinition
{
    public ForeignKeyDefinition(string schema, string table, string column)
    {
        Schema = schema;
        Table = table;
        Column = column;
    }

    public string Schema { get; }

    public string Table { get; }

    public string Column { get; }
}


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
