using MknImmiSql.Inner_Logic.V1;
using MknImmiSql.Inner_Table_Logic.V1;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MknImmiSql.Parser
{
    public static class ColumnsParser
    {
        public static void Parse(ref TableInfo tableInfo, string columnsInfo)
        {
            string[] words = columnsInfo.Split(new char[] { '\n', '\r', '(', ')', ';', ',' }, StringSplitOptions.RemoveEmptyEntries);


            for (int i = 0; i < words.Length; ++i)
            {
                if (words[i] == "" || words[i].Length < 2)
                    continue;

                ColumnInfo newColumn = new ColumnInfo();

                newColumn.DefaultValue.IsSpecified = false;
                newColumn.DefaultValue.Value = "";

                var x = Regex.Matches(words[i], @"""(.*?)""|'(.*?)'|[^ \s]+");
                List<String> tmpLis = new List<String>();
                foreach (Match m in x)
                    tmpLis.Add(m.Value);
                string[] tt = tmpLis.ToArray();

                List<string> tmpStringMas = new List<string>(tt);
                while (tmpStringMas[0].Length == 0 || tmpStringMas[0] == " ")
                    tmpStringMas.RemoveAt(0);

                string name = tmpStringMas[0];
                TypesInfo.NameNotTableObr(ref name);
                newColumn.Name = name;

                foreach (var tmpCloumn in tableInfo.TableColumnInfo)
                    if (tmpCloumn.Name == newColumn.Name)
                        throw new Exception();

                if (!TypesInfo.IsPossibleType(tmpStringMas[1].ToUpper()))
                    throw new Exception();
                newColumn.Type = tmpStringMas[1].ToUpper();
                if (newColumn.Type == "SERIAL" && tmpStringMas[2].ToLower() != "primary")
                    throw new Exception();



                newColumn.IsPKey = false;
                if (tmpStringMas.Count >= 3)
                {
                    if (tmpStringMas[2].ToLower() == "primary")
                    {
                        if (tmpStringMas[3].ToLower() != "key" || tableInfo.HasPrimaryKey)
                            throw new Exception();
                        tableInfo.HasPrimaryKey = true;
                        newColumn.IsPKey = true;
                    }
                }

                int crt_pos = -1;
                if (newColumn.IsPKey)
                    crt_pos = 4;
                else
                    crt_pos = 2;

                newColumn.IsNullable = true;
                if (newColumn.IsPKey)
                    newColumn.IsNullable = false;
                if (tmpStringMas.Count > crt_pos)
                {
                    if (tmpStringMas[crt_pos].ToLower() == "not")
                    {
                        if (tmpStringMas[crt_pos + 1].ToLower() != "null")
                            throw new Exception();
                        newColumn.IsNullable = false;
                        crt_pos = crt_pos + 2;
                    }
                }


                if (tmpStringMas.Count > crt_pos + 1)
                {
                    if (tmpStringMas[crt_pos].ToLower() == "default")
                    {
                        if (newColumn.IsPKey)
                            throw new Exception();
                        newColumn.DefaultValue.IsSpecified = true;
                        newColumn.DefaultValue.IsNull = true ? tmpStringMas[crt_pos + 1].ToLower() == "null" : false;
                        newColumn.DefaultValue.Value = tmpStringMas[crt_pos + 1];
                        if (newColumn.DefaultValue.Value == "NULL")
                            newColumn.DefaultValue.Value = "";

                        if (!newColumn.IsNullable && newColumn.DefaultValue.IsNull)
                            throw new Exception();

                        if (!TypesInfo.CompareTypes(newColumn.Type.ToLower(), newColumn.DefaultValue.Value, newColumn.DefaultValue.IsNull))
                            throw new Exception();

                        if (newColumn.Type.ToLower() == "boolean")
                            newColumn.DefaultValue.Value = newColumn.DefaultValue.Value?.ToLower();

                        if (newColumn.Type.ToLower() == "string")
                        {
                            string tmp = newColumn.DefaultValue.Value;
                            TypesInfo.NameNotTableObr(ref tmp);
                            newColumn.DefaultValue.Value = tmp;
                        }

                        crt_pos += 2;
                    }
                }
                tableInfo.addColumn(newColumn);
            }
        }
    }
}
