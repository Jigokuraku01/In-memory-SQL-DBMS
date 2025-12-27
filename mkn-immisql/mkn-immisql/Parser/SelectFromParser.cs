using MknImmiSql.Api.V1;
using MknImmiSql.Inner_Logic.V1;
using MknImmiSql.Inner_Table_Logic.V1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace MknImmiSql.Parser
{
    public static class SelectFromParser
    {
        public static TableQueryInfo Parse(string[] words)
        {

            string possibleColumns = string.Empty;

            for (int i = 1; i < words.Length && words[i].ToLower() != "from"; ++i)
                possibleColumns += words[i] + " ";

            string pattern = @"(?:[^,""']|""[^""]*""|'[^']*')+";
            var matches = Regex.Matches(possibleColumns, pattern);

            string[] possibleColumnsMas = matches
                                       .Cast<Match>()
                                       .Select(m => m.Value.Trim())
                                       .Where(s => !string.IsNullOrEmpty(s))
                                       .ToArray();
            int tmpIndex = -1;
            for (int i = words.Length - 1; i >= 0; --i)
                if (words[i].ToLower() == "from")
                    tmpIndex = i;
            if (tmpIndex == -1)
                throw new Exception("400");

            TableInfo curTable = new TableInfo(TakeTableFromStringSeq.Get(ref words, tmpIndex + 1));


            TableInfo tableForAns = new TableInfo();
            for (int i = 0; i < curTable.TableRowsInfo.Count; ++i)
                tableForAns.TableRowsInfo.Add(new RowInfo(tableForAns));
            List<int> indeciesOfColumns = new List<int>();
            if (String.CompareOrdinal(possibleColumnsMas[0], "*") == 0)
                for (int i = 0; i < curTable.TableColumnInfo.Count; ++i)
                {
                    List<string> possibleColumnstmp = new List<string>();
                    foreach (var col in curTable.TableColumnInfo)
                        possibleColumnstmp.Add(col.Name);
                    possibleColumnsMas = possibleColumnstmp.ToArray();
                }

            foreach (string tmpColNameMaybeWithAs in possibleColumnsMas)
            {
                string[] tmpStringMasWithAs = tmpColNameMaybeWithAs.Split(" ");

                if (tmpStringMasWithAs.Length != 1 && tmpStringMasWithAs[1].ToLower() != "as")
                    throw new Exception("400");
                bool hasAs = false;
                string tmpColName = tmpStringMasWithAs[0];
                if (tmpStringMasWithAs.Length != 1)
                    hasAs = true;
                bool hasCurColumn = false;
                for (int i = 0; i < curTable.TableColumnInfo.Count; ++i)
                    if (String.CompareOrdinal(curTable.TableColumnInfo[i].Name, tmpColName) == 0)
                    {
                        hasCurColumn = true;
                        ColumnInfo tmpColumn = new ColumnInfo(curTable.TableColumnInfo[i]);
                        indeciesOfColumns.Add(i);
                        if (hasAs)
                            tmpColumn.Name = tmpStringMasWithAs[2];

                        foreach (var col in tableForAns.TableColumnInfo)
                            if (String.CompareOrdinal(col.Name, tmpColumn.Name) == 0)
                                throw new Exception("400");
                        tableForAns.addColumn(tmpColumn);
                        for (int j = 0; j < tableForAns.TableRowsInfo.Count; ++j)
                            tableForAns.TableRowsInfo[j].CurrentRowObjects.Add(curTable.TableRowsInfo[j].CurrentRowObjects[i]);
                    }
                if (!hasCurColumn)
                    throw new Exception("400");
            }


            TableQueryInfo ans = new TableQueryInfo();
            tmpIndex = -1;
            for (int i = 0; i < words.Length; ++i)
                if (words[i].ToLower() == "where")
                    tmpIndex = i;

            if (tmpIndex != -1)
            {
                string pred = string.Empty;
                for (int i = tmpIndex + 1; i < words.Length && words[i].ToLower() != "limit" && words[i].ToLower() != "returning"; ++i)
                    pred += words[i];
                int[] indeciesFromWhere = Predicate.DoSelectionByPredicate(curTable, pred);
                for (int i = tableForAns.TableRowsInfo.Count - 1; i >= 0; --i)
                    if (!indeciesFromWhere.Contains(i))
                        tableForAns.TableRowsInfo.RemoveAt(i);
            }


            tmpIndex = -1;
            for (int i = 0; i < words.Length; ++i)
                if (words[i].ToLower() == "limit")
                    tmpIndex = i;

            if (tmpIndex != -1)
            {
                int maxLimit = 0;
                if (!int.TryParse(words[tmpIndex + 1], out maxLimit))
                    throw new Exception("400");
                maxLimit = int.Parse(words[tmpIndex + 1]);
                for (int i = tableForAns.TableRowsInfo.Count - 1; i >= maxLimit; --i)
                    tableForAns.TableRowsInfo.RemoveAt(i);
            }

            tmpIndex = -1;

            for (int i = 0; i < words.Length; ++i)
                if (words[i].ToLower() == "order")
                    tmpIndex = i;
            if (tmpIndex != -1)
            {
                if (words[tmpIndex + 1].ToLower() != "by")
                    throw new Exception("400");
                string colName = words[tmpIndex + 2];
                bool flag = false;
                ColumnInfo columnForSort = new ColumnInfo();
                int curIndex = -1;
                for (int i = 0; i < tableForAns.TableColumnInfo.Count; ++i) {
                    var tmpCol = tableForAns.TableColumnInfo[i];
                    if (string.CompareOrdinal(tmpCol.Name, colName) == 0)
                    {
                        flag = true;
                        columnForSort = tmpCol;
                        curIndex = i;
                    }
                }
                if (!flag)
                    throw new Exception("400");

                if(tmpIndex + 2 != words.Length - 1)
                {
                    bool isAsc = true;
                    if (words[tmpIndex + 3].ToLower() == "desc")
                        isAsc = false;
                    else if (words[tmpIndex + 3].ToLower() == "asc" || tmpIndex + 3 != words.Length - 1)
                        throw new Exception("400");

                    tableForAns.TableRowsInfo.Sort((a, b) =>
                    {
                        if (columnForSort.Type.ToLower() == "string" || columnForSort.Type.ToLower() == "boolean")
                            return String.CompareOrdinal(a.CurrentRowObjects[curIndex]?.ToString(), b.CurrentRowObjects[curIndex]?.ToString());
                        else
                        {
                            if (a.CurrentRowObjects[curIndex] == null && b.CurrentRowObjects[curIndex] == null)
                                return 0;
                            if (a.CurrentRowObjects[curIndex] == null)
                                return -1;
                            if (b.CurrentRowObjects[curIndex] == null)
                                return 1;
                            return int.Parse(a.CurrentRowObjects[curIndex].ToString()) - int.Parse(b.CurrentRowObjects[curIndex].ToString());
                        }
                    });
                    if (!isAsc)
                        tableForAns.TableRowsInfo.Reverse();
                }
            }

            for (int i = 0; i < words.Length; ++i)
                if (words[i].ToLower() == "returning")
                    throw new Exception("400");

            ans.TableInfo = tableForAns;
            ans.Code = 200;

            return ans;
        }
    }
}
