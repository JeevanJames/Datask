namespace Datask.Tool.ExcelData.Core;

public class DataConfiguration
{
    public string ConnectionString { get; set; } = null!;

    public FileInfo FilePath { get; set; } = null!;

    public IList<string> IncludeTables { get; } = new List<string>();

    public IList<string> ExcludeTables { get; } = new List<string>();
}
