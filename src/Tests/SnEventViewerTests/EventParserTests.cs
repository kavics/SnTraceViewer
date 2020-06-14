using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SnEventViewer;
using EventLogEntry = SnEventViewer.EventLogEntry;

namespace SnEventViewerTests
{
    [TestClass]
    public class EventParserTests
    {
        private class TestEventLogFile : EventLogFile
        {
            private string _src;

            public TestEventLogFile(string src) : base("asdf")
            {
                _src = src;
            }

            public override TextReader GetReader(string path)
            {
                return new StringReader(_src);
            }
        }



        [TestMethod]
        public void Event_ParseOne()
        {
            var src = @"Timestamp: 2020. 06. 10. 13:24:18
Message: Message1
Category: General
Priority: -1
EventId: 21
Severity: Information
Title: 
Machine: Machine1
Application Domain: AppDomain1
Process Id: 18748
Process Name: Process1
Managed Thread Id: 9
Thread Name: 
Extended Properties: Types - Type1, Type2
UserName - Startup
SpecialUserName - SYSTEM
WorkingMode - 
SnTrace - #538e5008-7642-436d-8053-a9e38e7f21dc
";
            // ACTION
            var entry = EventLogEntry.Parse(src);

            // ASSERT
            Assert.AreEqual(DateTime.Parse("2020-06-10 13:24:18"), entry.Timestamp);
            Assert.AreEqual("Message1", entry.Message);
            Assert.AreEqual("General", entry.Category);
            Assert.AreEqual(-1, entry.Priority);
            Assert.AreEqual(21, entry.EventId);
            Assert.AreEqual(TraceEventType.Information, entry.Severity);
            Assert.AreEqual("", entry.Title);
            Assert.AreEqual("Machine1", entry.Machine);
            Assert.AreEqual("AppDomain1", entry.AppDomain);
            Assert.AreEqual(18748, entry.ProcessId);
            Assert.AreEqual("Process1", entry.ProcessName);
            Assert.AreEqual(9, entry.ThreadId);
            Assert.AreEqual("", entry.ThreadName);
            Assert.AreEqual(5, entry.ExtendedProperties.Count);
            Assert.AreEqual("Type1, Type2", entry.ExtendedProperties["Types"]);
            Assert.AreEqual("Startup", entry.ExtendedProperties["UserName"]);
            Assert.AreEqual("SYSTEM", entry.ExtendedProperties["SpecialUserName"]);
            Assert.AreEqual("", entry.ExtendedProperties["WorkingMode"]);
            Assert.AreEqual("#538e5008-7642-436d-8053-a9e38e7f21dc", entry.ExtendedProperties["SnTrace"]);
        }
        [TestMethod]
        public void Event_ParseFile()
        {
            var src = @"Timestamp: 2020. 06. 10. 13:24:17
Message: Message1
Category: General
Priority: -1
EventId: 21
Severity: Information
Title: 
Machine: Machine1
Application Domain: AppDomain1
Process Id: 18748
Process Name: Process1
Managed Thread Id: 9
Thread Name: 
Extended Properties: UserName - Startup
SpecialUserName - SYSTEM
WorkingMode - 
SnTrace - #538e5008-7642-436d-8053-a9e38e7f21dc

Timestamp: 2020. 06. 10. 13:24:18
Message: Message2
Category: General
Priority: -1
EventId: 21
Severity: Information
Title: 
Machine: Machine1
Application Domain: AppDomain1
Process Id: 18748
Process Name: Process1
Managed Thread Id: 9
Thread Name: 
Extended Properties: UserName - Startup
SpecialUserName - SYSTEM
WorkingMode - 
SnTrace - #538e5008-7642-436d-8053-a9e38e7f21dc

Timestamp: 2020. 06. 10. 13:24:19
Message: Message3
Category: General
Priority: -1
EventId: 21
Severity: Information
Title: 
Machine: Machine1
Application Domain: AppDomain1
Process Id: 18748
Process Name: Process1
Managed Thread Id: 9
Thread Name: 
Extended Properties: UserName - Startup
SpecialUserName - SYSTEM
WorkingMode - 
SnTrace - #538e5008-7642-436d-8053-a9e38e7f21dc
";

            // ACTION
            var file = new TestEventLogFile(src);
            file.Scan();

            // ASSERT
            var entries = file.ToArray();
            Assert.AreEqual(3, entries.Length);

            for (int i = 0; i < 3; i++)
            {
                var entry = entries[i];

                Assert.AreEqual(DateTime.Parse($"2020-06-10 13:24:1{(7 + i)}"), entry.Timestamp);
                Assert.AreEqual($"Message{(i + 1)}", entry.Message);
                Assert.AreEqual("General", entry.Category);
                Assert.AreEqual(-1, entry.Priority);
                Assert.AreEqual(21, entry.EventId);
                Assert.AreEqual(TraceEventType.Information, entry.Severity);
                Assert.AreEqual("", entry.Title);
                Assert.AreEqual("Machine1", entry.Machine);
                Assert.AreEqual("AppDomain1", entry.AppDomain);
                Assert.AreEqual(18748, entry.ProcessId);
                Assert.AreEqual("Process1", entry.ProcessName);
                Assert.AreEqual(9, entry.ThreadId);
                Assert.AreEqual("", entry.ThreadName);
                Assert.AreEqual(4, entry.ExtendedProperties.Count);
                Assert.AreEqual("Startup", entry.ExtendedProperties["UserName"]);
                Assert.AreEqual("SYSTEM", entry.ExtendedProperties["SpecialUserName"]);
                Assert.AreEqual("", entry.ExtendedProperties["WorkingMode"]);
                Assert.AreEqual("#538e5008-7642-436d-8053-a9e38e7f21dc", entry.ExtendedProperties["SnTrace"]);
            }
        }
    }
}
