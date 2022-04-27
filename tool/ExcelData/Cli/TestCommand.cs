using Datask.Providers.Schemas;
using Datask.Providers.SqlServer;

namespace Datask.Tool.ExcelData;

[Command("test")]
public sealed class TestCommand : Command
{
    private const string ConnectionString =
        @"Server=(localdb)\MSSQLLocalDB;AttachDbFileName=D:\Temp\Northwnd.mdf;Database=NW;Trusted_Connection=Yes;";

#pragma warning disable S1075 // URIs should not be hardcoded
    private const string ExcelFilePath = @"D:\Temp\Northwnd.xlsx";
#pragma warning restore S1075 // URIs should not be hardcoded

    public override async Task<int> HandleCommandAsync(IParseResult parseResult)
    {
        //await ListTables();

        await ListExcelData();

        return 0;
    }

    private static async Task ListTables()
    {
        using IProvider provider = new SqlServerProvider(ConnectionString);

        TableDefinitionCollection tables = await provider.SchemaQuery
            .GetTables(new GetTableOptions { IncludeColumns = true, IncludeForeignKeys = true, });
        tables.SortByForeignKeyDependencies();

        PrintTableDefinitions(tables);
    }

    private static Task ListExcelData()
    {
        using DataExcelWorkbook workbook = new(ExcelFilePath);
        IEnumerable<TableDefinition> tables = workbook.EnumerateTables();
        PrintTableDefinitions(tables);
        return Task.CompletedTask;
    }

    private static void PrintTableDefinitions(IEnumerable<TableDefinition> tables)
    {
        foreach (TableDefinition table in tables)
        {
            AnsiConsole.MarkupLine($"[yellow][[{table.Schema}]].[[{table.Name}]][/]");

            if (table.Columns.Count > 0)
            {
                AnsiConsole.MarkupLine("  [grey]Columns:[/]");
                foreach (ColumnDefinition column in table.Columns)
                {
                    string format = $"    {(column.IsIdentity ? "[CadetBlue][[PK]][/]" : string.Empty)}" +
                                    $"[green]{column.Name.EscapeMarkup()}[/]{(column.IsNullable ? "*" : string.Empty)}: " +
                                    $"[white]{column.DatabaseType.EscapeMarkup()}, {column.ClrType.Name.EscapeMarkup()}, {column.DbType}[/]";
                    AnsiConsole.MarkupLine(format);

                    if (column.ForeignKey is not null)
                    {
                        AnsiConsole.MarkupLine(
                            $"        [green][[FK]][/] ==> [white]{column.ForeignKey.Schema.EscapeMarkup()}.{column.ForeignKey.Table.EscapeMarkup()}.{column.ForeignKey.Column.EscapeMarkup()}[/]");
                    }
                }
            }
        }
    }
}
