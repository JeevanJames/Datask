// Copyright (c) 2021 Jeevan James
// This file is licensed to you under the MIT License.
// See the LICENSE file in the project root for more information.

using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Datask.Providers.Schemas;

[DebuggerDisplay("{Name,nq}")]
public sealed record TableDefinition(DbObjectName Name) : IEquatable<ForeignKeyDefinition?>
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Lazy<IList<ColumnDefinition>> _columns = new(() => new List<ColumnDefinition>());

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public IList<ColumnDefinition> Columns => _columns.Value;

    public bool Equals(ForeignKeyDefinition? other)
    {
        if (other is null)
            return false;
        return Name == other.Value.Table;
    }

    public override string ToString()
    {
        return Name.ToString();
    }

    public static bool TryParse(string str, out TableDefinition table)
    {
        if (str is null)
            throw new ArgumentNullException(nameof(str));
        string[] parts = str.Split(new[] { '.' }, 2, StringSplitOptions.None);
        table = new TableDefinition(new DbObjectName(parts[1], parts[0]));
        return true;
    }
}

public sealed class TableDefinitionCollection : Collection<TableDefinition>
{
    public TableDefinitionCollection()
    {
    }

    public TableDefinitionCollection(IList<TableDefinition> list)
        : base(list)
    {
    }

    public void SortByForeignKeyDependencies()
    {
        TableForeignKeyComparer comparer = new();
        for (int i = 0; i < Count - 1; i++)
        {
            for (int j = i + 1; j < Count; j++)
            {
                if (comparer.Compare(this[i], this[j]) > 0)
                    (this[i], this[j]) = (this[j], this[i]);
            }
        }
    }
}
