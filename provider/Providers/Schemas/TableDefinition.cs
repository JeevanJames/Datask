// Copyright (c) 2021 Jeevan James
// This file is licensed to you under the MIT License.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;

namespace Datask.Providers.Schemas;

[DebuggerDisplay("{FullName,nq}")]
public sealed record TableDefinition : IEquatable<TableDefinition?>, IEquatable<ForeignKeyDefinition?>
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Lazy<IList<ColumnDefinition>> _columns = new(() => new List<ColumnDefinition>());

    public TableDefinition(string name, string schema, string fullName)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Schema = schema ?? throw new ArgumentNullException(nameof(schema));
        FullName = fullName ?? throw new ArgumentNullException(nameof(fullName));
    }

    public string Name { get; }

    public string Schema { get; }

    public string FullName { get; }

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public IList<ColumnDefinition> Columns => _columns.Value;

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
