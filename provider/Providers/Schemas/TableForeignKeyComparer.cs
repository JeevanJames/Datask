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
