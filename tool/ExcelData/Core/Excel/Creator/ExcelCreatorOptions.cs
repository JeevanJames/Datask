using Datask.Providers;

namespace Datask.Tool.ExcelData.Core.Excel.Creator;

public sealed class ExcelCreatorOptions : ProviderExecutorOptions
{
    private IList<string>? _includeTables;
    private IList<string>? _excludeTables;

    public ExcelCreatorOptions(Type providerType, string connectionString, FileInfo excelFilePath)
        : base(providerType, connectionString)
    {
        ExcelFilePath = excelFilePath ?? throw new ArgumentNullException(nameof(excelFilePath));
    }

    public FileInfo ExcelFilePath { get; }

    public IList<string> IncludeTables => _includeTables ??= new List<string>();

    public bool HasIncludeTables => _includeTables is not null;

    public IList<string> ExcludeTables => _excludeTables ??= new List<string>();

    public bool HasExcludeTables => _excludeTables is not null;
}
