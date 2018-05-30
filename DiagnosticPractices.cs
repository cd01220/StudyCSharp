namespace StudyCSharp
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Tracing;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// the DiagnosticPractices class.
    /// </summary>
    public class DiagnosticPractices
    {
        /// <summary>
        /// Enum of Colors.
        /// </summary>
        public enum MyColor
        {
            Red, Yellow, Blue
        }

        /// <summary>
        /// Debugging entrance.
        /// </summary>
        public static void TestDiagnostic()
        {
            TestDiagnostic05();
        }

        /// <summary>
        /// Writes a message to the trace listeners in the Listeners collection (just output window for now).
        /// </summary>
        public static void TestDiagnostic01()
        {
            Trace.WriteLine("Hello world!");
            Console.WriteLine(string.Empty);
        }

        /// <summary>
        /// Write log into Windows event logs.
        /// https://msdn.microsoft.com/zh-cn/library/system.diagnostics.eventlog(v=vs.110).aspx
        /// </summary>
        public static void TestDiagnostic02()
        {
            // Create the source, if it does not already exist.
            if (!EventLog.SourceExists("MySource"))
            {
                // An event log source should not be created and immediately used.
                // There is a latency time to enable the source, it should be created
                // prior to executing the application that uses the source.
                // Execute this sample a second time to use the new source.
                EventLog.CreateEventSource("MySource", "MyNewLog");
                Console.WriteLine("CreatedEventSource");
                Console.WriteLine("Exiting, execute the application a second time to use the source.");
                /* The source is created.  Exit the application to allow it to be registered. */
                return;
            }

            // Create an EventLog instance and assign its source.
            using (EventLog myLog = new EventLog("MyNewLog", ".", "MySource"))
            {
                myLog.WriteEntry("Writing to event log.");
                myLog.Dispose();
            }
            
            Console.WriteLine(string.Empty);
        }

        /// <summary>
        /// EventSource example 01.
        /// https://msdn.microsoft.com/zh-cn/library/system.diagnostics.tracing.eventsource(v=vs.110).aspx
        /// </summary>
        public static void TestDiagnostic03()
        {
            string name = MyCompanyEventSource01.GetName(typeof(MyCompanyEventSource01));
            IEnumerable<EventSource> eventSources = MyCompanyEventSource01.GetSources();
            MyCompanyEventSource01.Log.Startup();
            // ...
            MyCompanyEventSource01.Log.OpenFileStart("SomeFile");
            // ...
            MyCompanyEventSource01.Log.OpenFileStop();

            Debugger.Break();
        }

        public sealed class MyCompanyEventSource01 : EventSource
        {
            public static MyCompanyEventSource01 Log = new MyCompanyEventSource01();

            public void Startup() { WriteEvent(1); }
            public void OpenFileStart(string fileName) { WriteEvent(2, fileName); }
            public void OpenFileStop() { WriteEvent(3); }
        }

        /// <summary>
        /// EventSource example 02.
        /// https://msdn.microsoft.com/zh-cn/library/system.diagnostics.tracing.eventsource(v=vs.110).aspx
        /// </summary>
        public static void TestDiagnostic04()
        {
            MyCompanyEventSource02.Log.Startup();
            Console.WriteLine("Starting up");

            MyCompanyEventSource02.Log.DBQueryStart("Select * from MYTable");
            var url = "http://localhost";
            for (int i = 0; i < 10; i++)
            {
                MyCompanyEventSource02.Log.PageStart(i, url);
                MyCompanyEventSource02.Log.Mark(i);
                MyCompanyEventSource02.Log.PageStop(i);
            }
            MyCompanyEventSource02.Log.DBQueryStop();
            MyCompanyEventSource02.Log.LogColor(MyColor.Blue);

            MyCompanyEventSource02.Log.Failure("This is a failure 1");
            MyCompanyEventSource02.Log.Failure("This is a failure 2");
            MyCompanyEventSource02.Log.Failure("This is a failure 3");

            Debugger.Break();
        }

        /// <summary>
        /// the MyCompanyEventSource02 class. 
        /// </summary>
        [EventSource(Name = "MyCompany")]
        public sealed class MyCompanyEventSource02 : EventSource
        {
            [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed.")]
            [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed.")]
            public static MyCompanyEventSource02 Log = new MyCompanyEventSource02();

            [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed.")]
            [Event(1, Message = "Application Failure: {0}", Level = EventLevel.Error, Keywords = Keywords.Diagnostic)]
            public void Failure(string message)
            {
                this.WriteEvent(1, message);
            }

            [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed.")]
            [Event(2, Message = "Starting up.", Keywords = Keywords.Perf, Level = EventLevel.Informational)]
            public void Startup()
            {
                this.WriteEvent(2);
            }

            [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed.")]
            [Event(3, Message = "loading page {1} activityID={0}", Opcode = EventOpcode.Start,
                Task = Tasks.Page, Keywords = Keywords.Page, Level = EventLevel.Informational)]
            public void PageStart(int id, string url)
            {
                if (this.IsEnabled())
                {
                    this.WriteEvent(3, id, url);
                }
            }

            [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed.")]
            [Event(4, Opcode = EventOpcode.Stop, Task = Tasks.Page, Keywords = Keywords.Page, Level = EventLevel.Informational)]
            public void PageStop(int id)
            {
                if (this.IsEnabled())
                {
                    this.WriteEvent(4, id);
                }
            }

            [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed.")]
            [Event(5, Opcode = EventOpcode.Start, Task = Tasks.DBQuery, Keywords = Keywords.DataBase, Level = EventLevel.Informational)]
            public void DBQueryStart(string sqlQuery)
            {
                this.WriteEvent(5, sqlQuery);
            }

            [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed.")]
            [Event(6, Opcode = EventOpcode.Stop, Task = Tasks.DBQuery, Keywords = Keywords.DataBase, Level = EventLevel.Informational)]
            public void DBQueryStop()
            {
                this.WriteEvent(6);
            }

            [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed.")]
            [Event(7, Level = EventLevel.Verbose, Keywords = Keywords.DataBase)]
            public void Mark(int id)
            {
                if (this.IsEnabled())
                {
                    this.WriteEvent(7, id);
                }
            }

            [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed.")]
            [Event(8)]
            public void LogColor(MyColor color)
            {
                this.WriteEvent(8, (int)color);
            }

            [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed.")]
            public class Tasks
            {
                public const EventTask Page = (EventTask)1;
                public const EventTask DBQuery = (EventTask)2;
            }

            [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed.")]
            public class Keywords
            {
                public const EventKeywords Page = (EventKeywords)1;
                public const EventKeywords DataBase = (EventKeywords)2;
                public const EventKeywords Diagnostic = (EventKeywords)4;
                public const EventKeywords Perf = (EventKeywords)8;
            }
        }

        /// <summary>
        /// DefaultTraceListener Class.
        /// https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.defaulttracelistener?view=netframework-4.6.1
        /// </summary>
        public static void TestDiagnostic05()
        {
            decimal possibilities;
            decimal iter;

            // Remove the original default trace listener.
            Trace.Listeners.RemoveAt(0);

            // Create and add a new default trace listener.
            DefaultTraceListener defaultListener;
            defaultListener = new DefaultTraceListener();
            Trace.Listeners.Add(defaultListener);

            // Assign the log file specification from the command line, if entered.
            defaultListener.LogFileName = "LogFile.txt";

            possibilities = 100;
            try
            {
                const decimal MAX_POSSIBILITIES = 99;
                if (possibilities < 0 || possibilities > MAX_POSSIBILITIES)
                {
                    throw new Exception(String.Format("The number of possibilities must " +
                        "be in the range 0..{0}.", MAX_POSSIBILITIES));
                }
            }
            catch (Exception ex)
            {
                string failMessage = String.Format("\"{0}\" " + "is not a valid number of possibilities.", possibilities);
                defaultListener.Fail(failMessage, ex.Message);
                if (!defaultListener.AssertUiEnabled)
                {
                    Console.WriteLine(failMessage + "\n" + ex.Message);
                }
                return;
            }

            for (iter = 0; iter <= possibilities; iter++)
            {
                decimal result;
                string binomial;

                // Compute the next binomial coefficient and handle all exceptions.
                try
                {
                    result = CalcBinomial(possibilities, iter);
                }
                catch (Exception ex)
                {
                    string failMessage = String.Format("An exception was raised when " +
                        "calculating Binomial( {0}, {1} ).", possibilities, iter);
                    defaultListener.Fail(failMessage, ex.Message);
                    if (!defaultListener.AssertUiEnabled)
                    {
                        Console.WriteLine(failMessage + "\n" + ex.Message);
                    }
                    return;
                }

                // Format the trace and console output.
                binomial = String.Format("Binomial( {0}, {1} ) = ", possibilities, iter);
                defaultListener.Write(binomial);
                defaultListener.WriteLine(result.ToString());
                Console.WriteLine("{0} {1}", binomial, result);
            }

            Debugger.Break();
        }

        public static decimal CalcBinomial(decimal possibilities, decimal outcomes)
        {

            // Calculate a binomial coefficient, and minimize the chance of overflow.
            decimal result = 1;
            decimal iter;
            for (iter = 1; iter <= possibilities - outcomes; iter++)
            {
                result *= outcomes + iter;
                result /= iter;
            }
            return result;
        }
    }
}
