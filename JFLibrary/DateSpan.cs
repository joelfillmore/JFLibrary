using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace JFLibrary
{
    public struct DateSpan
    {
        private const int MONTHS_PER_YEAR = 12;

        private ushort _years;
        private byte _months;
        private byte _days;

        public DateSpan(DateTime startDate, DateTime endDate)
        {
            if (endDate < startDate)
                throw new ArgumentOutOfRangeException("endDate must occur after startDate");

            // initially assume that the end year, month, and day are >= start values
            int years = endDate.Year - startDate.Year;
            int months = endDate.Month - startDate.Month;
            int days = endDate.Day - startDate.Day;

            // adjust if the end month occurs before the start month, ex: April 2009 - Feb 2010
            if (endDate.Month < startDate.Month)
            {
                years--;
                months += MONTHS_PER_YEAR;
            }

            // adjust if the end day occurs before the start day, ex: April 30 - May 25
            if (endDate.Day < startDate.Day)
            {
                // The last day of the month is equal to any larger day following the
                // behavior of DateTime.AddMonths: March 31 + 1 month = April 30
                int endMonthDays = DateTime.DaysInMonth(endDate.Year, endDate.Month);
                if (endDate.Day == endMonthDays && startDate.Day > endMonthDays)
                {
                    days = 0;
                }
                else
                {
                    // remove one month
                    if (months > 0)
                    {
                        months--;
                    }
                    else
                    {
                        years--;
                        months = MONTHS_PER_YEAR - 1;
                    }

                    // get the number of days in the previous month
                    int previousMonthDays = (endDate.Month > 1)
                        ? DateTime.DaysInMonth(endDate.Year, endDate.Month - 1)
                        : DateTime.DaysInMonth(endDate.Year - 1, MONTHS_PER_YEAR);

                    // sum remaining days in previous month with the days in the current month
                    days = previousMonthDays - Math.Min(startDate.Day, previousMonthDays) + endDate.Day;
                }
            }

            _years = (ushort)years;
            _months = (byte)months;
            _days = (byte)days;
        }

        public int Years
        {
            get { return _years; }
        }

        public int Months
        {
            get { return _months; }
        }

        public int Days
        {
            get { return _days; }
        }

        /// <summary>
        ///   Prints all non-zero portions including years, months, and days
        /// </summary>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            PrintYears(sb);
            PrintMonths(sb);
            PrintDays(sb);
            return sb.ToString();
        }

        /// <summary>
        ///   Prints portions of the DateSpan that contribute at least 25% of the total duration
        /// </summary>
        public string ToShortString()
        {
            const double MINIMUM_PERCENTAGE = 0.25;

            // it doesn't matter if these values are not exact for this
            // specifc DateSpan, we only use them to find out if the years
            // months, or days are significant contributors to overall time.
            double yearDays = this.Years * 365.2425;
            double monthDays = this.Months * 30.43684;
            double totalDays = yearDays + monthDays + this.Days;

            StringBuilder sb = new StringBuilder();

            // always print years (if not zero)
            PrintYears(sb);

            // print months if significant
            if (monthDays / totalDays >= MINIMUM_PERCENTAGE)
            {
                PrintMonths(sb);
            }

            // print days if significant or total duration is zero
            if (totalDays == 0 || (double)this.Days / totalDays > MINIMUM_PERCENTAGE)
            {
                PrintDays(sb);
            }

            return sb.ToString();
        }

        private void PrintYears(StringBuilder sb)
        {
            if (this.Years > 0)
            {
                sb.AppendFormat("{0} {1}", this.Years, this.Years == 1 ? "year" : "years");
            }
        }

        private void PrintMonths(StringBuilder sb)
        {
            if (this.Months > 0)
            {
                if (sb.Length > 0)
                    sb.Append(", ");

                sb.AppendFormat("{0} {1}", this.Months, this.Months == 1 ? "month" : "months");
            }
        }

        private void PrintDays(StringBuilder sb)
        {
            if (this.Days > 0 || sb.Length == 0)
            {
                if (sb.Length > 0)
                    sb.Append(", ");

                sb.AppendFormat("{0} {1}", this.Days, this.Days == 1 ? "day" : "days");
            }
        }
    }

}
