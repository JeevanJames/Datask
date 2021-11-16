using Datask.Providers;

namespace Datask.Tool.ExcelData.Core;

public sealed record DataHelperConfiguration
{
    public string Namespace { get; set; } = null!;

    public string FilePath { get; set; } = null!;

    public IList<Flavors>? Flavors { get; set; }
}

public sealed record Flavors
{
    public string Name { get; set; } = null!;

    public string ExcelPath { get; set; } = null!;

    public IList<TableBindingModel> TableDefinitions { get; } = new List<TableBindingModel>();
}
