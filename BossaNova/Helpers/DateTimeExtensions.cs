using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tasks.Show.Helpers
{
    // From http://datetimeextensions.codeplex.com/

    public static class DayExtensions
    {
        /// <summary>
        /// Gets a DateTime representing the first day in the current month
        /// </summary>
        /// <param name="current">The current date</param>
        /// <returns></returns>
        public static DateTime First(this DateTime current)
        {
            DateTime first = current.AddDays(1 - current.Day);
            return first;
        }

        /// <summary>
        /// Gets a DateTime representing the first specified day in the current month
        /// </summary>
        /// <param name="current">The current day</param>
        /// <param name="dayOfWeek">The current day of week</param>
        /// <returns></returns>
        public static DateTime First(this DateTime current, DayOfWeek dayOfWeek)
        {
            DateTime first = current.First();

            if (first.DayOfWeek != dayOfWeek)
            {
                first = first.Next(dayOfWeek);
            }

            return first;
        }

        /// <summary>
        /// Gets a DateTime representing the last day in the current month
        /// </summary>
        /// <param name="current">The current date</param>
        /// <returns></returns>
        public static DateTime Last(this DateTime current)
        {
            int daysInMonth = DateTime.DaysInMonth(current.Year, current.Month);

            DateTime last = current.First().AddDays(daysInMonth - 1);
            return last;
        }

        /// <summary>
        /// Gets a DateTime representing the last specified day in the current month
        /// </summary>
        /// <param name="current">The current date</param>
        /// <param name="dayOfWeek">The current day of week</param>
        /// <returns></returns>
        public static DateTime Last(this DateTime current, DayOfWeek dayOfWeek)
        {
            DateTime last = current.Last();

            last = last.AddDays(Math.Abs(dayOfWeek - last.DayOfWeek) * -1);
            return last;
        }

        /// <summary>
        /// Gets a DateTime representing the first date following the current date which falls on the given day of the week
        /// </summary>
        /// <param name="current">The current date</param>
        /// <param name="dayOfWeek">The day of week for the next date to get</param>
        public static DateTime Next(this DateTime current, DayOfWeek dayOfWeek)
        {
            int offsetDays = dayOfWeek - current.DayOfWeek;

            if (offsetDays <= 0)
            {
                offsetDays += 7;
            }

            DateTime result = current.AddDays(offsetDays);
            return result;
        }

        /// <summary>
        /// Accounts for once the date1 is past date2.
        /// </summary>
        public static bool WithinOneDayOrPast(this DateTime? date1, DateTime date2)
        {
            if (date1.HasValue)
            {
                DateTime first = DateTime.Parse($"{date1}");
                if (first < date2) // Account for past-due amounts.
                {
                    return true;
                }
                else
                {
                    TimeSpan difference = first - date2;
                    return Math.Abs(difference.TotalDays) <= 1.0;
                }
            }
            return false;
        }

        /// <summary>
        /// Only accounts for date1 being within range of date2.
        /// </summary>
        public static bool WithinOneDay(this DateTime? date1, DateTime date2)
        {
            if (date1.HasValue)
            {
                TimeSpan difference = DateTime.Parse($"{date1}") - date2;
                return Math.Abs(difference.TotalDays) <= 1.0;
            }
            return false;
        }

        /// <summary>
        /// Only accounts for date1 being within range of date2 by some amount.
        /// </summary>
        public static bool WithinAmountOfDays(this DateTime? date1, DateTime date2, double days)
        {
            if (date1.HasValue)
            {
                TimeSpan difference = DateTime.Parse($"{date1}") - date2;
                return Math.Abs(difference.TotalDays) <= days;
            }
            return false;
        }

        public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
        {
            return val.CompareTo(min) < 0 ? min : (val.CompareTo(max) > 0 ? max : val);
        }
    }

    public static class DateMath
    {
        #region Public Methods

        public static DateTime Max(DateTime d1, DateTime d2)
        {
            if (d1 > d2) return d1; else return d2;
        }

        public static DateTime Min(DateTime d1, DateTime d2)
        {
            if (d1 < d2) return d1; else return d2;
        }

        #endregion Public Methods
    }
}
