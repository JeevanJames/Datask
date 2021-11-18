using Datask.Providers.Schemas;
using Datask.Providers.SqlServer;

namespace Datask.Tool.ExcelData;

[Command("test")]
public sealed class TestCommand : Command
{
    public override async Task<int> HandleCommandAsync(IParseResult parseResult)
    {
        using IProvider provider = new SqlServerProvider(
            @"Server=(localdb)\MSSQLLocalDB;AttachDbFilename=D:\Temp\Northwnd.mdf;Trusted_Connection=Yes;");
        IList<TableDefinition> tables = await provider.SchemaQuery
            .EnumerateTables(new EnumerateTableOptions { IncludeColumns = true, IncludeForeignKeys = true, });

        Sort(tables);

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
                                    $"[white]{column.DatabaseType.EscapeMarkup()}, {column.Type.Name.EscapeMarkup()}, {column.DbType}[/]";
                    AnsiConsole.MarkupLine(format);

                    if (column.ForeignKey is not null)
                    {
                        AnsiConsole.MarkupLine(
                            $"        [green][[FK]][/] ==> [white]{column.ForeignKey.Schema.EscapeMarkup()}.{column.ForeignKey.Table.EscapeMarkup()}.{column.ForeignKey.Column.EscapeMarkup()}[/]");
                    }
                }
            }
        }

        return 0;
    }

    private static void Sort(IList<TableDefinition> tables)
    {
        TableForeignKeyComparer comparer = new();
        for (int i = 0; i < tables.Count - 1; i++)
        {
            for (int j = i + 1; j < tables.Count; j++)
            {
                if (comparer.Compare(tables[i], tables[j]) > 0)
                {
                    (tables[i], tables[j]) = (tables[j], tables[i]);
                }
            }
        }
    }
}
