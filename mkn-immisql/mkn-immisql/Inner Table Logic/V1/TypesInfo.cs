using MknImmiSql.Inner_Logic.V1;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace MknImmiSql.Inner_Table_Logic.V1
{
    public static class TypesInfo
    {
        public static HashSet<string> PossibleTypes = new HashSet<string>() {
            "BOOLEAN",
            "INTEGER",
            "FLOAT",
            "STRING",
            "SERIAL"
        };
        public static bool IsPossibleType(string possibleType)
        {
            return PossibleTypes.Contains(possibleType);
        }
        public static void NameTableObr(ref string name)
        {
            if (name.Length >= 1 && name[0] == '\"' && name[name.Length - 1] == '\"')
                name = name.Substring(1, name.Length - 2);
        }

        public static void NameNotTableObr(ref string name)
        {
            if (name == null)
                return;
            if (name.Length >= 1 &&  name[0] == '\'' && name[name.Length - 1] == '\'')
                name = name.Substring(1, name.Length - 2);
        }



        public static bool CompareTypes(string TYPE, string? x, bool isNull = false)
        {
            if (x == null)
                return true;
            if (isNull)
                return true;
            switch (TYPE.ToLower())
            {
                case "integer":
                    return int.TryParse(x, out int numberi) == true;
                case "serial":
                    return int.TryParse(x, out int numbers) == true;
                case "string":
                    return ((x[0] == '\'' && x[x.Length - 1] == '\'') || x == null);
                case "float":
                    return float.TryParse(x, NumberStyles.Float, CultureInfo.InvariantCulture, out float numberf) == true;
                case "boolean":
                    return (x.ToLower() == "true" || x.ToLower() == "false");
            }
            return false;
        }

        public static TableInfo createDefaultResult(string x)
        {
            if (x.ToLower() != "true" && x.ToLower() != "false")
                throw new Exception("400");
            var tmpColumn = new ColumnInfo();
            tmpColumn.Name = "result";
            tmpColumn.Type = "boolean";
            tmpColumn.IsPKey = false;
            tmpColumn.IsNullable = false;
            tmpColumn.DefaultValue = new DefaultValue();
            tmpColumn.DefaultValue.IsNull = false;
            tmpColumn.DefaultValue.Value = "";
            tmpColumn.DefaultValue.IsSpecified = false;
            var ans = new TableInfo();
            ans.addColumn(tmpColumn);
            ans.addRow(new List<int> { 0 }, new List<string> { x }.ToArray());
            return ans;
        }
    }
}
