﻿// Copyright (c) 2021 Jeevan James
// This file is licensed to you under the MIT License.
// See the LICENSE file in the project root for more information.

using Datask.Providers.Scripts;

using Microsoft.Data.SqlClient;

namespace Datask.Providers.SqlServer;

public sealed class SqlServerScriptGeneratorProvider : ScriptGeneratorProvider<SqlConnection>
{
    public SqlServerScriptGeneratorProvider(SqlConnection connection)
        : base(connection)
    {
    }
}
