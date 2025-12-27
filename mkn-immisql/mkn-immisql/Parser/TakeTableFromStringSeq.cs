using MknImmiSql.Api.V1;
using MknImmiSql.Inner_Logic.V1;
using MknImmiSql.Inner_Table_Logic.V1;
using System.Collections.Generic;
using System.Text;

namespace MknImmiSql.Parser
{
    public static class TakeTableFromStringSeq
    {

        public static TableInfo Get(ref string[] words, int startIndex)
        {
            string posName = new string(words[startIndex]);
            if (GlocalServeiseContest.ContestSingleton.Instance.Tables.ContainsKey(posName))
                return GlocalServeiseContest.ContestSingleton.Instance.Tables[posName];
            if (posName[0] == '(')
                posName = posName.Substring(1);
            if (!CentralParser.posCommands.Contains(posName.ToUpper()))
                throw new System.Exception("404");
            StringBuilder sb = new StringBuilder();
            TableQueryInfo ans = new TableQueryInfo();
            int end = words.Length;
            if (words[startIndex][0] != '(')
            {
                for (int i = startIndex; i < words.Length; ++i)
                {
                    sb.Append(words[i]);
                    sb.Append(" ");
                }
            }
            else
            {
                int i = startIndex;
                while (i < words.Length && words[i][words[i].Length - 1] != ')')
                {
                    sb.Append(words[i]);
                    sb.Append(" ");
                    ++i;
                }
                if(i < words.Length)
                    sb.Append(words[i]);
                end = i;
            }
            string tmp = sb.ToString();
            if (tmp[0] == '(' && tmp[tmp.Length - 1] == ')')
                tmp = tmp.Substring(1, tmp.Length - 2);
            ans = CentralParser.ParseQuery(tmp);

            List<string> newMas = new List<string>();

            for(int i = 0; i < startIndex; ++i)
                newMas.Add(words[i]);
            for(int i = end + 1; i < words.Length; ++i)
                newMas.Add(words[i]);
            words = newMas.ToArray();
            if (ans.Code != 200)
                throw new System.Exception(ans.Code.ToString());
            return ans.TableInfo;
        }
    }
}
