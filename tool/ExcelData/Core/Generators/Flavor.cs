namespace Datask.Tool.ExcelData.Core.Generators;

/// <summary>
///     Represents the data stored in a specific Excel file, called a data flavor.
/// </summary>
public sealed class Flavor
{
    public Flavor(string name, string excelFilePath)
    {
        Name = name;
        ExcelFilePath = excelFilePath;
    }

    /// <summary>
    ///     Gets the name of the flavor.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     Gets the path to the Excel file containing the data.
    /// </summary>
    public string ExcelFilePath { get; }

    public IList<TableBindingModel> TableDefinitions { get; } = new List<TableBindingModel>();
}