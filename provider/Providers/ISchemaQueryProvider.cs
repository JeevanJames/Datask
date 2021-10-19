// Copyright (c) 2021 Jeevan James
// This file is licensed to you under the MIT License.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;

namespace Datask.Providers
{
    public interface ISchemaQueryProvider
    {
        public IAsyncEnumerable<TableDefinition> EnumerateTables();
    }

    public interface INamedDefinition
    {
        string Name { get; }
    }

    public record TableDefinition : INamedDefinition
    {
        public TableDefinition(string name, SchemaDefinition schema)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Schema = schema;
        }

        public string Name { get; }

        public SchemaDefinition Schema { get; }
    }

    public record SchemaDefinition : INamedDefinition
    {
        public SchemaDefinition(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public string Name { get; }
    }
}
