using MknImmiSql.Inner_Logic.V1;
using MknImmiSql.Inner_Table_Logic.V1;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Text.RegularExpressions;
namespace MknImmiSql.Parser
{
    public class TableQueryInfo
    {
        public TableInfo TableInfo { get; set; }
        public Int32 Code { get; set; }
        public TableQueryInfo()
        {
            TableInfo = new TableInfo();
        }

    }


    public static class CentralParser
    {
        public static HashSet<string> posCommands = new HashSet<string>()
        {
            "CREATE",
            "DELETE",
            "DROP",
            "INSERT",
            "SELECT",
            "UPDATE"
        };
        public static TableQueryInfo ParseQuery(string inpQuery)
        {
            try
            {
                var x = Regex.Matches(inpQuery, @"""(.*?)""|'(.*?)'|[^ \s]+");
                List<String> tmpLis = new List<String>();
                foreach (Match m in x)
                    tmpLis.Add(m.Value);
                string[] words = tmpLis.ToArray();
                switch (words[0].ToLower())
                {
                    case "drop":
                        return DropTableParser.Parse(words);
                    case "create":
                        return CreateTableParser.Parse(words);
                    case "insert":
                        return InsertIntoParser.Parse(words);
                    case "delete":
                        return DeleteFromParser.Parse(words);
                    case "update":
                        return UpdateParser.Parse(words);
                    case "select":
                        return SelectFromParser.Parse(words);

                }
                throw new Exception("400");
            }
            catch (Exception e)
            {
                Int32 number;
                TableQueryInfo ans = new TableQueryInfo();
                if (int.TryParse(e.Message, out number) == false || number == 400)
                {
                    ans.Code = 400;
                    ans.TableInfo = TypesInfo.createDefaultResult("false");
                    return ans;
                }

                number = int.Parse(e.Message);
                ans.Code = number;
                ans.TableInfo = TypesInfo.createDefaultResult("false");
                return ans;
            }

        }
    }
}
