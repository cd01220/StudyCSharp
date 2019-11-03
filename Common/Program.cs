using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Common
{
    public class Program
    {
        private static DateTime GetNextWeeklySettlingDateTime(DateTime dateTime)
        {
            DateTime dateTimeDiff = dateTime.AddHours(16);    // UTC ʱ��8��ǰȡ��ǰ����Ϊ�����գ�8���ȡ��һ�������յ�����Ϊ�����ա�
            DateTime settlingDateTime = dateTimeDiff.AddDays(((int)DayOfWeek.Friday - (int)dateTimeDiff.DayOfWeek + 7) % 7);
            return settlingDateTime.AddHours(8 - settlingDateTime.Hour) // UTCʱ������8��
                    .AddMinutes(0 - settlingDateTime.Minute)            // 00��
                    .AddSeconds(0 - settlingDateTime.Second);           // 00��
        }

        // ���Ⱥ�Լ����ʱ��Ϊ��3, 6, 9, 12 �µ����һ������, ����ʱ�� 16:00�� UTCʱ�� 8:00
        private static DateTime GetNextQuarterlySettlingDateTime(DateTime dateTime)
        {
            DateTime date = dateTime.AddHours(8 - dateTime.Hour)       // UTCʱ�� 8��
                                    .AddMinutes(0 - dateTime.Minute)   // 00��
                                    .AddSeconds(0 - dateTime.Second)   // 00��
                                    .AddMilliseconds(0 - dateTime.Millisecond);  //00����

            DateTime lastDayOfMonth = date.AddMonths((3 - date.Month % 3) % 3)                // 3, 6, 9, 12 ��
                                          .AddDays(1 - date.Day).AddMonths(1).AddDays(0 - 1); // ��ǰ�·ֵ����һ��

            // �������һ��֮ǰ�����һ��������
            DateTime settlingDateTime = lastDayOfMonth.AddDays(0 - ((7 + (int)lastDayOfMonth.DayOfWeek - 5) % 7));
            if (settlingDateTime < dateTime)
            {
                // settlingDateTime �ĺ�Լ�Ѿ����ȡ��һ�������Լ�����ڡ�
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
