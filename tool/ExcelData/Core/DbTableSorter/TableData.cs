using System.Collections.Generic;

namespace Datask.Tool.ExcelData.Core.DbTableSorter
{
    public class TableData
    {
        public string Schema { get; init; } = null!;

        public string TableName { get; init; } = null!;

        public RefType RefType { get; set; }

        public IList<string> ReferencedBy { get; } = new List<string>();

        public IList<ColumnData> Columns { get; } = new List<ColumnData>();
    }
}
