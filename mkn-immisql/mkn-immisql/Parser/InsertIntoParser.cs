using MknImmiSql.Api.V1;
using MknImmiSql.Inner_Logic.V1;
using MknImmiSql.Inner_Table_Logic.V1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
namespace MknImmiSql.Parser
{
    public static class InsertIntoParser
    {
        public static TableQueryInfo Parse(string[] x)
        {

            TableQueryInfo ans = new TableQueryInfo();
            if (x[1].ToLower() != "into")
                throw new Exception("400");
            string nameOfTable = x[2];
            TypesInfo.NameTableObr(ref nameOfTable);
            if (!GlocalServeiseContest.ContestSingleton.Instance.Tables.ContainsKey(nameOfTable))
                throw new Exception("404");

            var curTable = new TableInfo(GlocalServeiseContest.ContestSingleton.Instance.Tables[nameOfTable]);
            List<int> indecies = new List<int>();
            int curIndex = 3;
            bool f = true;
            while (f)
            {
                string curStr = x[curIndex++];
                if (curStr[curStr.Length - 1] == ')')
                    f = false;
                if (curStr[0] == '(')
                    curStr = curStr.Substring(1);
                if (curStr[curStr.Length - 1] == ')' || curStr[curStr.Length - 1] == ',')
                    curStr = curStr.Substring(0, curStr.Length - 1);
                if (curStr.Length >= 1)
                    TypesInfo.NameNotTableObr(ref curStr);
                bool hasThatColumn = false;
                for (int i = 0; i < curTable.TableColumnInfo.Count; ++i)
                    if (String.CompareOrdinal(curTable.TableColumnInfo[i].Name, curStr) == 0)
                    {
                        indecies.Add(i);
                        hasThatColumn = true;
                        if (curTable.TableColumnInfo[i].Type.ToLower() == "serial")
                            throw new Exception("400");
                    }
                if (!hasThatColumn && curStr != "")
                    throw new Exception("400");
            }
            if (x[curIndex++].ToLower() != "values")
                throw new Exception("400");

            string strWithAllAddInfo = "";

            for (int i = curIndex; i < x.Length && x[i].ToLower() != "returning"; ++i)
                strWithAllAddInfo += x[i];

            string[] StrMas = Regex.Split(strWithAllAddInfo, @",\s*(?![^()]*\))")
                           .Select(part => part.Trim())
                           .Where(part => !string.IsNullOrEmpty(part))
                           .ToArray();

            List<int> inputIndecies = new List<int>();
            foreach (string curStr in StrMas)
            {
                string trimmed = Regex.Replace(curStr, @"^[()\s]+|[()\s]+$", "");
                string[] parts = Regex.Split(trimmed, @",(?=(?:[^""]*""[^""]*"")*[^""]*$)")
                          .Select(part => part.Trim().Trim('"'))
                          .Where(part => !string.IsNullOrEmpty(part))
                          .ToArray();

                curTable.addRow(indecies, parts);
                inputIndecies.Add(curTable.TableRowsInfo.Count - 1);
            }
            GlocalServeiseContest.ContestSingleton.Instance.Tables[nameOfTable] = curTable;

            for (int i = 0; i < x.Length; ++i)
                if (x[i].ToLower() == "returning")
                    curIndex = i;
            TableInfo newTable = new TableInfo(curTable);
            if (curIndex < x.Length && x[curIndex].ToLower() == "returning")
            {
                List<string> returningList = new List<string>();
                for (int i = curIndex + 1; i < x.Length; ++i)
                {
                    string tmp = x[i];
                    if (tmp[tmp.Length - 1] == ',')
                        tmp = tmp.Substring(0, tmp.Length - 1);
                    returningList.Add(tmp);
                }
                newTable = ColumnReturner.SelectReturned(newTable, returningList.ToArray(), inputIndecies);
            }
            ans.TableInfo = newTable;
            ans.Code = 200;
            return ans;

        }
    }
}
