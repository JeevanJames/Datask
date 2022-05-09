using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Datask.Tool.ExcelData.Core.Generators.CSharpHelpers.Templates;

internal static class CSharpHelperTemplates
{
    private static readonly Dictionary<string, string> _cache = new();

    internal static Task<string> PopulateConsolidatedDataTemplate() => LoadResourceAsync();

    internal static Task<string> PopulateDataTemplate() => LoadResourceAsync();

    internal static Task<string> PopulateFlavorDataTemplate() => LoadResourceAsync();

    internal static Task<string> PopulateTableDataTemplate() => LoadResourceAsync();

    internal static async Task<string> LoadResourceAsync([CallerMemberName] string resourceName = null!)
    {
        if (_cache.TryGetValue(resourceName, out string? content))
            return content;

        Assembly assembly = Assembly.GetExecutingAssembly();
        Stream? resourceStream = assembly.GetManifestResourceStream(typeof(CSharpHelperTemplates), $"{resourceName}.liquid");
        if (resourceStream is null)
            throw new MissingManifestResourceException($"Cannot find resource '{resourceName}'.");

        using StreamReader reader = new(resourceStream);
        content = await reader.ReadToEndAsync();

        _cache.Add(resourceName, content);

        return content;
    }
}
