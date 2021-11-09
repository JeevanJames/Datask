using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datask.Tool.ExcelData.Generator
{
    public class DataModel
    {
        //public IList<string> DataFlavours { get; } = new List<string>();

        public string Namespace { get; set; } = null!;

        public string ClassName { get; set; } = "TestData";

        public string ContextName { get; set; } = "EolContext";

        public string ConnectionString { get; set; } = null!;

        //public IList<TableData> TableData { get; } = new List<TableData>();

        public IList<Flavours>? Flavours { get; set; }
    }

    public class Flavours
    {
        public string Name { get; set; } = null!;

        public string ExcelPath { get; set; } = null!;

        public IList<TableData> TableData { get; } = new List<TableData>();
    }

    public class TableData
    {
        public string Schema { get; set; } = null!;

        public string Name { get; set; } = null!;

        public bool ContainsIdentityColumn { get; set; }

        public IList<TableColumns> TableColumns { get; } = new List<TableColumns>();

        public IList<List<string?>> DataRows { get; } = new List<List<string?>>();
    }

    public class TableColumns
    {
        public string Name { get; set; } = null!;

        public string Type { get; set; } = null!;

        public bool IsIdentity { get; set; }

        public string CSharpType { get; set; } = null!;

        public string DbType { get; set; } = null!;

        public IList<object> ColumnRows { get; } = new List<object>();
    }
}
