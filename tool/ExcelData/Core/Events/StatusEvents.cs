using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datask.Tool.ExcelData.Core.Events
{
#pragma warning disable CA1717 // Only FlagsAttribute enums should have plural names
    public enum StatusEvents
#pragma warning restore CA1717 // Only FlagsAttribute enums should have plural names
    {
        Generate,
        Verify,
        Update,
    }
}
