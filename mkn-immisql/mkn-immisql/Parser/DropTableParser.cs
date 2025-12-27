using MknImmiSql.Api.V1;
using MknImmiSql.Inner_Table_Logic.V1;

namespace MknImmiSql.Parser
{
    public static class DropTableParser
    {
        public static TableQueryInfo Parse(string[] x)
        {
            if (x.Length < 3 || x[0].ToLower() != "drop" || x[1].ToLower() != "table")
                throw new System.Exception("400");
            string name;
            bool WasParseSuccecful;
            var ans = new TableQueryInfo();
            if (x[2].ToLower() == "if")
            {
                if (x[3].ToLower() != "exists" || x.Length != 5)
                    throw new System.Exception("400");
                name = x[4];
                ans.Code = 200;
                TypesInfo.NameTableObr(ref name);
                if (!GlocalServeiseContest.ContestSingleton.Instance.Tables.ContainsKey(name))
                    WasParseSuccecful = false;
                else
                    WasParseSuccecful = true;
                ans.TableInfo = TypesInfo.createDefaultResult(WasParseSuccecful.ToString().ToLower());
                return ans;
            }

            if (x.Length != 3)
                return new TableQueryInfo { Code = -1, TableInfo = null };
            name = x[2];
            TypesInfo.NameTableObr(ref name);
            if (!GlocalServeiseContest.ContestSingleton.Instance.Tables.ContainsKey(name))
            {
                WasParseSuccecful = false;
                string tmp = name;
                TypesInfo.NameNotTableObr(ref tmp);
                if (GlocalServeiseContest.ContestSingleton.Instance.Tables.ContainsKey(tmp))
                    throw new System.Exception("400");
                throw new System.Exception("404");
            }
            else
            {
                ans.Code = 200;
                WasParseSuccecful = true;
                GlocalServeiseContest.ContestSingleton.Instance.Tables.Remove(name);
            }
            ans.TableInfo = TypesInfo.createDefaultResult(WasParseSuccecful.ToString().ToLower());
            return ans;
        }
    }
}
