using MknImmiSql.Inner_Logic.V1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MknImmiSql.Inner_Table_Logic.V1
{
    public class RowInfo
    {
        public List<object> CurrentRowObjects { get; set; }
        public TableInfo _tableInfo;
        public RowInfo(TableInfo CurTable)
        {
            _tableInfo = CurTable;
            CurrentRowObjects = new List<object>();
        }
        public bool isValid(List<int> IndexWhereToAdd, string[] words)
        {
            if (words.Length != 0)
            {
                if (words[0].Length >= 1 && words[0][0] == '(')
                    words[0] = words[0].Substring(1);
                string tmp = words[words.Length - 1];

                if (tmp.Length >= 1 && tmp[tmp.Length - 1] == ')')
                    words[words.Length - 1] = tmp.Substring(0, tmp.Length - 1);
            }
            List<string> possibleValues = new List<string>();
            foreach (string word in words)
            {
                var tmpMas = Regex.Split(word, @",(?=(?:[^""'\\]|\\.|""(?:[^""\\]|\\.)*""|'(?:[^'\\]|\\.)*')*$)").Select(part => part.Trim()).ToArray();
                foreach (var x in tmpMas)
                    possibleValues.Add(x);
            }

            CurrentRowObjects = new List<object>(_tableInfo.TableColumnInfo.Count);
            for (int i = 0; i < CurrentRowObjects.Capacity; ++i)
                CurrentRowObjects.Add(new object());
            for (int i = 0; i < CurrentRowObjects.Count; ++i)
                if (!IndexWhereToAdd.Contains(i) && _tableInfo.TableColumnInfo[i].Type.ToLower() != "serial")
                {
                    if (_tableInfo.TableColumnInfo[i].DefaultValue.IsSpecified)
                    {
                        if (!_tableInfo.TableColumnInfo[i].DefaultValue.IsNull)
                            CurrentRowObjects[i] = _tableInfo.TableColumnInfo[i].DefaultValue.Value;
                        else
                            CurrentRowObjects[i] = null;
                    }
                    else
                        throw new System.Exception("400");
                }

            int curPosValuePos = 0;

            for (int i = 0; i < CurrentRowObjects.Count; ++i)
                if (IndexWhereToAdd.Contains(i))
                {
                    if (possibleValues[curPosValuePos].ToLower() == "null")
                        possibleValues[curPosValuePos] = null;
                    if (TypesInfo.CompareTypes(_tableInfo.TableColumnInfo[i].Type.ToLower(), possibleValues[curPosValuePos]))
                    {
                        string tmpStr = possibleValues[curPosValuePos];
                        if (tmpStr == null && !_tableInfo.TableColumnInfo[i].IsNullable)
                            throw new System.Exception("400");

                        if (tmpStr == null)
                            CurrentRowObjects[i] = null;
                        else
                        {
                            if (_tableInfo.TableColumnInfo[i].Type.ToLower() == "string")
                                TypesInfo.NameNotTableObr(ref tmpStr);
                            CurrentRowObjects[i] = tmpStr;
                        }
                        curPosValuePos++;
                    }
                    else
                        throw new System.Exception("400");
                }
            for (int i = 0; i < _tableInfo.TableColumnInfo.Count; ++i)
            {
                if (_tableInfo.TableColumnInfo[i].Type.ToLower() == "serial" && !IndexWhereToAdd.Contains(i))
                    CurrentRowObjects[i] = _tableInfo.SerialCoutner++;
            }
            foreach (int index in IndexWhereToAdd)
            {
                if (_tableInfo.TableColumnInfo[index].IsPKey)
                {

                    foreach (RowInfo tmpRow in _tableInfo.TableRowsInfo)
                    {
                        if (String.CompareOrdinal(tmpRow.CurrentRowObjects[index].ToString(), CurrentRowObjects[index].ToString()) == 0)
                            throw new System.Exception("409");
                    }
                }
            }
            return true;
        }
    }
}
