using System;
using System.Collections.Generic;

namespace SystemExtensions.Primitives
{
    public static class DateTimeExtensions
    {
        #region DateTime creation

        /// <summary>
        /// Usage: DateTime myBirthday = 1975.August(11);
        /// </summary>
        /// <param name="year"></param>
        /// <param name="day"></param>
        /// <returns></returns>
        public static DateTime January(this int year, int day)
        {
            return new DateTime(year, 1, day);
        }

        public static DateTime February(this int year, int day)
        {
            return new DateTime(year, 2, day);
        }

        public static DateTime March(this int year, int day)
        {
            return new DateTime(year, 3, day);
        }

        public static DateTime April(this int year, int day)
        {
            return new DateTime(year, 4, day);
        }

        public static DateTime May(this int year, int day)
        {
            return new DateTime(year, 5, day);
        }

        public static DateTime June(this int year, int day)
        {
            return new DateTime(year, 6, day);
        }

        public static DateTime July(this int year, int day)
        {
            return new DateTime(year, 7, day);
        }

        public static DateTime August(this int year, int day)
        {
            return new DateTime(year, 8, day);
        }

        public static DateTime September(this int year, int day)
        {
            return new DateTime(year, 9, day);
        }

        public static DateTime October(this int year, int day)
        {
            return new DateTime(year, 10, day);
        }

        public static DateTime November(this int year, int day)
        {
            return new DateTime(year, 11, day);
        }

        public static DateTime December(this int year, int day)
        {
            return new DateTime(year, 12, day);
        }

        #endregion DateTime creation

        public static bool IsWeekday(this DateTime value)
        {
            if (value.DayOfWeek == DayOfWeek.Saturday || value.DayOfWeek == DayOfWeek.Sunday)
                return false;
            else
                return true;
        }

        public static DateTime NextDay(this DateTime value)
        {
            return value.AddDays(1);
        }

        public static DateTime NextWeekDay(this DateTime value)
        {
            DateTime date = value.NextDay();
            if (date.IsWeekday() == false)
                return date.NextWeekDay();
            else
                return date;
        }

        public static DateTime PrevDay(this DateTime value)
        {
            return value.AddDays(-1);
        }

        public static DateTime PrevWeekDay(this DateTime value)
        {
            DateTime date = value.PrevDay();
            if (date.IsWeekday() == false)
                return date.PrevWeekDay();
            else
                return date;
        }

        public static DateTime LastDayOfMonth(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, 1).AddMonths(1).AddDays(-1);
        }

        public static DateTime FirstDayOfMonth(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, 1);
        }

        public static IEnumerable<DateTime> DatesTo(this DateTime value, DateTime EndDate)
        {
            // impossible case
            if (EndDate < value)
                yield return value;

            var currentDate = value;
            while (currentDate <= EndDate)
            {
                yield return currentDate;
                currentDate = currentDate.NextDay();
            }
        }

        public static IEnumerable<DateTime> WeekDatesTo(this DateTime value, DateTime EndDate)
        {
            // impossible case
            if (EndDate < value)
                yield return value;

            var currentDate = value.IsWeekday() ? value : value.NextWeekDay();
            while (currentDate <= EndDate)
            {
                yield return currentDate;
                currentDate = currentDate.NextWeekDay();
            }
        }

        public static IEnumerable<DateTime> DatesRange(this DateTime value, int numDays)
        {
            var currentDate = value;
            if (numDays > 0)
            {
                for (int i = 0; i <= numDays - 1; i++)
                {
                    yield return currentDate;
                    currentDate = currentDate.NextDay();
                }
            }
            else if (numDays < 0)
            {
                for (int i = 0; i >= numDays + 1; i--)
                {
                    yield return currentDate;
                    currentDate = currentDate.NextDay();
                }
            }
            else
                yield return currentDate;
        }

        public static IEnumerable<DateTime> WeekDatesRange(this DateTime value, int numDays)
        {
            if (numDays > 0)
            {
                var currentDate = value.IsWeekday() ? value : value.NextWeekDay();
                for (int i = 0; i <= numDays - 1; i++)
                {
                    yield return currentDate;
                    currentDate = currentDate.NextWeekDay();
                }
            }
            else if (numDays < 0)
            {
                var currentDate = value.IsWeekday() ? value : value.PrevWeekDay();
                for (int i = 0; i >= numDays + 1; i--)
                {
                    yield return currentDate;
                    currentDate = currentDate.PrevWeekDay();
                }
            }
            else
                yield return value;
        }

        private static readonly DateTime monthID_BaseDate = new DateTime(1901, 01, 01);

        public static DateTime FirstDayOfMonth(this Int32 value)
        {
            //int month = 1 + (value - 1) % 12;
            //int year = 1901 + (value - 1) / 12;

            DateTime date = new DateTime(1901, 1, 1);
            return monthID_BaseDate.AddMonths(value);
            //return new DateTime(year, month, 1);
        }

        public static bool DatesWinOverlap(DateTime d1Start, DateTime d1End, DateTime d2Start, DateTime d2End)
        {
            if (d1Start > d1End)
            {
                DateTime tmp = d1End;
                d1End = d1Start;
                d1Start = tmp;
            }

            if (d2Start > d2End)
            {
                DateTime tmp = d2End;
                d2End = d2Start;
                d2Start = tmp;
            }

            if (d1Start > d2End)
                return false;

            if (d1End < d2Start)
                return false;

            return true;
        }

        public static DateTime AddWeekDays(this DateTime value, int numDays)
        {
            /*
             Coded to return the equivalent of the last row from the SQL (for positive numdays)
                select top numDays Date
                from Static..AllDates
                where WeekDate = 1
                and Date>= value
             */

            int sign = numDays >= 0 ? 1 : -1;
            int countDays = sign * numDays; /*positive count of days*/

            if (value.IsWeekday() == false)
                value = (numDays > 0) ? value.NextWeekDay() : value.PrevWeekDay();

            var numWholeWeeks = Math.Floor(countDays / 5.0);
            var result = value.AddDays(7 * sign * numWholeWeeks);
            var remainingDays = countDays % 5;

            while (remainingDays > 0)
            {
                result = (numDays > 0) ? result.NextWeekDay() : result.PrevWeekDay();
                remainingDays--;
            }
            return result;
        }

        public static DateTime Max(this DateTime date1, DateTime date2)
        {
            return date1 > date2 ? date1 : date2;
        }

        public static DateTime Min(this DateTime date1, DateTime date2)
        {
            return date1 < date2 ? date1 : date2;
        }

        /// <summary>
        /// Corresponds with table in Statoc..Months
        /// </summary>
        /// <param name="value"></param>
        /// <returns>Number of months since January 1901</returns>
        public static Int32 MonthID(this DateTime value)
        {
            return value.Month + 12 * (value.Year - 1901);
        }

        //public static int Quarter(this DateTime value)
        //{
        //    return (int)(Math.Ceiling(value.Month / 3.0));
        //}

        public static string Quarter(this DateTime value)
        {
            return string.Format("{0}.{1}", value.Year, Math.Ceiling(value.Month / 3.0));
        }
    }
}