using MknImmiSql.Inner_Logic.V1;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MknImmiSql.Inner_Table_Logic.V1
{
    public static class Predicate
    {
        public static int[] DoSelectionByPredicate(TableInfo table, string PredStr)
        {
            string pattern = @"(!=)|(>=)|(<=)|(>)|(<)|(=)";
            Match match = Regex.Match(PredStr, pattern);

            if (!match.Success)
                throw new Exception("400");


            int opPos = match.Index;
            int opLength = match.Length;
            string op = match.Value;

            string left = PredStr.Substring(0, opPos).TrimEnd();

            string right = PredStr.Substring(opPos + opLength).TrimStart();

            List<int> ans = new List<int>();

            for (int i = 0; i < table.TableRowsInfo.Count; ++i)
            {
                if (Comp(op, table, left, right, i))
                    ans.Add(i);
            }

            return ans.ToArray();
        }

        private static bool Comp(string oper, TableInfo curTable, string left, string right, int pos)
        {
            string actualLeft = left, actualRight = right;

            for (int i = 0; i < curTable.TableColumnInfo.Count; ++i)
                if (String.CompareOrdinal(curTable.TableColumnInfo[i].Name, left) == 0)
                    actualLeft = curTable.TableRowsInfo[pos].CurrentRowObjects[i].ToString();
            for (int i = 0; i < curTable.TableColumnInfo.Count; ++i)
                if (String.CompareOrdinal(curTable.TableColumnInfo[i].Name, right) == 0)
                    actualRight = curTable.TableRowsInfo[pos].CurrentRowObjects[i].ToString();

            if (String.CompareOrdinal(actualLeft.ToLower(), "null") == 0)
                actualLeft = null;
            if (String.CompareOrdinal(actualRight.ToLower(), "null") == 0)
                actualRight = null;
            TypesInfo.NameNotTableObr(ref actualLeft);
            TypesInfo.NameNotTableObr(ref actualRight);
            if (String.CompareOrdinal(oper, "=") == 0)
                return String.CompareOrdinal(actualLeft, actualRight) == 0;
            if (String.CompareOrdinal(oper, "!=") == 0)
                return String.CompareOrdinal(actualLeft, actualRight) != 0;
            int numberLeft, numberRight = 0;
            bool isCmpForInt = false;
            if (int.TryParse(actualLeft, out numberLeft) && int.TryParse(actualRight, out numberRight))
            {
                isCmpForInt = true;
                numberRight = int.Parse(actualRight);
                numberLeft = int.Parse(actualLeft);
            }

            if (String.CompareOrdinal(">", oper) == 0)
            {
                if (isCmpForInt)
                    return numberLeft > numberRight;
                else
                    return String.CompareOrdinal(actualLeft, actualRight) > 0;
            }
            else if (String.CompareOrdinal("<", oper) == 0)
            {
                if (isCmpForInt)
                    return numberLeft < numberRight;
                else
                    return String.CompareOrdinal(actualLeft, actualRight) < 0;
            }
            else if (String.CompareOrdinal(">=", oper) == 0)
            {
                if (isCmpForInt)
                    return numberLeft >= numberRight;
                else
                    return String.CompareOrdinal(actualLeft, actualRight) >= 0;
            }
            else if (String.CompareOrdinal("<=", oper) == 0)
            {
                if (isCmpForInt)
                    return numberLeft <= numberRight;
                else
                    return String.CompareOrdinal(actualLeft, actualRight) <= 0;
            }
            else
                throw new Exception("400");
        }
    }
}
