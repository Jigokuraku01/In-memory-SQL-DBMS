using MknImmiSql.Inner_Table_Logic.V1;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MknImmiSql.Inner_Logic.V1
{
    public class TableInfo
    {
        public int SerialCoutner = 1;
        [JsonIgnore] public Boolean HasPrimaryKey { get; set; }
        [JsonIgnore] public string name { get; set; }
        public List<ColumnInfo> TableColumnInfo { get; set; }

        public List<RowInfo> TableRowsInfo { get; set; }
        public void addColumn(ColumnInfo newColumn)
        {
            TableColumnInfo.Add(newColumn);
        }
        public TableInfo()
        {
            TableColumnInfo = new List<ColumnInfo>();
            TableRowsInfo = new List<RowInfo>();
        }

        public TableInfo(TableInfo newTableInfo)
        {
            SerialCoutner = new int();
            SerialCoutner = newTableInfo.SerialCoutner;
            HasPrimaryKey = new bool();
            HasPrimaryKey = newTableInfo.HasPrimaryKey;
            name = new string(newTableInfo.name);
            TableColumnInfo = new List<ColumnInfo>(newTableInfo.TableColumnInfo);
            TableRowsInfo = new List<RowInfo>(newTableInfo.TableRowsInfo);
        }
        public void addRow(List<int> IndexOf, string[] words)
        {
            RowInfo tmp = new RowInfo(this);
            if (tmp.isValid(IndexOf, words))
            {
                SerialCoutner = tmp._tableInfo.SerialCoutner;
                TableRowsInfo.Add(tmp);
            }
            else
                throw new Exception("400");
        }

    }
}

