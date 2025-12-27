using MknImmiSql.Inner_Logic.V1;
using System;
using System.Collections.Generic;

namespace MknImmiSql.Inner_Table_Logic.V1
{
    public static class ColumnReturner
    {
        public static TableInfo SelectReturned(TableInfo curTable, string[] PossibleColumns, List<int> RowsIndecies)
        {
            List<int> indecies = new List<int>();
            foreach (var x in PossibleColumns)
            {
                string column = x;
                if (column[column.Length - 1] == ',' || column[column.Length - 1] == ';')
                    column = column.Substring(0, column.Length - 1);
                bool hasWhatColumn = false;
                for (int i = 0; i < curTable.TableColumnInfo.Count; ++i)
                    if (curTable.TableColumnInfo[i].Name == column)
                    {
                        hasWhatColumn = true;
                        indecies.Add(i);
                    }
                if (!hasWhatColumn)
                    throw new Exception("400");
            }

            TableInfo newTable = new TableInfo();
            newTable.SerialCoutner = curTable.SerialCoutner - RowsIndecies.Count;

            foreach (int x in indecies)
            {
                newTable.addColumn(curTable.TableColumnInfo[x]);
                if (curTable.TableColumnInfo[x].IsPKey)
                    newTable.HasPrimaryKey = true;
            }

            List<int> indeciesToAdd = new List<int>();
            for (int i = 0; i < indecies.Count; ++i)
                indeciesToAdd.Add(i);

            for (int j = 0; j < curTable.TableRowsInfo.Count; ++j)
            {
                if (RowsIndecies.Contains(j))
                {
                    var tmpRow = curTable.TableRowsInfo[j];
                    List<string> rowsToAdd = new List<string>();
                    foreach (int i in indecies)
                    {
                        string tmp = tmpRow.CurrentRowObjects[i]?.ToString();
                        if (curTable.TableColumnInfo[i].Type.ToLower() == "string")
                            tmp = "'" + tmp + "'";
                        rowsToAdd.Add(tmp);
                    }
                    newTable.addRow(indeciesToAdd, rowsToAdd.ToArray());
                }
            }

            return newTable;
        }
    }
}
