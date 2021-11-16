
using Microsoft.CodeAnalysis;

namespace Datask.Tool.ExcelData.Generator
{
    internal static class SourceGeneratorExtensions
    {
        internal static bool TryReadAdditionalFilesOption(this GeneratorExecutionContext context,
            AdditionalText additionalText, string property, out string? value)
        {
            return context.AnalyzerConfigOptions.GetOptions(additionalText)
                .TryGetValue($"build_metadata.AdditionalFiles.{property}", out value);
        }
    }
}
