namespace Datask.Tool.ExcelData.Core.Generators;

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
