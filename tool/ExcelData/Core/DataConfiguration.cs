using System.Collections.Generic;
using System.IO;

namespace Datask.Tool.ExcelData.Core
{
    public class DataConfiguration
    {
        public string ConnectionString { get; init; } = null!;

        public FileInfo FilePath { get; init; } = null!;

        public IList<string> IncludeTables { get; } = new List<string>();

        public IList<string> ExcludeTables { get; } = new List<string>();
    }
}
