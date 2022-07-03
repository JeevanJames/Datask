using Datask.Providers;

namespace Datask.Tool.ExcelData.Core.Excel.Validator;

public sealed class ExcelValidatorOptions : ProviderExecutorOptions
{
    public ExcelValidatorOptions(Type providerType, string connectionString, FileInfo excelFilePath)
        : base(providerType, connectionString)
    {
        ExcelFilePath = excelFilePath ?? throw new ArgumentNullException(nameof(excelFilePath));
    }

    public FileInfo ExcelFilePath { get; }
}
