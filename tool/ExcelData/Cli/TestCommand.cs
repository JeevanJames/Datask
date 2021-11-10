using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ConsoleFx.CmdLine;

using Datask.Providers;
using Datask.Providers.SqlServer;

using Spectre.Console;

namespace Datask.Tool.ExcelData
{
    [Command("test")]
    public sealed class TestCommand : Command
    {
        public override async Task<int> HandleCommandAsync(IParseResult parseResult)
        {
            await using IProvider provider = new SqlServerProvider(
                @"Server=(localdb)\MSSQLLocalDB;AttachDbFilename=D:\Temp\Northwnd.mdf;Trusted_Connection=Yes;");
            List<TableDefinition> tables = await provider.SchemaQuery
                .EnumerateTables(new EnumerateTableOptions { IncludeColumns = true, IncludeForeignKeys = true, })
                .ToListAsync();

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
                    }
                }

                if (table.ForeignKeys.Count > 0)
                {
                    AnsiConsole.MarkupLine("  [grey]References:[/]");
                    foreach (ForeignKeyDefinition fk in table.ForeignKeys)
                    {
                        AnsiConsole.MarkupLine($"    [green]{fk.ColumnName.EscapeMarkup()}[/] ==> [white]{fk.ReferenceSchema.EscapeMarkup()}.{fk.ReferenceTable.EscapeMarkup()}.{fk.ReferenceColumn.EscapeMarkup()}[/]");
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
}
