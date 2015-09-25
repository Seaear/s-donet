using System;
using System.Collections.Generic;

namespace DoNet.Utility
{
    public class ValidationHelper<T>
    {
        /// <summary>
        ///     创建一个ValidationHelper的对象.
        /// </summary>
        /// <param name="value">待验证的参数的值.</param>
        /// <param name="name">待验证的参数的名称.</param>
        /// <param name="errorList">验证错误列表.</param>
        public ValidationHelper(T value, string name, List<string> errorList)
        {
            Value = value;
            Name = name;
            ErrorList = errorList ?? new List<string>();
        }

        /// <summary>
        ///     获取待验证的参数的值.
        /// </summary>
        public T Value { get; }

        /// <summary>
        ///     获取待验证的参数的名称.
        /// </summary>
        public string Name { get; }

        public List<string> ErrorList { get; set; }

        /// <summary>
        ///     验证参数不为其默认值.
        /// </summary>
        /// <returns>this指针以方便链式调用.</returns>
        /// <exception cref="ArgumentException">参数为值类型且为默认值.</exception>
        /// <exception cref="ArgumentNullException">参数为引用类型且为null.</exception>
        public bool NotDefault()
        {
            if (Value.Equals(default(T)))
            {
                //if (Value is ValueType)
                //{
                //    ErrorList.Add(string.Format("{0}不能为默认值！", Name));
                //}
                //ErrorList.Add(string.Format("{0}不能为null值！", Name));
                return false;
            }
            //return this;
            return true;
        }

        /// <summary>
        ///     使用自定义方法进行验证.
        /// </summary>
        /// <param name="rule">用以验证的自定义方法.</param>
        /// <returns>this指针以方便链式调用.</returns>
        /// <exception cref="Exception">验证失败抛出相应异常.</exception>
        /// <remarks>rule的第一个参数为参数值,第二个参数为参数名称.</remarks>
        public ValidationHelper<T> CustomRule(Action<T, string> rule)
        {
            rule(Value, Name);
            return this;
        }
    }
}