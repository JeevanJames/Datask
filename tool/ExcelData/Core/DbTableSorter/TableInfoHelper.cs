using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
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

            string includeSchemas = string.Join(',', configuration.IncludeSchemas.Select(x => $"'{x}'"));
            using SqlDataAdapter dataAdapter = new(GetSqlStatementForTableInformation(includeSchemas), sqlConnection);
            await Task.Run(() => dataAdapter.Fill(dataSet, "Tables")).ConfigureAwait(false);

            using SqlDataAdapter refDataAdapter = new(GetSqlStatementForReferences(includeSchemas), sqlConnection);
            await Task.Run(() => refDataAdapter.Fill(dataSet, "Reference")).ConfigureAwait(false);

            using SqlDataAdapter colDataAdapter = new(GetSqlStatementForTableColumnInformation(includeSchemas), sqlConnection);
            await Task.Run(() => colDataAdapter.Fill(dataSet, "Columns")).ConfigureAwait(false);

            // Get DataColumn Information
            DataColumn colTblTableName = dataSet.Tables["Tables"]?.Columns["TableName"]!;
            DataColumn colRefReferenceTableName = dataSet.Tables["Reference"]?.Columns["ReferenceTableName"]!;
            DataColumn colRefTableName = dataSet.Tables["Reference"]?.Columns["TableName"]!;

            // Create Table Relationship
            DataRelation dataRelation = new("tableRelation", colTblTableName, colRefReferenceTableName, true);
            dataSet.Relations.Add(dataRelation);

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

                    colData.FormattedType = colData.MaxLength != 0
                        ? $"({colData.Type}({colData.MaxLength}))"
                        : $"({colData.Type})";

                    tableData.Columns.Add(colData);
                }

                sortedList.Add(tableData.TableName, tableData);
            }

            // Find Reference Table Information For Each Table
            foreach (DataRow row in dataSet.Tables["Tables"]?.Rows!)
            {
                TableData tableInfo = sortedList[row[colTblTableName]?.ToString()!];

                // Get the Reference Tables
                foreach (DataRow childRow in row.GetChildRows(dataRelation))
                {
                    string childTable = childRow[colRefTableName]?.ToString()!;

                    if (childTable.Equals(tableInfo.TableName, StringComparison.OrdinalIgnoreCase))
                        continue;

                    // Add Child Table to Current Table
                    if (tableInfo.ReferencedBy.Contains(childTable))
                        continue;

                    tableInfo.ReferencedBy.Add(childTable);

                    // Add Reference Type Enumeration For Both Referenced and Referenced By Tables
                    tableInfo.AddRefType(RefType.ReferencedBy);
                    sortedList[childTable].AddRefType(RefType.References);
                }

                // Sort Referenced By Tables By Name
                tableInfo.ReferencedBy.ToList().Sort();
            }

            // Perform the Sort with the Custom Sorter
            List<TableData> list = new(sortedList.Values);
            list.Sort(new TableDataComparer(sortedList));

            return list;
        }

        private static string GetSqlStatementForTableColumnInformation(string includeSchemas)
        {
            StringBuilder sb = new();
            sb.Append("select schema_name(tab.schema_id) + \'.\' + tab.name as TableName, ");
            sb.Append("col.column_id, ");
            sb.Append("col.name as ColumnName, ");
            sb.Append("col_s.DATA_TYPE, col_s.ORDINAL_POSITION, ");
            sb.Append("col_s.CHARACTER_MAXIMUM_LENGTH as MaxLength, ");
            sb.Append("col_s.is_nullable, ");
            sb.Append("ku.CONSTRAINT_NAME as PrimaryKeyCol, ");
            sb.Append("case when fk.object_id is not null then '>-' else null end as rel, ");
            sb.Append("schema_name(pk_tab.schema_id) + \'.\' + pk_tab.name as ReferenceTableName, ");
            sb.Append("pk_col.name as ReferenceColumnName, ");
            sb.Append("fk.name as fk_constraint_name ");
            sb.Append("from sys.tables tab ");
            sb.Append("inner join INFORMATION_SCHEMA.COLUMNS col_s on col_s.TABLE_NAME = tab.name and col_s.TABLE_SCHEMA = schema_name(tab.schema_id) ");
            sb.Append("left outer join sys.columns col ");
            sb.Append("on col.object_id = tab.object_id and col.Name = col_s.COLUMN_NAME ");
            sb.Append("left join INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS ku on ku.COLUMN_NAME = col_s.COLUMN_NAME and ku.TABLE_NAME = col_s.TABLE_NAME and ku.TABLE_SCHEMA = col_s.TABLE_SCHEMA ");
            sb.Append("and ku.CONSTRAINT_NAME like '%PK%' ");
            sb.Append("left outer join sys.foreign_key_columns fk_cols ");
            sb.Append("on fk_cols.parent_object_id = tab.object_id ");
            sb.Append("and fk_cols.parent_column_id = col.column_id ");
            sb.Append("left outer join sys.foreign_keys fk ");
            sb.Append("on fk.object_id = fk_cols.constraint_object_id ");
            sb.Append("left outer join sys.tables pk_tab ");
            sb.Append("on pk_tab.object_id = fk_cols.referenced_object_id ");
            sb.Append("left outer join sys.columns pk_col ");
            sb.Append("on pk_col.column_id = fk_cols.referenced_column_id ");
            sb.Append("and pk_col.object_id = fk_cols.referenced_object_id ");

            if (!string.IsNullOrEmpty(includeSchemas))
                sb.Append($"where  SCHEMA_NAME (tab.schema_id) in ({includeSchemas})");
            sb.Append("order by schema_name(tab.schema_id) + '.' + tab.name, col.column_id ");

            return sb.ToString();
        }

        private static string GetSqlStatementForTableInformation(string includeSchemas)
        {
            StringBuilder sb = new();

            sb.Append("SELECT TABLE_SCHEMA + \'.\' + TABLE_NAME AS TableName ");
            sb.Append("FROM INFORMATION_SCHEMA.Tables ");
            sb.Append("WHERE TABLE_TYPE=\'BASE TABLE\' ");
            if (!string.IsNullOrEmpty(includeSchemas))
                sb.Append($"AND TABLE_SCHEMA in ({includeSchemas})");
            sb.Append("ORDER BY TableName ");

            return sb.ToString();
        }

        private static string GetSqlStatementForReferences(string includeSchemas)
        {
            StringBuilder sb = new();

            sb.Append("SELECT  ");
            sb.Append("    OBJECT_SCHEMA_NAME (fkey.referenced_object_id) + \'.\' +  ");
            sb.Append("        OBJECT_NAME (fkey.referenced_object_id) AS ReferenceTableName ");
            sb.Append("    ,COL_NAME(fcol.referenced_object_id, fcol.referenced_column_id) AS ReferenceColumnName ");
            sb.Append("    ,OBJECT_SCHEMA_NAME (fkey.parent_object_id) + \'.\' +  ");
            sb.Append("        OBJECT_NAME(fkey.parent_object_id) AS TableName ");
            sb.Append("    ,COL_NAME(fcol.parent_object_id, fcol.parent_column_id) AS ColumnName ");
            sb.Append("FROM sys.foreign_keys AS fkey ");
            sb.Append("    INNER JOIN sys.foreign_key_columns AS fcol ON fkey.OBJECT_ID = fcol.constraint_object_id ");
            if (!string.IsNullOrEmpty(includeSchemas))
                sb.Append($"where  OBJECT_SCHEMA_NAME (fkey.parent_object_id) in ({includeSchemas})");
            sb.Append("ORDER BY ReferenceTableName, ReferenceColumnName, TableName, ColumnName ");

            return sb.ToString();
        }

        /// <summary>
        /// AddRefType.
        /// </summary>
        /// <param name="tableData">tableData.</param>
        /// <param name="referenceType">referenceType.</param>
        private static void AddRefType(this TableData tableData, RefType referenceType)
        {
            if (tableData.RefType == RefType.RefAndRefBy)
                return;

            tableData.RefType |= referenceType;

            if (tableData.RefType == (RefType.ReferencedBy | RefType.References))
                tableData.RefType = RefType.RefAndRefBy;
        }
    }
}
