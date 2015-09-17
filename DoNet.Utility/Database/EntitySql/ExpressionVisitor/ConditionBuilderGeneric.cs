﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using DoNet.Utility.Database.EntitySql.Entity;

namespace DoNet.Utility.Database.EntitySql.ExpressionVisitor
{
    internal class ConditionBuilderGeneric<T> : ExpressionVisitor
    {
        /// <summary>
        ///     表的别名
        /// </summary>
        private readonly string _TableAlias;

        /// <summary>
        ///     相关的实体
        /// </summary>
        private readonly GenericWhereEntity<T> _TheWhereEntity;

        private List<object> m_arguments;
        private Stack<string> m_conditionParts;
        private List<DbType> m_DbTypes;
        private List<string> m_ParameterNames;

        /// <summary>
        ///     当前访问的数据表字段成员
        /// </summary>
        private string m_TmpDBColumnName;

        /// <summary>
        ///     当前访问的数据表字段类型
        /// </summary>
        private DbType m_TmpDBColumnType = DbType.AnsiString;

        /// <summary>
        ///     当前访问的数据是否使用参数
        /// </summary>
        private bool m_TmpUsedParameter;

        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="tableAlias">表的别名</param>
        /// <param name="theEntity">实体</param>
        public ConditionBuilderGeneric(string tableAlias, GenericWhereEntity<T> theEntity)
        {
            _TableAlias = tableAlias;
            _TheWhereEntity = theEntity;
        }

        /// <summary>
        ///     最终的查询条件
        /// </summary>
        public string Condition { get; private set; }

        /// <summary>
        ///     参数值的列表
        /// </summary>
        public object[] Arguments { get; private set; }

        /// <summary>
        ///     数据类型的列表
        /// </summary>
        public DbType[] DbTypes { get; private set; }

        /// <summary>
        ///     参数名称的列表
        /// </summary>
        public string[] ParameterNames { get; private set; }

        /// <summary>
        ///     建立条件语句
        /// </summary>
        /// <param name="expression"></param>
        public void Build(Expression expression)
        {
            var evaluator = new PartialEvaluator();
            var evaluatedExpression = evaluator.Eval(expression);

            m_arguments = new List<object>();
            m_conditionParts = new Stack<string>();
            m_DbTypes = new List<DbType>();
            m_ParameterNames = new List<string>();

            Visit(evaluatedExpression);

            Arguments = m_arguments.ToArray();
            DbTypes = m_DbTypes.ToArray();
            ParameterNames = m_ParameterNames.ToArray();

            Condition = m_conditionParts.Count > 0 ? m_conditionParts.Pop() : null;
        }

