using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Datask.Tool.ExcelData.Core.Generators.CSharpHelpers.Templates;

internal static class CSharpHelperTemplates
{
    private static readonly Dictionary<string, string> _cache = new();

    internal static ValueTask<string> PopulateConsolidatedDataTemplate => LoadResourceAsync();

    internal static ValueTask<string> PopulateDataTemplate => LoadResourceAsync();

    internal static ValueTask<string> PopulateFlavorDataTemplate => LoadResourceAsync();

    internal static ValueTask<string> PopulateTableDataTemplate => LoadResourceAsync();

    private static async ValueTask<string> LoadResourceAsync([CallerMemberName] string resourceName = "")
    {
        if (_cache.TryGetValue(resourceName, out string? content))
            return content;

        Assembly assembly = Assembly.GetExecutingAssembly();
        Stream? resourceStream = assembly.GetManifestResourceStream(typeof(CSharpHelperTemplates), $"{resourceName}.liquid");
        if (resourceStream is null)
            throw new MissingManifestResourceException($"Cannot find resource '{typeof(CSharpHelperTemplates).Namespace}.{resourceName}'.");

        using StreamReader reader = new(resourceStream);
        content = await reader.ReadToEndAsync();

        _cache.Add(resourceName, content);

        return content;
    }
}
