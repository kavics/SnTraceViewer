using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SenseNet.Diagnostics.Analysis;

namespace SnTraceViewerTests
{
    [TestClass]
    public class DateTimeToDisplayTests
    {
        [TestMethod]
        public void DateTime_ISO()
        {
            ToDisplayTest("2018-05-06 11:12:13.123456", null, "2018-05-06 11:12:13.12345");
        }
        [TestMethod]
        public void DateTime_ISO_AddMillisecondsRounds()
        {
            var now = new DateTime(2018, 05, 06, 11, 12, 13);
            var d = now.AddMilliseconds(123.456);

            var actual = d.ToDisplayString();

            Assert.AreEqual("2018-05-06 11:12:13.12300", actual);
        }
        [TestMethod]
        public void DateTime_TodayOneHourBefore()
        {
            ToDisplayTest("2018-05-06 11:12:13.123456", "2018-05-06 10:12:13.123456", "Today 11:12:13.12345");
        }
        [TestMethod]
        public void DateTime_YesterdayOneHourBefore()
        {
            ToDisplayTest("2018-05-05 10:12:13.123456", "2018-05-06 11:12:13.123456", "Yesterday 10:12:13.12345");
        }
        [TestMethod]
        public void DateTime_ThisWeek()
        {
            ToDisplayTest("2018-05-08 10:12:13.123456", "2018-05-10 11:12:13.123456", "Tuesday 10:12:13.12345");
            ToDisplayTest("2018-05-07 10:12:13.123456", "2018-05-10 11:12:13.123456", "Monday 10:12:13.12345");
            ToDisplayTest("2018-05-06 10:12:13.123456", "2018-05-10 11:12:13.123456", "Sunday 10:12:13.12345");
            ToDisplayTest("2018-05-05 10:12:13.123456", "2018-05-10 11:12:13.123456", "Saturday 10:12:13.12345");
            ToDisplayTest("2018-05-04 10:12:13.123456", "2018-05-10 11:12:13.123456", "Friday 10:12:13.12345");
        }
        [TestMethod]
        public void DateTime_ThisMonth()
        {
            ToDisplayTest("2018-05-03 10:12:13.123456", "2018-05-10 11:12:13.123456", "03 10:12:13.12345");
            ToDisplayTest("2018-05-02 10:12:13.123456", "2018-05-10 11:12:13.123456", "02 10:12:13.12345");
            ToDisplayTest("2018-05-01 10:12:13.123456", "2018-05-10 11:12:13.123456", "01 10:12:13.12345");
        }
        [TestMethod]
        public void DateTime_LastMonth()
        {
            ToDisplayTest("2018-04-30 10:12:13.123456", "2018-05-10 11:12:13.123456", "Last month 30 10:12:13.12345");
            ToDisplayTest("2018-04-29 10:12:13.123456", "2018-05-10 11:12:13.123456", "Last month 29 10:12:13.12345");
            ToDisplayTest("2018-04-01 10:12:13.123456", "2018-05-10 11:12:13.123456", "Last month 01 10:12:13.12345");
        }
        [TestMethod]
        public void DateTime_ThisYear()
        {
            ToDisplayTest("2018-03-31 10:12:13.123456", "2018-05-10 11:12:13.123456", "03-31 10:12:13.12345");
            ToDisplayTest("2018-01-01 10:12:13.123456", "2018-05-10 11:12:13.123456", "01-01 10:12:13.12345");
        }
        [TestMethod]
        public void DateTime_LastYear()
        {
            ToDisplayTest("2017-12-31 10:12:13.123456", "2018-05-10 11:12:13.123456", "2017-12-31 10:12:13.12345");
        }

        private void ToDisplayTest(string dateTime, string now, string expected)
        {
            var d = DateTime.Parse(dateTime);

            var actual = now == null
                ? d.ToDisplayString()
                : d.ToDisplayString(DateTime.Parse(now));

            Assert.AreEqual(expected, actual);
        }
    }
}
