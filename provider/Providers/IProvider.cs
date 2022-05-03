// Copyright (c) 2021 Jeevan James
// This file is licensed to you under the MIT License.
// See the LICENSE file in the project root for more information.

using Datask.Providers.DbManagement;
using Datask.Providers.Schemas;
using Datask.Providers.Scripts;
using Datask.Providers.Standards;

namespace Datask.Providers;

public interface IProvider : IDisposable
{
    IDbManagementProvider DbManagement { get; }

    ISchemaQueryProvider SchemaQuery { get; }

    IScriptGeneratorProvider ScriptGenerator { get; }

    IStandardsProvider Standards { get; }
}
