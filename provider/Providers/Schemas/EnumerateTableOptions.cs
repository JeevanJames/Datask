// Copyright (c) 2021 Jeevan James
// This file is licensed to you under the MIT License.
// See the LICENSE file in the project root for more information.

namespace Datask.Providers.Schemas;

public sealed class EnumerateTableOptions
{
    public bool IncludeColumns { get; set; }

    public bool IncludeForeignKeys { get; set; }
}
