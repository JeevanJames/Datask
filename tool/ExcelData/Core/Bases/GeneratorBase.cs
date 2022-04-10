using System.Reflection;
using System.Resources;

using DotLiquid;

namespace Datask.Tool.ExcelData.Core.Bases;

public abstract class GeneratorBase<TGeneratorOptions, TStatusEvents> : Executor<TGeneratorOptions, TStatusEvents>
    where TGeneratorOptions : ExecutorOptions
    where TStatusEvents : Enum
{
    protected GeneratorBase(TGeneratorOptions options)
        : base(options)
    {
    }

    protected static async Task<Template> ParseTemplate(string templateName, Assembly assembly, Type type)
    {
        Stream? resourceStream = assembly.GetManifestResourceStream(type, $"Templates.{templateName}.liquid");
        if (resourceStream is null)
            throw new MissingManifestResourceException($"Templates.{templateName}.liquid");
        using StreamReader reader = new(resourceStream);
        string modelTemplate = await reader.ReadToEndAsync().ConfigureAwait(false);
        return Template.Parse(modelTemplate);
    }

}
