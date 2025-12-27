using System;

namespace MknImmiSql.Inner_Logic.V1
{
    public class DefaultValue
    {
        public Boolean IsSpecified { get; set; }
        public Boolean IsNull { get; set; }
        public String Value { get; set; }
    }
    public class ColumnInfo
    {
        public String Name { get; set; }
        public String Type { get; set; }
        public Boolean IsPKey { get; set; }
        public Boolean IsNullable { get; set; }
        public DefaultValue DefaultValue { get; set; }
        public ColumnInfo()
        {
            DefaultValue = new DefaultValue();
        }
        public ColumnInfo(ColumnInfo tmp)
        {
            Name = tmp.Name;
            Type = tmp.Type;
            IsPKey = tmp.IsPKey;
            IsNullable = tmp.IsNullable;
            DefaultValue = tmp.DefaultValue;
        }
    }
}