        /// <summary>
        ///     访问操作符
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        protected override Expression VisitBinary(BinaryExpression b)
        {
            if (b == null) return b;

            var isLeftConstant = (b.Left is ConstantExpression);
            var isRightConstant = (b.Right is ConstantExpression);

            //不支持两端都是静态变量的表达式
            if (b.Left is ConstantExpression && b.Right is ConstantExpression)
                throw new NotSupportedException("不支持两端都是静态变量的表达式！" + b);

            //右侧的静态变量访问，如果是null，操作符要进行特殊处理
            var isRightConstantNull = false;
            if (isRightConstant)
            {
                var ce = (ConstantExpression) b.Right;
                if (ce.Value == null)
                    isRightConstantNull = true;
            }

            string opr;
            switch (b.NodeType)
            {
                case ExpressionType.Equal:
                    if (isRightConstantNull)
                        opr = "is";
                    else
                        opr = "=";
                    break;
                case ExpressionType.NotEqual:
                    if (isRightConstantNull)
                        opr = "is not";
                    else
                        opr = "<>";
                    break;
                case ExpressionType.GreaterThan:
                    if (isRightConstantNull)
                        throw new NotSupportedException("对null值不支持大于的操作");
                    opr = ">";
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    if (isRightConstantNull)
                        throw new NotSupportedException("对null值不支持大于等于的操作");
                    opr = ">=";
                    break;
                case ExpressionType.LessThan:
                    if (isRightConstantNull)
                        throw new NotSupportedException("对null值不支持小于的操作");
                    opr = "<";
                    break;
                case ExpressionType.LessThanOrEqual:
                    if (isRightConstantNull)
                        throw new NotSupportedException("对null值不支持小于等于的操作");
                    opr = "<=";
                    break;
                case ExpressionType.AndAlso:
                    if (isRightConstantNull)
                        throw new NotSupportedException("对null值不支持与的操作");
                    opr = "AND";
                    break;
                case ExpressionType.OrElse:
                    if (isRightConstantNull)
                        throw new NotSupportedException("对null值不支持或的操作");
                    opr = "OR";
                    break;
                case ExpressionType.Add:
                    opr = "+";
                    break;
                case ExpressionType.Subtract:
                    opr = "-";
                    break;
                case ExpressionType.Multiply:
                    opr = "*";
                    break;
                case ExpressionType.Divide:
                    opr = "/";
                    break;
                default:
                    throw new NotSupportedException("不支持操作类型：" + b.NodeType);
            }

            //如果涉及到访问实体成员，则先检索成员相关信息，再访问子节点。访问完成后，恢复临时成员的信息。
            var bkDBColumnName = m_TmpDBColumnName;
            var bkDBColumnType = m_TmpDBColumnType;
            var bkUsedParameter = m_TmpUsedParameter;
            MemberExpression leftMemberExp = null, rightMemberExp = null;
            bool isLeftMember = false, isRightMember = false;
            if (b.Left is MemberExpression)
                leftMemberExp = b.Left as MemberExpression;
            else if (b.Left.NodeType == ExpressionType.Convert)
                leftMemberExp = ((UnaryExpression) b.Left).Operand as MemberExpression;
            isLeftMember = (leftMemberExp != null);

            //如果左侧是成员访问，右侧是空值，直接输出语句
            if (b.Right is MemberExpression)
                rightMemberExp = b.Right as MemberExpression;
            else if (b.Right.NodeType == ExpressionType.Convert)
                rightMemberExp = ((UnaryExpression) b.Right).Operand as MemberExpression;
            isRightMember = (rightMemberExp != null);

            m_TmpUsedParameter = ((isLeftMember && isRightConstant) || (isRightMember && isLeftConstant));
            //左侧是访问成员，肯定要加载
            if (isLeftMember)
            {
                m_TmpDBColumnName = EntityMappingTool.GetDbColumnName(_TheWhereEntity.EntityType,
                    leftMemberExp.Member.Name);
                m_TmpDBColumnType = EntityMappingTool.GetDbColumnType(_TheWhereEntity.EntityType,
                    leftMemberExp.Member.Name);
                //如果是左侧为成员，右侧为null值，需要特殊处理
                if (isRightConstantNull)
                {
                    //1.访问Left
                    Visit(b.Left);
                    //2.左条件出栈
                    var tmpLeftCondition = m_conditionParts.Pop();
                    //3.生成语句并入栈
                    var tmpCondition = string.Format("({0} {1} {2})", tmpLeftCondition, opr, "null");
                    m_conditionParts.Push(tmpCondition);
                    //4.恢复成员
                    m_TmpUsedParameter = bkUsedParameter;
                    m_TmpDBColumnType = bkDBColumnType;
                    m_TmpDBColumnName = bkDBColumnName;
                    //5.退出本次执行
                    return b;
                }
            }
            else if (isLeftConstant && isRightMember)
            {
                m_TmpDBColumnName = EntityMappingTool.GetDbColumnName(_TheWhereEntity.EntityType,
                    rightMemberExp.Member.Name);
                m_TmpDBColumnType = EntityMappingTool.GetDbColumnType(_TheWhereEntity.EntityType,
                    rightMemberExp.Member.Name);
            }
            Visit(b.Left);

            //两侧都是访问成员的话，也加载
            if (isLeftMember && isRightMember)
            {
                m_TmpDBColumnName = EntityMappingTool.GetDbColumnName(_TheWhereEntity.EntityType,
                    rightMemberExp.Member.Name);
                m_TmpDBColumnType = EntityMappingTool.GetDbColumnType(_TheWhereEntity.EntityType,
                    rightMemberExp.Member.Name);
            }
            Visit(b.Right);

            //恢复成员
            m_TmpUsedParameter = bkUsedParameter;
            m_TmpDBColumnType = bkDBColumnType;
            m_TmpDBColumnName = bkDBColumnName;

            var right = m_conditionParts.Pop();
            var left = m_conditionParts.Pop();

            //默认的输出格式
            var condition = string.Format("({0} {1} {2})", left, opr, right);

            /*
            //输出方式的改变，内部如果存在and将不再重复输出括号，最后在最外层输出输出一个括号
            string condition = null;
            switch (b.NodeType)
            {
                case ExpressionType.AndAlso:
                    if (NeedQuoteForChildren(b))
                        condition = String.Format("({0} {1} {2})", left, opr, right);
                    else
                        condition = String.Format("{0} {1} {2}", left, opr, right);
                    break;
                case ExpressionType.OrElse:
                    if (NeedQuoteForChildren(b))
                        condition = String.Format("({0} {1} {2})", left, opr, right);
                    else
                        condition = String.Format("{0} {1} {2}", left, opr, right);
                    break;
                default:
                    condition = String.Format("{0} {1} {2}", left, opr, right);
                    break;
            }
            //新的组合方式
            */

            m_conditionParts.Push(condition);

            return b;
        }

