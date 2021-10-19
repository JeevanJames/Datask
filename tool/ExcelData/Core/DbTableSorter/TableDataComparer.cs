using System;
using System.Collections.Generic;

namespace Datask.Tool.ExcelData.Core.DbTableSorter
{
    public class TableDataComparer : IComparer<TableData>
    {
        private readonly SortedList<string, TableData> _sortedList;

        public TableDataComparer(SortedList<string, TableData> sortedList)
        {
            _sortedList = sortedList;
        }

        /// <summary>
        /// Compare two tables and its relation precedence.
        /// </summary>
        /// <param name="x">Table info x.</param>
        /// <param name="y">Table info y.</param>
        /// <returns>int value.</returns>
        public int Compare(TableData? x, TableData? y)
        {
            // Invalid Data
            if ((x == null) || (y == null))
                throw new ArgumentException("Invalid data type");

            // Objects are the Same
            if (x == y)
            {
                PrintLine(x.TableName, "Equal to", y.TableName);
                return 0;
            }

            // Sort by Category Number - Object X is in a LOWER Categroy than Object Y
            if (x.RefType < y.RefType)
                return -1;

            // Sort by Category Number - Object X is in a HIGHER Categroy than Object Y
            if (x.RefType > y.RefType)
                return 1;

            // Sort by Category Number - Object X and Object Y are in the SAME Category
            // Is this a Category 3
            if (x.RefType != RefType.RefAndRefBy)
                return CompareName(x, y);

            if (IsInReferenceList(y.TableName, x))
            {
                PrintLine(x.TableName, "Less Than", y.TableName);
                return -1;
            }

            if (!IsInReferenceList(x.TableName, y))
                return CompareName(x, y);

            PrintLine(x.TableName, "Greater Than", y.TableName);
            return 1;

            // As a last resort sort by name
        }

        /// <summary>
        /// CompareName.
        /// </summary>
        /// <param name="x">Table info x.</param>
        /// <param name="y">Table info y.</param>
        /// <returns>int.</returns>
        private static int CompareName(TableData x, TableData y)
        {
            switch (string.Compare(x.TableName, y.TableName, StringComparison.Ordinal))
            {
                case -1:
                    PrintLine(x.TableName, "Less Than", y.TableName);
                    break;
                case 0:
                    PrintLine(x.TableName, "Equal to", y.TableName);
                    break;
                case 1:
                    PrintLine(x.TableName, "Greater Than", y.TableName);
                    break;
            }

            return string.Compare(x.TableName, y.TableName, StringComparison.Ordinal);
        }

        /// <summary>
        /// IsInReferenceList.
        /// </summary>
        /// <param name="tableName">tableName.</param>
        /// <param name="tableInfo">tableInfo.</param>
        /// <returns>bool.</returns>
        private static bool IsInReferenceList(string tableName, TableData tableInfo)
        {
            // Nothing to compare
            return tableInfo.ReferencedBy.Count != 0 && tableInfo.ReferencedBy.Contains(tableName);

            // This Table is Referenced By TableName.

            // Recursive call
            //foreach (string childTableName in tableInfo.ReferencedBy)
            //{
            //    // Looking for a child of this table
            //    if (IsInReferenceList(tableName, _sortedList[childTableName]))
            //        return true;
            //}
        }

        /// <summary>
        /// PrintLine.
        /// </summary>
        /// <param name="left">left.</param>
        /// <param name="comparison">comparison.</param>
        /// <param name="right">right.</param>
        /// TODO - Need to add status event and remove Console.WriteLine
        private static void PrintLine(string left, string comparison, string right)
        {
            Console.WriteLine(left, comparison, right);
        }
    }
}
