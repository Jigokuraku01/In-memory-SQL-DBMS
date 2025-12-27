using MknImmiSql.Api.V1;
using MknImmiSql.Inner_Logic.V1;
using MknImmiSql.Inner_Table_Logic.V1;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;

namespace MknImmiSql.Parser
{
    public static class DeleteFromParser
    {
        public static TableQueryInfo Parse(string[] words)
        {

            if (words[1].ToLower() != "from")
                throw new Exception("400");
            string tableName = words[2];
            TypesInfo.NameTableObr(ref tableName);
            if (!GlocalServeiseContest.ContestSingleton.Instance.Tables.ContainsKey(tableName))
                throw new Exception("409");
            TableInfo curTable = GlocalServeiseContest.ContestSingleton.Instance.Tables[tableName];
            int curPos = 2;
            TableQueryInfo ans = new TableQueryInfo();
            TableInfo obrTable = new TableInfo(curTable);
            obrTable.TableRowsInfo.Clear();
            List<int> indeciesOfDeletedRows = new List<int>();
            for (int i = 0; i < curTable.TableRowsInfo.Count; ++i)
                indeciesOfDeletedRows.Add(i);
            if (curPos + 1 < words.Length && words[curPos + 1].ToLower() == "where")
            {
                curPos++;
                string pred = string.Empty;
                for (int i = curPos + 1; i < words.Length && words[i].ToLower() != "returning"; ++i)
                    pred += words[i];
                indeciesOfDeletedRows = new List<int>(Predicate.DoSelectionByPredicate(curTable, pred));
            }

            foreach (int x in indeciesOfDeletedRows)
            {
                obrTable.TableRowsInfo.Add(curTable.TableRowsInfo[x]);
            }
            indeciesOfDeletedRows.Reverse();
            foreach (int x in indeciesOfDeletedRows)
                curTable.TableRowsInfo.RemoveAt(x);
            indeciesOfDeletedRows.Reverse();
            int indexOfReturning = -1;
            for (int i = 0; i < words.Length; ++i)
                if (words[i].ToLower() == "returning")
                    indexOfReturning = i;
            if (indexOfReturning == -1)
            {
                ans.Code = 200;
                return ans;
            }

            List<string> col = new List<string>();
            for (int i = indexOfReturning + 1; i < words.Length; ++i)
                col.Add(words[i]);

            List<int> indeciesForReturning = new List<int>();
            for (int i = 0; i < obrTable.TableRowsInfo.Count; ++i)
                indeciesForReturning.Add(i);
            ans.TableInfo = ColumnReturner.SelectReturned(obrTable, col.ToArray(), indeciesForReturning);
            ans.Code = 200;
            return ans;


        }
    }
}
