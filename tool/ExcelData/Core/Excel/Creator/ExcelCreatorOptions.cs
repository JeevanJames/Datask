namespace Datask.Tool.ExcelData.Core.Excel.Creator;

public sealed class ExcelCreatorOptions : ExecutorOptions
{
    public ExcelCreatorOptions(string connectionString, FileInfo excelFilePath)
    {
        ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        ExcelFilePath = excelFilePath ?? throw new ArgumentNullException(nameof(excelFilePath));
    }

    public string ConnectionString { get; }

    public FileInfo ExcelFilePath { get; }

    public IList<string> IncludeTables { get; } = new List<string>();

    public IList<string> ExcludeTables { get; } = new List<string>();
}
