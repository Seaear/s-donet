using System;
using System.Linq.Expressions;
using DoNet.Utility.Database.EntitySql.Entity;

namespace DoNet.Utility.Database.EntitySql.ExpressionVisitor
{
    internal static class JoinConditionBuilderGeneric
    {
        /// <summary>
        ///     获取连接查询条件
        /// </summary>
        /// <param name="theJoinEntity">连接定义实体</param>
        /// <returns>连接查询条件</returns>
        public static string GetJoinCondition<TA, TB>(GenericJoinEntity<TA, TB> theJoinEntity)
        {
            var condition = GetSubConditions(theJoinEntity.MainEntity, theJoinEntity.EntityToJoin,
                theJoinEntity.JoinConditionExpression, theJoinEntity.JoinConditionFirstParameter);
            return condition;
        }

        private static string GetSubConditions<TA, TB>(GenericWhereEntity<TA> mainEntity,
            GenericWhereEntity<TB> joinEntity, Expression joinExpression, string firstParameter)
        {
            if (joinExpression is MemberExpression)
            {
                var me = (MemberExpression) joinExpression;
                if (me.Expression.ToString() == firstParameter)
                {
                    var dbColumnName = EntityMappingTool.GetDbColumnName(mainEntity.EntityType, me.Member.Name);
                    return string.Format("{0}.[{1}]", mainEntity.TableName, dbColumnName);
                }
                else
                {
                    var dbColumnName = EntityMappingTool.GetDbColumnName(joinEntity.EntityType, me.Member.Name);
                    return string.Format("{0}.[{1}]", joinEntity.TableName, dbColumnName);
                }
            }

            string opr;
            var be = (BinaryExpression) joinExpression;
            switch (be.NodeType)
            {
                case ExpressionType.Equal:
                    opr = "=";
                    break;
                case ExpressionType.NotEqual:
                    opr = "<>";
                    break;
                case ExpressionType.GreaterThan:
                    opr = ">";
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    opr = ">=";
                    break;
                case ExpressionType.LessThan:
                    opr = "<";
                    break;
                case ExpressionType.LessThanOrEqual:
                    opr = "<=";
                    break;
                default:
                    throw new NotSupportedException("不支持连接条件类型：" + joinExpression.NodeType);
            }
            var left = GetSubConditions(mainEntity, joinEntity, be.Left, firstParameter);
            var right = GetSubConditions(mainEntity, joinEntity, be.Right, firstParameter);

            var con = string.Format("({0} {1} {2})", left, opr, right);

            return con;
        }
    }
}