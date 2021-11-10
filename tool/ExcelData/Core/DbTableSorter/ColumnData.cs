// Copyright (c) 2021 Jeevan James
// This file is licensed to you under the MIT License.
// See the LICENSE file in the project root for more information.

using System.Text.Json.Serialization;

namespace Datask.Tool.ExcelData.Core.DbTableSorter
{
    public class ColumnData
    {
        public string Name { get; init; } = null!;

        public bool IsIdentity { get; set; }

        [JsonIgnore]
        public int OrdinalPosition { get; init; }

        public string Type { get; init; } = null!;

        public int? MaxLength { get; init; }

        public bool IsPrimaryKey { get; init; }

        public bool IsNull { get; init; }

        public bool IsForeignKey { get; init; }

        public string? ReferenceTableName { get; init; }

        public string? ReferenceColumnName { get; init; }
    }
}
