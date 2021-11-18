// Copyright (c) 2021 Jeevan James
// This file is licensed to you under the MIT License.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;

namespace Datask.Providers.Schemas;

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
