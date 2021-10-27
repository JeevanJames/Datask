using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datask.Tool.ExcelData.Generator
{
    public class DataModel
    {
        public IList<string> DataFlavours { get; } = new List<string>();

        public string Namespace { get; set; } = null!;

        public string ClassName { get; set; } = "TestData";

        public string ContextName { get; set; } = "EolContext";

        public IList<TableData> TableData { get; } = new List<TableData>();
    }

    public class TableData
    {
        public string Name { get; set; } = null!;

        public IList<TableColumns> TableColumns { get; } = new List<TableColumns>();
    }

    public class TableColumns
    {
        public string Name { get; set; } = null!;

        public string Type { get; set; } = null!;

        public IList<object> ColumnRows { get; } = new List<object>();
    }
}
