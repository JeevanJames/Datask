// Copyright (c) 2021 Jeevan James
// This file is licensed to you under the MIT License.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using Datask.Tool.ExcelData.Core.Scripts;

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

            //Retrieve Tables, Columns and Foreign Key Relationships
            await using SqlConnection connection = new(configuration.ConnectionString);

            //string includeSchemas = string.Join(',', configuration.IncludeSchemas.Select(x => $"'{x}'"));
            using SqlDataAdapter tablesAdapter = new(await SqlScripts.BaseTables().ConfigureAwait(false), connection);
            DataTable tablesDt = new();
            tablesAdapter.Fill(tablesDt);

            using SqlDataAdapter referencesAdapter = new(await SqlScripts.TableReferenceSchema().ConfigureAwait(false),
                connection);
            DataTable referencesDt = new();
            referencesAdapter.Fill(referencesDt);

            using SqlDataAdapter columnsAdapter = new(await SqlScripts.TableColumnSchema().ConfigureAwait(false),
                connection);
            DataTable columnsDt = new();
            columnsAdapter.Fill(columnsDt);

            using DataSet dataSet = new();
            dataSet.Tables.AddRange(new[] { tablesDt, referencesDt, columnsDt });

            // Get DataColumn Information
            DataColumn colTblTableName = tablesDt.Columns["TableName"]!;
            DataColumn colRefReferenceTableName = referencesDt.Columns["ReferenceTableName"]!;
            DataColumn colRefTableName = referencesDt.Columns["TableName"]!;

            // Create Table Relationship
            DataRelation dataRelation = new("tableRelation", colTblTableName, colRefReferenceTableName, true);
            dataSet.Relations.Add(dataRelation);

            SortedList<string, TableData> sortedList = new();
            List<TableData> tableDataList = new();

            // Add All Tables to a List
            foreach (DataRow tableRow in tablesDt.Rows)
            {
                TableData tableData = new()
                {
                    TableName = tableRow[colTblTableName]?.ToString()!,
                };

                DataRow[] dataRows = columnsDt.Select($"TableName = '{tableData.TableName}'");

                foreach (DataRow colRows in dataRows)
                {
                    if (colRows["DATA_TYPE"].ToString() == "timestamp" || colRows["DATA_TYPE"].ToString() == "rowversion")
                        continue;
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
                        IsIdentity = Convert.ToBoolean(colRows["IsIdentity"]),
                    };

                    tableData.Columns.Add(colData);
                }

                tableDataList.Add(tableData);

                sortedList.Add(tableData.TableName, tableData);
            }

            // Find Reference Table Information For Each Table
            foreach (DataRow row in referencesDt.Rows)
            {
                TableData? tableInfo = tableDataList.Find(t => t.TableName == row[colRefTableName].ToString()!);

                if (tableInfo == null)
                    continue;

                TableData? referenceTableData = tableDataList.Find(t => t.TableName == row[colRefReferenceTableName!].ToString());

                if (referenceTableData != null)
                {
                    tableInfo.References.Add(new References
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

        public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> items, Func<T, TKey> property)
        {
            return items.GroupBy(property).Select(x => x.First());
        }
    }
}
