using Datask.Tool.ExcelData.Core.Bases;

namespace Datask.Tool.ExcelData.Core;

/// <summary>
///     Represents the options for generators that use the <see cref="Flavor"/> concept.
/// </summary>
public abstract class FlavorGeneratorOptions : ExecutorOptions
{
    private readonly Lazy<IList<Flavor>> _flavors;

    protected FlavorGeneratorOptions(params Flavor[] flavors)
    {
        if (flavors is null)
            throw new ArgumentNullException(nameof(flavors));

        _flavors = flavors.Length > 0 ? new(new List<Flavor>(flavors)) : new(() => new List<Flavor>());
    }

    /// <summary>
    ///     Gets the data flavors to generate.
    /// </summary>
    public IList<Flavor> Flavors => _flavors.IsValueCreated ? _flavors.Value : Array.Empty<Flavor>();
}

public sealed class ExtensionMethodsGeneratorOptions : FlavorGeneratorOptions
{
    public ExtensionMethodsGeneratorOptions(string ns, string filePath, params Flavor[] flavors)
        : base(flavors)
    {
        Namespace = ns ?? throw new ArgumentNullException(nameof(ns));
        FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
    }

    /// <summary>
    ///     Gets the C# namespace for the generated code.
    /// </summary>
    public string Namespace { get; }

    /// <summary>
    ///     Gets the path to the generated C# file.
    /// </summary>
    public string FilePath { get; }
}

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
