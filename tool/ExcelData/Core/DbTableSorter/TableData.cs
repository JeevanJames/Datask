// Copyright (c) 2021 Jeevan James
// This file is licensed to you under the MIT License.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Data;

namespace Datask.Tool.ExcelData.Core.DbTableSorter
{
    public class TableData
    {
        public string Schema { get; init; } = null!;

        public string TableName { get; init; } = null!;

        public IList<ColumnData> Columns { get; } = new List<ColumnData>();

        public IList<References> References { get; set; } = new List<References>();
    }

    public class References
    {
        public string ForeignKey { get; set; } = null!;

        public TableData ReferenceTable { get; set; } = null!;
    }
}
