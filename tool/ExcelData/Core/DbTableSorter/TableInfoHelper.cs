using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Datask.Tool.ExcelData.Core.DbTableSorter
{
    public static class TableInfoHelper
    {
#pragma warning disable S4457 // Parameter validation in "async"/"await" methods should be wrapped
        public static async Task<IList<TableData>> GetTableList(DataConfiguration configuration)
#pragma warning restore S4457 // Parameter validation in "async"/"await" methods should be wrapped
        {
            if (configuration is null)
                throw new ArgumentNullException(nameof(configuration));

            SortedList<string, TableData> sortedList = new();

            //Retrieve Tables, Columns and Foreign Key Relationships
            using DataSet dataSet = new();
            await using var sqlConnection = new SqlConnection(configuration.ConnectionString);

            //string includeSchemas = string.Join(',', configuration.IncludeSchemas.Select(x => $"'{x}'"));
            using SqlDataAdapter dataAdapter = new(ReadResource("Datask.Tool.ExcelData.Core.Scripts.BaseTables.sql"), sqlConnection);
            dataAdapter.Fill(dataSet, "Tables");

            using SqlDataAdapter refDataAdapter = new(ReadResource("Datask.Tool.ExcelData.Core.Scripts.TableReferenceSchema.sql"), sqlConnection);
            refDataAdapter.Fill(dataSet, "Reference");

            using SqlDataAdapter colDataAdapter = new(ReadResource("Datask.Tool.ExcelData.Core.Scripts.TableColumnSchema.sql"), sqlConnection);
            colDataAdapter.Fill(dataSet, "Columns");

            // Get DataColumn Information
            DataColumn colTblTableName = dataSet.Tables["Tables"]?.Columns["TableName"]!;
            DataColumn colRefReferenceTableName = dataSet.Tables["Reference"]?.Columns["ReferenceTableName"]!;
            DataColumn colRefTableName = dataSet.Tables["Reference"]?.Columns["TableName"]!;

            // Create Table Relationship
            DataRelation dataRelation = new("tableRelation", colTblTableName, colRefReferenceTableName, true);
            dataSet.Relations.Add(dataRelation);

            List<TableData> tableDataList = new();

            // Add All Tables to a List
            foreach (DataRow tableRow in dataSet.Tables["Tables"]?.Rows!)
            {
                TableData tableData = new()
                {
                    TableName = tableRow[colTblTableName]?.ToString()!,
                };

                DataRow[]? dataRows = dataSet.Tables["Columns"]?.Select($"TableName = '{tableData.TableName}'");

                foreach (DataRow colRows in dataRows!)
                {
                    ColumnData colData = new()
                    {
                        Name = colRows["ColumnName"].ToString()!,
                        ReferenceTableName = colRows["ReferenceTableName"]?.ToString(),
                        ReferenceColumnName = colRows["ReferenceColumnName"]?.ToString(),
                        IsPrimaryKey = colRows["PrimaryKeyCol"] != DBNull.Value,
                        IsForeignKey = colRows["ReferenceTableName"] != DBNull.Value,
                        IsNull = colRows["is_nullable"]?.ToString() == "YES",
                        MaxLength = colRows["MaxLength"] != DBNull.Value
                            ? Convert.ToInt32(colRows["MaxLength"], new NumberFormatInfo())
                            : 0,
                        Type = colRows["DATA_TYPE"].ToString()!,
                        OrdinalPosition = Convert.ToInt32(colRows["ORDINAL_POSITION"], new NumberFormatInfo()),
                    };

                    tableData.Columns.Add(colData);
                }

                tableDataList.Add(tableData);

                sortedList.Add(tableData.TableName, tableData);
            }

            // Find Reference Table Information For Each Table
            foreach (DataRow row in dataSet.Tables["Reference"]?.Rows!)
            {
                TableData? tableInfo = tableDataList.Find(t => t.TableName == row[colRefTableName].ToString()!);

                if (tableInfo == null)
                    continue;

                TableData? referenceTableData = tableDataList.Find(t => t.TableName == row[colRefReferenceTableName!].ToString());

                if (referenceTableData != null)
                {
                    tableInfo.References.Add(new References()
                    {
                        ForeignKey = row["ColumnName"].ToString()!,
                        ReferenceTable = referenceTableData,
                    });
                }
            }

            //Perform Topological sorting
            var sortOrder = GetTopologicalSortOrder(tableDataList).Reverse().ToList();

            return sortOrder.ConvertAll(t => tableDataList[t]);
        }

        private static IEnumerable<int> GetTopologicalSortOrder(List<TableData> tables)
        {
            TableSorter tableSorter = new(tables.Count);
            Dictionary<string, int> indexes = new();

            //add vertices
            for (int i = 0; i < tables.Count; i++)
            {
                indexes[tables[i].TableName.ToLower()] = tableSorter.AddVertex(i);
            }

            //add edges
            for (int i = 0; i < tables.Count; i++)
            {
                var referenceTables = tables[i].References.Select(r => r.ReferenceTable.TableName).ToList();
                if (!referenceTables.Any())
                    continue;

                foreach (string t in referenceTables)
                {
                    tableSorter.AddEdge(i, indexes[t.ToLower()]);
                }
            }

            int[] result = tableSorter.Sort();
            return result;
        }

        public static string ReadResource(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
#pragma warning disable CS8604 // Possible null reference argument.
            StreamReader reader = new(assembly.GetManifestResourceStream(resourceName));
#pragma warning restore CS8604 // Possible null reference argument.
            return reader.ReadToEnd();
        }

        public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> items, Func<T, TKey> property)
        {
            return items.GroupBy(property).Select(x => x.First());
        }
    }
}
