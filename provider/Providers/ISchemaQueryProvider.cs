// Copyright (c) 2021 Jeevan James
// This file is licensed to you under the MIT License.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;

namespace Datask.Providers
{
    public interface ISchemaQueryProvider
    {
        public IAsyncEnumerable<TableDefinition> EnumerateTables(EnumerateTableOptions options);
    }

    public sealed record EnumerateTableOptions
    {
        public bool IncludeForeignKeys { get; init; }

        public bool IncludeColumns { get; init; }
    }

    public interface INamedDefinition
    {
        string Name { get; }
    }

    public record TableDefinition : INamedDefinition
    {
        private readonly Lazy<IList<ColumnDefinition>> _columns = new(() => new List<ColumnDefinition>());
        private readonly Lazy<IList<ForeignKeyDefinition>> _foreignKeys = new(() => new List<ForeignKeyDefinition>());

        public TableDefinition(string name, SchemaDefinition schema)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Schema = schema;
        }

        public string Name { get; }

        public SchemaDefinition Schema { get; }

        public IList<ColumnDefinition> Columns => _columns.Value;

        public IList<ForeignKeyDefinition> ForeignKeys => _foreignKeys.Value;
    }

    public record SchemaDefinition : INamedDefinition
    {
        public SchemaDefinition(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public string Name { get; }
    }

    public sealed record ColumnDefinition
    {
        public ColumnDefinition(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public string DbType { get; init; } = null!;

        public Type Type { get; init; } = null!;
    }

    public sealed record ForeignKeyDefinition(TableDefinition Table, string Column);
}
