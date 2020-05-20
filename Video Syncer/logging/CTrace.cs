using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Video_Syncer.logging
{
    public class CTrace
    {
        protected static readonly string tag = "[VSY]";

        public static void WriteLine(string message)
        {
            Trace.WriteLine(tag + " " + GetFormattedDateTime() + ": " + message);
        }

        public static void TraceInformation(string message)
        {
            Trace.TraceInformation(tag + " " + GetFormattedDateTime() + ": " + message);
        }

        public static void TraceWarning(string message)
        {
            Trace.TraceWarning(tag + " " + GetFormattedDateTime() + ": " + message);
        }

        public static void TraceError(string message)
        {
            Trace.TraceError(tag + " " + GetFormattedDateTime() + ": " + message);
        }

        protected static string GetFormattedDateTime()
        {
            return DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss");
        }
    }
}
