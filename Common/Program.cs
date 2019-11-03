using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Common
{
    public class Program
    {
        private static DateTime GetNextWeeklySettlingDateTime(DateTime dateTime)
        {
            DateTime dateTimeDiff = dateTime.AddHours(16);    // UTC 时间8点前取当前日期为交割日，8点后取下一个交割日的日期为交割日。
            DateTime settlingDateTime = dateTimeDiff.AddDays(((int)DayOfWeek.Friday - (int)dateTimeDiff.DayOfWeek + 7) % 7);
            return settlingDateTime.AddHours(8 - settlingDateTime.Hour) // UTC时间早上8点
                    .AddMinutes(0 - settlingDateTime.Minute)            // 00分
                    .AddSeconds(0 - settlingDateTime.Second);           // 00秒
        }

        // 季度合约交割时间为：3, 6, 9, 12 月的最后一个周五, 北京时间 16:00， UTC时间 8:00
        private static DateTime GetNextQuarterlySettlingDateTime(DateTime dateTime)
        {
            DateTime date = dateTime.AddHours(8 - dateTime.Hour)       // UTC时间 8点
                                    .AddMinutes(0 - dateTime.Minute)   // 00分
                                    .AddSeconds(0 - dateTime.Second)   // 00秒
                                    .AddMilliseconds(0 - dateTime.Millisecond);  //00毫秒

            DateTime lastDayOfMonth = date.AddMonths((3 - date.Month % 3) % 3)                // 3, 6, 9, 12 月
                                          .AddDays(1 - date.Day).AddMonths(1).AddDays(0 - 1); // 当前月分的最后一天

            // 本月最后一天之前最近的一个星期五
            DateTime settlingDateTime = lastDayOfMonth.AddDays(0 - ((7 + (int)lastDayOfMonth.DayOfWeek - 5) % 7));
            if (settlingDateTime < dateTime)
            {
                // settlingDateTime 的合约已经交割，取下一季交割合约的日期。
                settlingDateTime = GetNextQuarterlySettlingDateTime(dateTime.AddDays(7));
            }

            return settlingDateTime;
        }

        private static void Main()
        {
            DateTime dateTime = DateTime.Parse("2019-12-27T00:00:00.000Z").ToUniversalTime();
            DateTime result0 = GetNextQuarterlySettlingDateTime(dateTime);
            DateTime result1 = GetNextQuarterlySettlingDateTime(result0.AddDays(7));
            Console.WriteLine(string.Format("{0:yyyy-MM-dd}, {1:yyyy-MM-dd}", result0, result1));
        }
    }
}
