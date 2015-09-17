using System;
using System.Linq.Expressions;

namespace DoNet.Utility.Database.EntitySql.Entity
{
    /// <summary>
    ///     支持泛型的表连接的条件
    /// </summary>
    public class GenericJoinEntity<TA, TB>
    {
        public string LeftTableGuid { get; private set; }

        public string RightTableGuid { get; private set; }

        public GenericWhereEntity<TA> MainEntity { get; set; }
        public GenericWhereEntity<TB> EntityToJoin { get; set; }

        /// <summary>
        ///     连接模式
        /// </summary>
        public JoinModeEnum JoinMode { get; set; }

        /// <summary>
        ///     连接条件
        /// </summary>
        public string JoinCondition { get; set; }

        /// <summary>
        ///     连接条件的首参数，用于表达式的定位
        /// </summary>
        public string JoinConditionFirstParameter { get; private set; }

        /// <summary>
        ///     连接条件表达式
        /// </summary>
        public Expression JoinConditionExpression { get; private set; }

        public void InnerJoin(GenericWhereEntity<TA> TA, GenericWhereEntity<TB> TB,
            Expression<Func<TA, TB, bool>> conditionExpression)
        {
            Join(TA, TB, conditionExpression, JoinModeEnum.InnerJoin);
        }

        public void LeftJoin(GenericWhereEntity<TA> TA, GenericWhereEntity<TB> TB,
            Expression<Func<TA, TB, bool>> conditionExpression)
        {
            Join(TA, TB, conditionExpression, JoinModeEnum.LeftJoin);
        }

        private void Join(GenericWhereEntity<TA> TA, GenericWhereEntity<TB> TB,
            Expression<Func<TA, TB, bool>> conditionExpression, JoinModeEnum joinMode)
        {
            if (conditionExpression.Body == null)
                throw new EntitySqlException("未指定连接条件！");
            if (!(conditionExpression.Body is BinaryExpression) || !CheckJoinCondition(conditionExpression.Body))
                throw new EntitySqlException("指定的连接条件无效！");

            JoinMode = joinMode;
            MainEntity = TA;
            EntityToJoin = TB;
            LeftTableGuid = TA.Guid;
            RightTableGuid = TB.Guid;
            JoinConditionExpression = conditionExpression.Body;
            JoinConditionFirstParameter = conditionExpression.Parameters[0].Name;
        }

        /// <summary>
        ///     判断连接条件是否符合要求
        /// </summary>
        /// <param name="joinExpression">连接条件的表达式</param>
        /// <returns></returns>
        private static bool CheckJoinCondition(Expression joinExpression)
        {
            if (joinExpression is BinaryExpression)
            {
                var b = (BinaryExpression) joinExpression;
                return CheckJoinCondition(b.Left) && CheckJoinCondition(b.Right);
            }
            if (joinExpression is MemberExpression)
            {
                return true;
            }
            return false;
        }
    }
}