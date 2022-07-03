// Copyright (c) 2021 Jeevan James
// This file is licensed to you under the MIT License.
// See the LICENSE file in the project root for more information.

using System.Text.RegularExpressions;

namespace Datask.Providers.Schemas;

public sealed class GetTableOptions
{
    private IList<Regex>? _includeTables;
    private IList<Regex>? _excludeTables;

    public IList<Regex> IncludeTables => _includeTables ??= new List<Regex>();

    public bool HasIncludeTables => _includeTables is not null;

    public IList<Regex> ExcludeTables => _excludeTables ??= new List<Regex>();

    public bool HasExcludeTables => _excludeTables is not null;

    public bool IncludeColumns { get; set; }

    public bool IncludeForeignKeys { get; set; }

    public bool SortByForeignKeyDependencies { get; set; }

    public Func<string, string, DbObjectName>? CustomizeTableName { get; set; }
}
