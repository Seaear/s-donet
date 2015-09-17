using System;

namespace DoNet.Utility.Database.EntitySql.Attribute
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class Table : System.Attribute
    {
        public Table(string tableName)
        {
            TableName = tableName;
        }

        public string TableName { get; set; }
    }
}