using System.Collections.Generic;

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
