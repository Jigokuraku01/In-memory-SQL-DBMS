using MknImmiSql.Api.V1;
using MknImmiSql.Inner_Logic.V1;
using MknImmiSql.Inner_Table_Logic.V1;
using System.Text;

namespace MknImmiSql.Parser
{
    public static class CreateTableParser
    {
        public static TableQueryInfo Parse(string[] x)
        {
            var ans = new TableQueryInfo();

            var TableForAdding = new TableInfo();
            bool WasParseSuccecful;
            if (x[1].ToLower() != "table")
                throw new System.Exception("400");

            bool hasIfNotExists = false;
            if (x[2].ToLower() == "if")
            {
                if (x[3].ToLower() != "not" || x[4].ToLower() != "exists")
                    throw new System.Exception();
                hasIfNotExists = true;
            }

            int pos_with_name = -1;
            if (hasIfNotExists)
                pos_with_name = 5;
            else
                pos_with_name = 2;

            string name = "";
            for (int j = 0; j < x[pos_with_name].Length; ++j)
                if (x[pos_with_name][j] == '(')
                    break;
                else
                    name += x[pos_with_name][j];

            TypesInfo.NameTableObr(ref name);

            TableForAdding.name = name;

            int i = 0;
            StringBuilder stringBuilder = new StringBuilder();
            for (; i < x.Length; ++i)
                if (x[i].Contains('('))
                    break;

            for (int j = i; j < x.Length; ++j)
            {
                stringBuilder.Append(x[j]);
                stringBuilder.Append(" ");
            }
            if (stringBuilder.Length > 0)
            {
                while (stringBuilder[0] != '(')
                    stringBuilder.Remove(0, 1);

                ColumnsParser.Parse(ref TableForAdding, stringBuilder.ToString());
            }



            WasParseSuccecful = false;
            if (!GlocalServeiseContest.ContestSingleton.Instance.Tables.ContainsKey(name))
            {
                GlocalServeiseContest.ContestSingleton.Instance.Tables.Add(name, TableForAdding);
                WasParseSuccecful = true;
            }
            if (!hasIfNotExists && !WasParseSuccecful)
            {
                ans.Code = 409;
            }
            else
            {
                ans.Code = 200;
            }

            if (WasParseSuccecful)
                ans.TableInfo = TypesInfo.createDefaultResult("true");
            else
                ans.TableInfo = TypesInfo.createDefaultResult("false");
            return ans;


        }
    }
}
