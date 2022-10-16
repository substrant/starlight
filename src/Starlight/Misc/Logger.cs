using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using System;

namespace Starlight.Misc
{
    public class Logger
    {
        public static string LogFile { get; protected set; }

        public static void Init(bool verbose)
        {
            var hierarchy = (Hierarchy)LogManager.GetRepository();

            var patternLayout = new PatternLayout { ConversionPattern = "%date [%thread] %-5level %logger - %message%newline" };
            patternLayout.ActivateOptions();

            var roller = new RollingFileAppender
            {
                AppendToFile = false,
                File = LogFile = $"Logs/Starlight-{DateTime.Now:MM-dd-yyyy-HH-mm-ss}.txt",
                Layout = patternLayout,
                MaxSizeRollBackups = 5,
                MaximumFileSize = "10MB",
                RollingStyle = RollingFileAppender.RollingMode.Size,
                StaticLogFileName = true
            };
            roller.ActivateOptions();
            hierarchy.Root.AddAppender(roller);

            var memory = new MemoryAppender();
            memory.ActivateOptions();
            hierarchy.Root.AddAppender(memory);

            hierarchy.Root.Level = verbose ? Level.Debug : Level.Info;
            hierarchy.Configured = true;
        }
    }
}