using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Syncd.logging
{
    public class LoggingHandler
    {
        public static ILoggerFactory LoggerFactory { get; } = Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
        {
            //builder.AddConfiguration(Configuration.GetSection("Logging"));
            builder.AddConsole();
            builder.AddDebug();
            builder.AddAzureWebAppDiagnostics();
            builder.AddApplicationInsights("ikey");
        });

        public static ILogger CreateLogger2<T>()
        {
            return LoggerFactory.CreateLogger<T>();
        }

        public static ILogger CreateLogger<T>()
        {
            return LoggerFactory.CreateLogger<T>();
        }
    }
}