// Copyright (c) 2021 Jeevan James
// This file is licensed to you under the MIT License.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;

namespace Datask.Providers.Schemas;

[DebuggerDisplay("{Table,nq}.{Column,nq}")]
public readonly record struct ForeignKeyDefinition(DbObjectName Table, string Column);
