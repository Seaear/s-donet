using System;

namespace DoNet.Utility.Database.EntitySql.Attribute
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class Field : System.Attribute
    {
        public Field(string fieldName)
        {
            FieldName = fieldName;
        }

        public string FieldName { get; set; }
    }
}