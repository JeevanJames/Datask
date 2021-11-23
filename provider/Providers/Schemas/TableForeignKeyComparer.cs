// Copyright (c) 2021 Jeevan James
// This file is licensed to you under the MIT License.
// See the LICENSE file in the project root for more information.

namespace Datask.Providers.Schemas;

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

        var xForeignKeys = (from c in x.Columns
                            where c.ForeignKey is not null
                            select c.ForeignKey).ToList();
        var yForeignKeys = (from c in y.Columns
                            where c.ForeignKey is not null
                            select c.ForeignKey).ToList();

        if (xForeignKeys.Any(y.Equals))
            return 1;
        if (yForeignKeys.Any(x.Equals))
            return -1;
        if (xForeignKeys.Any(x.Equals) || yForeignKeys.Any(y.Equals))
            return 0;

        return 0;
    }
}
