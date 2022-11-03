using System;
using System.Runtime.CompilerServices;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;

namespace Starlight.Misc;

public class Logger
{
    public static string LogFile { get; protected set; }

    public static void Init()
    {
        var hierarchy = (Hierarchy)LogManager.GetRepository();

        var patternLayout = new PatternLayout
            { ConversionPattern = "%date [%thread] %-5level %logger - %message%newline" };
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

        hierarchy.Root.Level = Level.Debug;
        hierarchy.Configured = true;
    }

    public static void Out(string message, Level level, [CallerMemberName] string methodName = "anonymous method")
    {
        var logger = LogManager.GetLogger(methodName);

        if (level == Level.Debug)
            logger.Debug(message);
        else if (level == Level.Info)
            logger.Info(message);
        else if (level == Level.Warn)
            logger.Warn(message);
        else if (level == Level.Error)
            logger.Error(message);
        else if (level == Level.Fatal)
            logger.Fatal(message);
        else
            throw new NotImplementedException();
    }

    public static void Out(string message, Exception inner, Level level,
        [CallerMemberName] string methodName = "anonymous method")
    {
        var logger = LogManager.GetLogger(methodName);

        if (level == Level.Debug)
            logger.Debug(message, inner);
        else if (level == Level.Info)
            logger.Info(message, inner);
        else if (level == Level.Warn)
            logger.Warn(message, inner);
        else if (level == Level.Error)
            logger.Error(message, inner);
        else if (level == Level.Fatal)
            logger.Fatal(message, inner);
        else
            throw new NotImplementedException();
    }
}