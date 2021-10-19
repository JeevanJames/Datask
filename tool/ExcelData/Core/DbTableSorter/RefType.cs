using System;

namespace Datask.Tool.ExcelData.Core.DbTableSorter
{
    [Flags]
    public enum RefType
    {
        None = 0x00,
        ReferencedBy = 0x01,
        RefAndRefBy = 0x02,
        References = 0x04,
    }
}
