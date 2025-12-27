using Microsoft.Extensions.FileSystemGlobbing.Internal;
using MknImmiSql.Api.V1;
using MknImmiSql.Inner_Logic.V1;
using MknImmiSql.Inner_Table_Logic.V1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace MknImmiSql.Parser
{
    public static class UpdateParser
    {
        public static TableQueryInfo Parse(string[] words)
        {

            string tableName = words[1];
            if (!GlocalServeiseContest.ContestSingleton.Instance.Tables.ContainsKey(tableName))
                throw new Exception("400");
            TableInfo curTable = GlocalServeiseContest.ContestSingleton.Instance.Tables[tableName];

            List<UpdateStruct> updateStructs = new List<UpdateStruct>();
            if (words[2].ToLower() != "set")
                throw new Exception("400");
            string setValues = string.Empty;
            for (int i = 3; i < words.Length && words[i].ToLower() != "where" && words[i].ToLower() != "returning"; ++i)
                setValues += words[i];

            string[] result = Regex.Split(setValues, @"\s*,\s*")
                          .Where(s => !string.IsNullOrEmpty(s))
                          .ToArray();
            foreach (string whatToErase in result)
            {
                string[] parts = Regex.Split(whatToErase, @"\s*=\s*");
                int index = -1;
                ColumnInfo curColumn = new ColumnInfo();
                for (int i = 0; i < curTable.TableColumnInfo.Count; ++i)
                {
                    var tmpColumn = curTable.TableColumnInfo[i];
                    if (String.CompareOrdinal(parts[0], tmpColumn.Name) == 0)
                    {
                        curColumn = tmpColumn;
                        index = i;
                    }
                }
                if (index == -1)
                    throw new Exception("400");
                if (parts[1].ToLower() == "null")
                    parts[1] = null;
                if (!TypesInfo.CompareTypes(curColumn.Type, parts[1]) && parts[1].ToLower() != "default")
                    throw new Exception("400");
                UpdateStruct tmp = new UpdateStruct();

                if (parts[1].ToLower() == "default" && !curColumn.DefaultValue.IsSpecified)
                    throw new Exception("400");
                tmp.value = parts[1];
                if (parts[1].ToLower() == "default")
                {
                    if (curColumn.DefaultValue.IsNull)
                    {
                        tmp.value = null;
                    }
                    else
                        tmp.value = curColumn.DefaultValue.Value;
                }
                if (curColumn.Type.ToLower() == "boolean")
                    tmp.value = tmp.value?.ToString().ToLower();
                tmp.columnIndex = index;
                updateStructs.Add(tmp);
            }

            List<int> indeciesForUpdate = new List<int>();
            for (int i = 0; i < curTable.TableRowsInfo.Count; ++i)
                indeciesForUpdate.Add(i);

            int indexOfWhere = -1;
            for (int i = 0; i < words.Length; ++i)
                if (words[i].ToLower() == "where")
                    indexOfWhere = i;

            if (indexOfWhere != -1)
            {
                string pred = string.Empty;
                for (int i = indexOfWhere + 1; i < words.Length && words[i].ToLower() != "returning"; ++i)
                    pred += words[i];
                indeciesForUpdate = new List<int>(Predicate.DoSelectionByPredicate(curTable, pred));
            }

            foreach (int index in indeciesForUpdate)
            {
                foreach (UpdateStruct tmpStruct in updateStructs)
                    TryToUpdate(curTable, tmpStruct, index);
            }

            int indexOfReturning = -1;
            for (int i = 0; i < words.Length; ++i)
                if (words[i].ToLower() == "returning")
                    indexOfReturning = i;
            TableQueryInfo ans = new TableQueryInfo();
            if (indexOfReturning != -1)
            {
                List<string> tmpColumns = new List<string>();
                for (int i = indexOfReturning + 1; i < words.Length; ++i)
                    tmpColumns.Add(words[i]);
                List<int> indeciesForReturning = new List<int>();
                for (int i = 0; i < curTable.TableRowsInfo.Count; ++i)
                    indeciesForReturning.Add(i);
                ans.TableInfo = ColumnReturner.SelectReturned(curTable, tmpColumns.ToArray(), indeciesForUpdate);
            }
            else
                ans.TableInfo = new TableInfo();
            ans.Code = 200;
            return ans;
        }



        private static void TryToUpdate(TableInfo curTable, UpdateStruct updateStruct, int RowIndex)
        {
            if (curTable.TableColumnInfo[updateStruct.columnIndex].IsPKey)
            {
                for (int i = 0; i < curTable.TableRowsInfo.Count; ++i)
                {
                    if (i != RowIndex && String.CompareOrdinal(curTable.TableRowsInfo[i].CurrentRowObjects[updateStruct.columnIndex].ToString(), updateStruct.value.ToString()) == 0)
                        throw new Exception("409");

                    if (i != RowIndex && curTable.TableColumnInfo[updateStruct.columnIndex].Type.ToLower() == "boolean" && String.CompareOrdinal(curTable.TableRowsInfo[i].CurrentRowObjects[updateStruct.columnIndex].ToString().ToLower(), updateStruct.value.ToString().ToLower()) == 0)
                        throw new Exception("409");
                }

            }
            if (!curTable.TableColumnInfo[updateStruct.columnIndex].IsNullable && updateStruct.value == null)
                throw new Exception("400");

            curTable.TableRowsInfo[RowIndex].CurrentRowObjects[updateStruct.columnIndex] = updateStruct.value;
        }
        private struct UpdateStruct
        {
            public int columnIndex;
            public object value;
        }
    }
}
