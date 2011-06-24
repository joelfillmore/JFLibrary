using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JFLibrary;

namespace JFLibraryTest
{
    /// <summary>
    /// Tests for DateSpan
    /// </summary>
    [TestClass]
    public class DateSpanTest
    {
        [TestMethod]
        public void SameYearTests()
        {
            Console.WriteLine("Same year tests");

            // same date
            TestDateSpan(
                new DateTime(2011, 1, 2),
                new DateTime(2011, 1, 2),
                days: 0);

            // one day
            TestDateSpan(
                new DateTime(2011, 1, 1),
                new DateTime(2011, 1, 2),
                days: 1);

            // one month
            TestDateSpan(
                new DateTime(2011, 1, 1),
                new DateTime(2011, 2, 1),
                months: 1);

            // several months
            TestDateSpan(
                new DateTime(2011, 1, 1),
                new DateTime(2011, 4, 10),
                months: 3,
                days: 9);

            // less than a month, across boundary
            TestDateSpan(
                new DateTime(2011, 1, 25),
                new DateTime(2011, 2, 1),
                days: 7);
        }

        [TestMethod]
        public void PreviousYearTests()
        {
            Console.WriteLine("Previous year tests");

            // one year
            TestDateSpan(
                new DateTime(2010, 1, 1),
                new DateTime(2011, 1, 1),
                years: 1);

            // more than a year
            TestDateSpan(
                new DateTime(2010, 2, 1),
                new DateTime(2011, 5, 5),
                years: 1,
                months: 3,
                days: 4);

            // less than a year, across boundary
            TestDateSpan(
                new DateTime(2010, 2, 1),
                new DateTime(2011, 1, 1),
                months: 11);

            // across month and year boundary
            TestDateSpan(
                new DateTime(2010, 1, 25),
                new DateTime(2011, 2, 1),
                years: 1,
                months: 0,
                days: 7);
        }

        [TestMethod]
        public void MultiYearTest()
        {
            Console.WriteLine("Multi-year tests");

            // two years
            TestDateSpan(
                new DateTime(2009, 1, 1),
                new DateTime(2011, 1, 1),
                years: 2);

            // across month and year boundary
            TestDateSpan(
                new DateTime(2009, 1, 25),
                new DateTime(2011, 2, 1),
                years: 2,
                months: 0,
                days: 7);
        }

        [TestMethod]
        public void LeapYearTest()
        {
            Console.WriteLine("Leap year tests");

            // three years with start as leap day
            TestDateSpan(
                new DateTime(2008, 2, 29),
                new DateTime(2011, 2, 28),
                years: 3);

            // between two leap years
            TestDateSpan(
                new DateTime(2008, 2, 29),
                new DateTime(2012, 2, 29),
                years: 4);

            // last day of month is bigger
            TestDateSpan(
                new DateTime(2007, 7, 30),
                new DateTime(2008, 2, 29),
                years: 0,
                months: 7,
                days: 0);

            // across month and year boundary
            TestDateSpan(
                new DateTime(2008, 2, 25),
                new DateTime(2011, 3, 1),
                years: 3,
                months: 0,
                days: 4);

            TestDateSpan(
                new DateTime(2011, 2, 25),
                new DateTime(2012, 3, 1),
                years: 1,
                months: 0,
                days: 5);
        }

        [TestMethod]
        public void July4Test()
        {
            DateTime today = new DateTime(2011, 6, 22);

            DateTime declarationOfIndependence = new DateTime(1776, 7, 4);
            TimeSpan ageTimeSpan = today.Subtract(declarationOfIndependence);
            Console.WriteLine("The US is {0} days old.", ageTimeSpan.TotalDays);
            // The US is 85819 days old.

            // flawed conversion to years
            Console.WriteLine(
                "{0} years old.  (incorrect)",
                Math.Floor((today - declarationOfIndependence).TotalDays / 365));
            // 235 years old. (incorrect)
        }


        [TestMethod]
        public void UserMembershipTest()
        {
            Console.WriteLine("User membership test");

            // user who joined 2 years ago
            DateTime joined = new DateTime(2008, 6, 22);
            DateTime today = new DateTime(2011, 6, 22);

            Console.WriteLine(
                "Joined {0} years ago.  (incorrect)",
                Math.Floor((today - joined).TotalDays / 365.2425));
            // Joined 2 years ago. (incorrect)

            TestDateSpan(joined, today, years: 3);
        }

        [TestMethod]
        public void IraqWarTest()
        {
            DateTime warBegan = new DateTime(2003, 3, 20);
            DateTime warEnded = new DateTime(2010, 8, 31);
            DateSpan warLength = new DateSpan(warBegan, warEnded);
            Console.WriteLine("The Iraq War lasted {0}.", warLength.ToString());
            //The Iraq War lasted 7 years, 5 months, 11 days.

            TestDateSpan(warBegan, warEnded, years: 7, months: 5, days: 11);
        }

        private static void TestDateSpan(
            DateTime startDate,
            DateTime endDate,
            int years = 0, int months = 0, int days = 0)
        {
            DateSpan diff = new DateSpan(startDate, endDate);
            Console.WriteLine(
                "{0} - {1} : {2}",
                startDate.ToShortDateString(),
                endDate.ToShortDateString(),
                diff.ToString());

            Assert.IsTrue(
                diff.Years == years && diff.Months == months && diff.Days == days,
                String.Format("FAILURE: expected {0} years, {1} months, {2} days", years, months, days));
        }
    }
}
