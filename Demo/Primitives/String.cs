using System;
using System.Linq;

namespace SystemExtensions.Primitives
{
    public static class StringExtensions
    {
        public static string FormatString(this string me, params object[] args)
        {
            return string.Format(me, args);
        }

        private enum DateTimeConverterKeyWords
        {
            NONE,
            TODAY,
            TOMORROW,
            YESTERDAY,
            NEXTWORKINGDAY,
            PREVWORKINGDAY,
            STARTOFMONTH,
            STARTOFNEXTMONTH,
            STARTYEAR,
            STARTNEXTYEAR,
            START1950,
            START1960,
            START1970,
            START1980,
            START1990,
            START2000,
            START2010,
            NOW
        }

        public static bool TryGetDateTime(this string value, out DateTime date)
        {
            value = value.Trim('\'', '\"');
            // perfectly normal date
            if (DateTime.TryParse(value, out date) == true)
                return true;

            DateTimeConverterKeyWords type = DateTimeConverterKeyWords.NONE;

            if (Enum.IsDefined(typeof(DateTimeConverterKeyWords), value) == false)
                return false;

            type = (DateTimeConverterKeyWords)Enum.Parse(typeof(DateTimeConverterKeyWords), value, true);

            date = DateTime.Today;

            switch (type)
            {
                case DateTimeConverterKeyWords.TOMORROW:
                    date = DateTime.Today.AddDays(1);
                    break;

                case DateTimeConverterKeyWords.YESTERDAY:
                    date = DateTime.Today.AddDays(-1);
                    break;

                case DateTimeConverterKeyWords.NEXTWORKINGDAY:
                    date = DateTime.Today.AddWeekDays(1);
                    break;

                case DateTimeConverterKeyWords.PREVWORKINGDAY:
                    date = DateTime.Today.AddWeekDays(-1);
                    break;

                case DateTimeConverterKeyWords.STARTOFMONTH:
                    date = DateTime.Today.MonthID().FirstDayOfMonth();
                    break;

                case DateTimeConverterKeyWords.STARTOFNEXTMONTH:
                    date = (DateTime.Today.MonthID() + 1).FirstDayOfMonth();
                    break;

                case DateTimeConverterKeyWords.STARTYEAR:
                    date = DateTime.Today.Year.January(1);
                    break;

                case DateTimeConverterKeyWords.STARTNEXTYEAR:
                    date = (DateTime.Today.Year + 1).January(1);
                    break;

                case DateTimeConverterKeyWords.START1990:
                    date = 1990.January(1);
                    break;

                case DateTimeConverterKeyWords.START1970:
                    date = 1970.January(1);
                    break;

                case DateTimeConverterKeyWords.START1980:
                    date = 1980.January(1);
                    break;

                case DateTimeConverterKeyWords.START1960:
                    date = 1960.January(1);
                    break;

                case DateTimeConverterKeyWords.START1950:
                    date = 1950.January(1);
                    break;

                case DateTimeConverterKeyWords.START2000:
                    date = 2000.January(1);
                    break;

                case DateTimeConverterKeyWords.START2010:
                    date = 2010.January(1);
                    break;

                case DateTimeConverterKeyWords.TODAY:
                    date = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day);
                    if (DateTime.Now.TimeOfDay < new TimeSpan(13, 30, 00))
                        date = date.PrevWeekDay();
                    break;

                case DateTimeConverterKeyWords.NOW:
                    date = DateTime.Now;
                    break;
            }

            return true;
        }

        public static bool IsEmpty(this string me)
        {
            if (me == default(string) || me.Length == 0)
                return true;
            return false;
        }

        public static bool TryGetDateRange(this string value, out DateTime dateFirst, out DateTime dateLast)
        {
            dateFirst = default(DateTime);
            dateLast = default(DateTime);

            /*have a look at the dates by splitting*/
            var possDates = value.Split('-');
            if (possDates.Count() != 2)
                return false;

            /*Can we parse the string into dates?*/
            return (possDates[0].TryGetDateTime(out dateFirst) &&
                 possDates[1].TryGetDateTime(out dateLast));
        }
    }
}