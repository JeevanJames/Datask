namespace Datask.Tool.ExcelData.Core;

public sealed class ExcelGeneratorOptions
{
    public ExcelGeneratorOptions(string connectionString, FileInfo excelFilePath)
    {
        ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        ExcelFilePath = excelFilePath ?? throw new ArgumentNullException(nameof(excelFilePath));
    }

    public string ConnectionString { get; }

    public FileInfo ExcelFilePath { get; }

    public IList<string> IncludeTables { get; } = new List<string>();

    public IList<string> ExcludeTables { get; } = new List<string>();
}
