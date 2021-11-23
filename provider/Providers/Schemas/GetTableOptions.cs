// Copyright (c) 2021 Jeevan James
// This file is licensed to you under the MIT License.
// See the LICENSE file in the project root for more information.

using System.Text.RegularExpressions;

namespace Datask.Providers.Schemas;

public sealed class GetTableOptions
{
    private readonly Lazy<IList<Regex>> _includeTables = new(() => new List<Regex>());
    private readonly Lazy<IList<Regex>> _excludeTables = new(() => new List<Regex>());

    public IList<Regex> IncludeTables => _includeTables.Value;

    public IList<Regex> ExcludeTables => _excludeTables.Value;

    public bool IncludeColumns { get; set; }

    public bool IncludeForeignKeys { get; set; }
}
