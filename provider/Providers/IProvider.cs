// Copyright (c) 2021 Jeevan James
// This file is licensed to you under the MIT License.
// See the LICENSE file in the project root for more information.

using Datask.Providers.Schemas;
using Datask.Providers.Scripts;

namespace Datask.Providers;

public interface IProvider : IDisposable
{
    ISchemaQueryProvider SchemaQuery { get; }

    IScriptGeneratorProvider ScriptGenerator { get; }
}