        /// <summary>
        ///     调用方法
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (m.Object != null && (m.Object is MemberExpression || m.Object is ConstantExpression))
            {
                MethodCallVisitor.Visit(_TheWhereEntity.EntityType, m, _TableAlias, m_conditionParts,
                    m_ParameterNames, m_DbTypes, m_arguments);
            }
            else
            {
                return Expression.Call(m.Object, m.Method, m.Arguments);
                //TODO
                /*Expression obj = this.Visit(m.Object);
                IEnumerable<Expression> args = this.VisitExpressionList(m.Arguments);
                if (obj != m.Object || args != m.Arguments)
                {
                    return Expression.Call(obj, m.Method, args);
                }*/
            }
            return m;
        }

        /// <summary>
        ///     访问静态值
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        protected override Expression VisitConstant(ConstantExpression c)
        {
            /*
             * 因为不支持两端都是常量的语句，所以这里可使用m_TmpMember
             */

            if (c == null) return c;

            m_arguments.Add(c.Value);
            var parameterName = string.Format("@{0}_{1}", _TableAlias, m_TmpDBColumnName);
            //如果一个字段多次使用，则需要加后缀
            var tmpIndex = 0;
            var parameterNameExtern = "";
            while (m_ParameterNames.Contains(parameterName + parameterNameExtern))
            {
                tmpIndex++;
                parameterNameExtern = tmpIndex.ToString();
            }
            parameterName = parameterName + parameterNameExtern;
            m_ParameterNames.Add(parameterName);
            m_DbTypes.Add(m_TmpDBColumnType);

            m_conditionParts.Push(parameterName);

            return c;
        }

        /// <summary>
        ///     访问类的成员
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        protected override Expression VisitMemberAccess(MemberExpression m)
        {
            if (m == null) return m;

            m_conditionParts.Push(string.Format("{0}.[{1}]", _TableAlias, m_TmpDBColumnName));

            return m;
        }

        /// <summary>
        ///     对可空类型的转换
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        protected override Expression VisitConvert(UnaryExpression v)
        {
            if (v == null) return v;

            var m = (MemberExpression) v.Operand;
            m_conditionParts.Push(string.Format("{0}.[{1}]", _TableAlias, m_TmpDBColumnName));

            return v;
        }

        /*
        /// <summary>
        /// 判断是否需要使用括号将整个节点括起来
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        private static bool NeedQuoteForChildren(BinaryExpression b)
        {
            BinaryExpression bl = b.Left as BinaryExpression;
            BinaryExpression br = b.Right as BinaryExpression;
            if (bl == null || br == null)
                return false;
            if (bl.NodeType != b.NodeType || br.NodeType != b.NodeType)
                return true;
            return false;
        }
        */
    }
}