using System.Text.Json;
using System.Text.Json.Serialization;

namespace Datask.Tool.ExcelData;

[Command("data-helper", "d")]
[CommandHelp("Generates data helper file with excel table and column information.")]
public sealed class DataHelperCommand : BaseCommand
{
    [Argument(Order = 0)]
    [ArgumentHelp("file name", "The name of the config file to create data helper class.")]
    public FileInfo ConfigFile { get; set; } = null!;

    protected override async Task<int> ExecuteAsync(StatusContext ctx, IParseResult parseResult)
    {
        //Parse config file and fill the data model
        DataHelperConfiguration? config = ParseConfiguration();
        if (config is null || config.Flavors is null || config.Flavors.Count == 0)
        {
            AnsiConsole.MarkupLine($"The config does not contains valid value..");
            return 0;
        }

        DataExtensionBuilder builder = new(config);

        builder.OnStatus += (_, args) =>
        {
            ctx.Status(args.Message ?? string.Empty);
            ctx.Refresh();
        };

        await builder.BuildDataExtensionAsync().ConfigureAwait(false);

        AnsiConsole.MarkupLine($"The file generated successfully.");

        return 0;
    }

    private DataHelperConfiguration? ParseConfiguration()
    {
        string configurationJson = File.ReadAllText(ConfigFile.FullName);
        return JsonSerializer.Deserialize<DataHelperConfiguration>(configurationJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            Converters =
                    {
                        new JsonStringEnumConverter(JsonNamingPolicy.CamelCase),
                    },
        });
    }

    public override string? Validate(IParseResult parseResult)
    {
        if (parseResult.Group != 0)
            return null;

        if (!File.Exists(ConfigFile.FullName))
            return $"[red]The specified config file does not exists.";

        return null;
    }
}
