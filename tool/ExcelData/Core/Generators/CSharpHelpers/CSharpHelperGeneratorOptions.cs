namespace Datask.Tool.ExcelData.Core.Generators.CSharpHelpers;

public sealed class CSharpHelperGeneratorOptions : FlavorGeneratorOptions
{
    public CSharpHelperGeneratorOptions(string ns, string filePath, params Flavor[] flavors)
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
