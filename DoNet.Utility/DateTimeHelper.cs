using System;
using System.Globalization;

namespace DoNet.Utility
{
    public class DateTimeHelper
    {
        /// <summary>
        ///     把秒转换成分钟
        /// </summary>
        /// <returns></returns>
        public static int SecondToMinute(int second)
        {
            var mm = second/(decimal) 60;
            return Convert.ToInt32(Math.Ceiling(mm));
        }

        /// <summary>
        ///     返回某年某月最后一天
        /// </summary>
        /// <param name="year">年份</param>
        /// <param name="month">月份</param>
        /// <returns>日</returns>
        public static int GetMonthLastDate(int year, int month)
        {
            var lastDay = new DateTime(year, month, new GregorianCalendar().GetDaysInMonth(year, month));
            var Day = lastDay.Day;
            return Day;
        }

        /// <summary>
        ///     返回时间差
        /// </summary>
        public static string DateDiff(DateTime dateTime1, DateTime dateTime2)
        {
            string dateDiff = null;
            try
            {
                //TimeSpan ts1 = new TimeSpan(DateTime1.Ticks);
                //TimeSpan ts2 = new TimeSpan(DateTime2.Ticks);
                //TimeSpan ts = ts1.Subtract(ts2).Duration();
                var ts = dateTime2 - dateTime1;
                if (ts.Days >= 1)
                {
                    dateDiff = dateTime1.Month + "月" + dateTime1.Day + "日";
                }
                else
                {
                    if (ts.Hours > 1)
                    {
                        dateDiff = ts.Hours + "小时前";
                    }
                    else
                    {
                        dateDiff = ts.Minutes + "分钟前";
                    }
                }
            }
            catch
            {
                // ignored
            }
            return dateDiff;
        }
    }
}