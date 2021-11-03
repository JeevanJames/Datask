﻿using System.Text.Json.Serialization;

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
