using Microsoft.CodeAnalysis;

namespace Datask.Tool.ExcelData.Generator.Extensions
{
    internal static class SourceGeneratorExtensions
    {
        internal static bool TryReadGlobalOption(this GeneratorExecutionContext context, string property, out string? value) =>
            context.AnalyzerConfigOptions.GlobalOptions.TryGetValue($"build_property.{property}", out value);

        internal static bool TryReadAdditionalFilesOption(this GeneratorExecutionContext context, AdditionalText additionalText,
            string property, out string? value) =>
            context.AnalyzerConfigOptions.GetOptions(additionalText)
                .TryGetValue($"build_metadata.AdditionalFiles.{property}", out value);
    }
}
