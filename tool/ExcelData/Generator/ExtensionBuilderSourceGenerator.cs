using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using Datask.Tool.ExcelData.Core;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Datask.Tool.ExcelData.Generator
{
    [Generator]
    public class ExtensionBuilderSourceGenerator : ISourceGenerator
    {
#pragma warning disable RS2008 // Enable analyzer release tracking
        private static readonly DiagnosticDescriptor _dataBuilderExtensionErrorDescriptor = new(id: "DBE001",
#pragma warning restore RS2008 // Enable analyzer release tracking
            title: "Error in creating data builder extension methods",
            messageFormat: "Error in creating data builder extension methods : '{0}'",
            category: "DataBuilderExtension",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public void Initialize(GeneratorInitializationContext context)
        {
            //Initialization not required.
        }

        public void Execute(GeneratorExecutionContext context)
        {
#if DEBUG
            if (!System.Diagnostics.Debugger.IsAttached)
            {
                System.Diagnostics.Debugger.Launch();
            }
#endif
            CreateDataHelper(context);
        }

        private void CreateDataHelper(GeneratorExecutionContext context)
        {
            DataHelperConfiguration? dataSetupConfiguration = new();
            try
            {
                //Read Excel data
                foreach (AdditionalText? file in context.AdditionalFiles)
                {
                    if (!context.TryReadAdditionalFilesOption(file, "Type", out string? type) || type != "DataBuilderConfiguration")
                        continue;

                    //Parse config file and fill the data model
                    string configurationJson = File.ReadAllText(file.Path);
                    dataSetupConfiguration = JsonSerializer.Deserialize<DataHelperConfiguration>(configurationJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        AllowTrailingCommas = true,
                        ReadCommentHandling = JsonCommentHandling.Skip,
                        Converters =
                    {
                        new JsonStringEnumConverter(JsonNamingPolicy.CamelCase),
                    },
                    });

                    if (dataSetupConfiguration?.Flavors is null)
                        continue;

                    if (string.IsNullOrEmpty(dataSetupConfiguration.FilePath))
                        dataSetupConfiguration.FilePath = Path.Combine(Path.GetTempPath(), "TestDataHelper.cs");

                    //Get absolute path for excel files
                    foreach (Flavors configFlavor in dataSetupConfiguration.Flavors!)
                    {
                        if (!Path.IsPathRooted(configFlavor.ExcelPath))
                        {
                            Uri myUri = new (new Uri(file.Path), configFlavor.ExcelPath);

                            configFlavor.ExcelPath = myUri.LocalPath;
                        }
                    }

                    DataExtensionBuilder builder = new(dataSetupConfiguration);
                    builder.BuildDataExtensionAsync().GetAwaiter().GetResult();

                    //Read file stream and add to the source
                    using FileStream fsSource = new (dataSetupConfiguration.FilePath, FileMode.Open, FileAccess.Read);
                    context.AddSource("TestDataHelper.cs", SourceText.From(fsSource, Encoding.UTF8, canBeEmbedded: true));
                }
            }
#pragma warning disable RCS1075 // Avoid empty catch clause that catches System.Exception.
            catch (Exception exception)
#pragma warning restore RCS1075 // Avoid empty catch clause that catches System.Exception.
            {
                context.ReportDiagnostic(Diagnostic.Create(_dataBuilderExtensionErrorDescriptor, Location.None, $"{exception.Message} {exception.InnerException}{exception.StackTrace}"));
            }
            finally
            {
                if (!string.IsNullOrEmpty(dataSetupConfiguration?.FilePath))
                    File.Delete(dataSetupConfiguration?.FilePath);
            }
        }

        private string GetRelativePath(string filespec, string folder)
        {
            Uri pathUri = new(filespec);
            // Folders must end in a slash
            if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                folder += Path.DirectorySeparatorChar;
            }
            Uri folderUri = new (folder);
            return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
        }
    }
}
