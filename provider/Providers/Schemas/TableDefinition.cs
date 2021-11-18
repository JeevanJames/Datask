// Copyright (c) 2021 Jeevan James
// This file is licensed to you under the MIT License.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;

namespace Datask.Providers.Schemas;

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
