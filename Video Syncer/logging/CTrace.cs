using Microsoft.Extensions.Logging;
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
        public static ILogger logger { get; set; }

        public static void WriteLine(string message)
        {
            if (logger == null)
                return;

            logger.LogDebug(tag + " " + GetFormattedDateTime() + ": " + message);
        }

        public static void TraceInformation(string message)
        {
            if (logger == null)
                return;

            logger.LogInformation(tag + " " + GetFormattedDateTime() + ": " + message);
        }

        public static void TraceWarning(string message)
        {
            if (logger == null)
                return;

            logger.LogWarning(tag + " " + GetFormattedDateTime() + ": " + message);
        }

        public static void TraceError(string message)
        {
            if (logger == null)
                return;

            logger.LogError(tag + " " + GetFormattedDateTime() + ": " + message);
        }

        protected static string GetFormattedDateTime()
        {
            return DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss");
        }
    }
}
