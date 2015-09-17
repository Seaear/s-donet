using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace DoNet.Utility.Database.EntitySql.Entity
{
    /// <summary>
    ///     支持泛型的查询条件
    /// </summary>
    public class GenericWhereEntity<T>
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        public GenericWhereEntity()
        {
            Guid = new Guid().ToString();
            DbProviderName = "System.Data.SqlClient";
            EntityType = typeof (T);
            WhereExpressions = new List<Expression>();
            WhereParameterNames = new List<string>(8);
            WhereParameterValues = new List<object>(8);
            WhereParameterTypes = new List<DbType>(8);

            TableName = "T" + TableNameIndex.ToString().PadLeft(2, '0');
            TableNameIndex = 0;
        }

        public int TableNameIndex { get; private set; }

        public string TableName { get; private set; } = string.Empty;

        /// <summary>
        ///     数据库提供器
        /// </summary>
        public string DbProviderName { get; set; }

        /// <summary>
        ///     Guid标识符
        /// </summary>
        public string Guid { get; }

        /// <summary>
        ///     条件表达式
        /// </summary>
        public List<Expression> WhereExpressions { get; }

        /// <summary>
        ///     实体的类型
        /// </summary>
        public Type EntityType { get; }

        /// <summary>
        ///     查询条件
        /// </summary>
        public string WhereCondition { get; set; }

        /// <summary>
        ///     是否禁用表的别名
        /// </summary>
        public bool DisableTableAlias { get; set; }

        /// <summary>
        ///     查询条件参数列表
        /// </summary>
        public List<string> WhereParameterNames { get; }

        /// <summary>
        ///     查询条件参数值
        /// </summary>
        public List<object> WhereParameterValues { get; }

        /// <summary>
        ///     查询条件类型
        /// </summary>
        public List<DbType> WhereParameterTypes { get; }

        /// <summary>
        ///     设置查询条件
        /// </summary>
        /// <param name="predicate"></param>
        public void Where(Expression<Func<T, bool>> conditionExpression)
        {
            if (conditionExpression == null || conditionExpression.Body == null)
                return;
            WhereExpressions.Add(conditionExpression.Body);
        }

        /// <summary>
        ///     设置查询条件
        /// </summary>
        /// <param name="predicate"></param>
        public void Where(Expression conditionExpression)
        {
            if (conditionExpression == null)
                return;
            WhereExpressions.Add(conditionExpression);
        }

        /// <summary>
        ///     重置表的别名
        /// </summary>
        /// <param name="tableNameIndex"></param>
        public void ResetTableName(int tableNameIndex)
        {
            TableNameIndex = tableNameIndex;
            TableName = "T" + TableNameIndex.ToString().PadLeft(2, '0');
        }
    }
}